using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Features.Auth.Interfaces;
using GdscSharingPlatform.Application.Features.Auth.Models;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Infrastructure.Identity.Options;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.Infrastructure.Identity.Services;

public sealed class AuthService : IAuthService
{
    private const string InvalidCredentialsMessage =
        "Invalid email or password.";

    private const string InvalidRefreshTokenMessage =
        "Invalid refresh token.";

    private const string TokenType = "Bearer";

    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenGenerator tokenGenerator,
        IOptions<JwtOptions> jwtOptions,
        ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenGenerator = tokenGenerator;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Chỉ chuẩn hóa email, tuyệt đối không trim password.
        var normalizedEmail = request.Email
            .Trim()
            .ToLowerInvariant();

        var user = await _userManager.FindByEmailAsync(
            normalizedEmail);

        if (user is null ||
            user.IsDeleted ||
            user.Status != UserStatus.Active)
        {
            _logger.LogWarning(
                "Authentication failed for an invalid or inactive account.");

            throw new AuthenticationException(
                InvalidCredentialsMessage);
        }

        var passwordResult =
            await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

        if (!passwordResult.Succeeded)
        {
            _logger.LogWarning(
                "Authentication failed for UserId {UserId}. " +
                "LockedOut: {LockedOut}.",
                user.Id,
                passwordResult.IsLockedOut);

            throw new AuthenticationException(
                InvalidCredentialsMessage);
        }

        await _dbContext.Entry(user)
            .Reference(currentUser => currentUser.Department)
            .LoadAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);

        var tokenPair = CreateTokenPair(
            user,
            roles,
            ipAddress,
            userAgent);

        user.LastLoginAt = DateTimeOffset.UtcNow;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.RefreshTokens.Add(
            tokenPair.RefreshTokenEntity);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "User {UserId} logged in successfully.",
            user.Id);

        var currentUser = MapCurrentUser(
            user,
            roles);

        return new AuthResponse(
            AccessToken: tokenPair.AccessToken,
            RefreshToken: tokenPair.RawRefreshToken,
            TokenType: TokenType,
            ExpiresIn: tokenPair.ExpiresInSeconds,
            User: currentUser);
    }

    public async Task<TokenResponse> RefreshTokenAsync(
        string refreshToken,
        string? ipAddress,
        string? userAgent,

        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new AuthenticationException(
                InvalidRefreshTokenMessage);
        }

        var tokenHash = _tokenGenerator.HashToken(
            refreshToken);

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var storedToken = await _dbContext.RefreshTokens
            .Include(token => token.User)
            .ThenInclude(user => user.Department)
            .SingleOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        if (storedToken is null)
        {
            _logger.LogWarning(
                "Refresh attempt used an unknown token hash.");

            throw new AuthenticationException(
                InvalidRefreshTokenMessage);
        }

        if (storedToken.IsRevoked)
        {
            await RevokeAllActiveSessionsAsync(
                storedToken.UserId,
                "Refresh token reuse detected",
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            _logger.LogWarning(
                "Refresh token reuse detected for UserId {UserId}. " +
                "All active sessions were revoked.",
                storedToken.UserId);

            throw new AuthenticationException(
                InvalidRefreshTokenMessage);
        }

        var utcNow = DateTimeOffset.UtcNow;
        var user = storedToken.User;

        var isLockedOut =
            await _userManager.IsLockedOutAsync(user);

        if (storedToken.ExpiresAt <= utcNow ||
            user.IsDeleted ||
            user.Status != UserStatus.Active ||
            isLockedOut)
        {
            _logger.LogWarning(
                "Refresh token rejected for UserId {UserId}.",
                storedToken.UserId);

            throw new AuthenticationException(
                InvalidRefreshTokenMessage);
        }

        var roles = await _userManager.GetRolesAsync(user);

        var newRawRefreshToken =
            _tokenGenerator.GenerateRefreshToken();

        var newTokenHash =
            _tokenGenerator.HashToken(
                newRawRefreshToken);

        var accessToken =
            _tokenGenerator.GenerateAccessToken(
                user.Id,
                user.Email ?? string.Empty,
                user.FullName,
                roles,
                user.DepartmentId,
                user.Status.ToString(),
                user.TokenVersion);

        /*
         * Atomic conditional update:
         * chỉ một request đồng thời được phép revoke token cũ.
         */
        var updatedRows = await _dbContext.RefreshTokens
            .Where(token =>
                token.Id == storedToken.Id &&
                !token.IsRevoked)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        token => token.IsRevoked,
                        true)
                    .SetProperty(
                        token => token.RevokedAt,
                        utcNow)
                    .SetProperty(
                        token => token.RevocationReason,
                        "Token rotation")
                    .SetProperty(
                        token => token.ReplacedByTokenHash,
                        newTokenHash),
                cancellationToken);

        if (updatedRows == 0)
        {
            await RevokeAllActiveSessionsAsync(
                storedToken.UserId,
                "Concurrent refresh or token reuse detected",
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            _logger.LogWarning(
                "Concurrent refresh or token reuse detected " +
                "for UserId {UserId}.",
                storedToken.UserId);

            throw new AuthenticationException(
                InvalidRefreshTokenMessage);
        }

        var newRefreshToken = new RefreshToken(
            userId: user.Id,
            tokenHash: newTokenHash,
            createdAt: utcNow,
            expiresAt: utcNow.AddDays(
                _jwtOptions.RefreshTokenExpirationDays),
            createdByIp: ipAddress,
            userAgent: userAgent);

        _dbContext.RefreshTokens.Add(
            newRefreshToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        _logger.LogInformation(
            "Refresh token rotated successfully for UserId {UserId}.",
            user.Id);

        return new TokenResponse(
            AccessToken: accessToken.Token,
            RefreshToken: newRawRefreshToken,
            TokenType: TokenType,
            ExpiresIn: accessToken.ExpiresInSeconds);
    }

    public async Task LogoutAsync(
        string refreshToken,
        Guid? currentUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            // Idempotent logout.
            return;
        }

        var tokenHash = _tokenGenerator.HashToken(
            refreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        if (storedToken is null)
        {
            // Không tiết lộ token có tồn tại hay không.
            return;
        }

        if (currentUserId.HasValue &&
            storedToken.UserId != currentUserId.Value)
        {
            _logger.LogWarning(
                "User {CurrentUserId} attempted to revoke " +
                "a session belonging to another user.",
                currentUserId.Value);

            throw new AuthenticationException(
                InvalidRefreshTokenMessage);
        }

        if (storedToken.IsRevoked)
        {
            // Idempotent: token đã revoke thì không làm gì.
            return;
        }

        storedToken.Revoke(
            revokedAt: DateTimeOffset.UtcNow,
            reason: "User logout");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "User {UserId} logged out from one session.",
            storedToken.UserId);
    }

    public async Task LogoutAllAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new AuthenticationException(
                "Authenticated user is required.");
        }

        var userExists = await _dbContext.Users
            .AnyAsync(
                user => user.Id == userId,
                cancellationToken);

        if (!userExists)
        {
            throw new AuthenticationException(
                "Authenticated user is invalid.");
        }

        var utcNow = DateTimeOffset.UtcNow;

        await _dbContext.RefreshTokens
            .Where(token =>
                token.UserId == userId &&
                !token.IsRevoked &&
                token.ExpiresAt > utcNow)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        token => token.IsRevoked,
                        true)
                    .SetProperty(
                        token => token.RevokedAt,
                        utcNow)
                    .SetProperty(
                        token => token.RevocationReason,
                        "User logout from all devices"),
                cancellationToken);

        /*
         * Tăng TokenVersion để những token được tạo trước đó
         * có thể bị từ chối khi Phase 4 kiểm tra token version.
         */
        await _dbContext.Users
            .Where(user => user.Id == userId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        user => user.TokenVersion,
                        user => user.TokenVersion + 1)
                    .SetProperty(
                        user => user.UpdatedAt,
                        utcNow),
                cancellationToken);

        _logger.LogInformation(
            "All active sessions were revoked for UserId {UserId}.",
            userId);
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new AuthenticationException(
                "Authenticated user is required.");
        }

        var user = await _dbContext.Users
            .Include(currentUser => currentUser.Department)
            .SingleOrDefaultAsync(
                currentUser => currentUser.Id == userId,
                cancellationToken);

        if (user is null ||
            user.IsDeleted ||
            user.Status != UserStatus.Active)
        {
            throw new NotFoundException("User", userId);
        }

        var roles = await _userManager.GetRolesAsync(user);

        return MapCurrentUser(
            user,
            roles);
    }

    private TokenPair CreateTokenPair(
        ApplicationUser user,
        IEnumerable<string> roles,
        string? ipAddress,
        string? userAgent)
    {
        var accessToken =
            _tokenGenerator.GenerateAccessToken(
                user.Id,
                user.Email ?? string.Empty,
                user.FullName,
                roles,
                user.DepartmentId,
                user.Status.ToString(),
                user.TokenVersion);

        var rawRefreshToken =
            _tokenGenerator.GenerateRefreshToken();

        var refreshTokenHash =
            _tokenGenerator.HashToken(
                rawRefreshToken);

        var utcNow = DateTimeOffset.UtcNow;

        var refreshTokenEntity = new RefreshToken(
            userId: user.Id,
            tokenHash: refreshTokenHash,
            createdAt: utcNow,
            expiresAt: utcNow.AddDays(
                _jwtOptions.RefreshTokenExpirationDays),
            createdByIp: ipAddress,
            userAgent: userAgent);

        return new TokenPair(
            AccessToken: accessToken.Token,
            RawRefreshToken: rawRefreshToken,
            ExpiresInSeconds: accessToken.ExpiresInSeconds,
            RefreshTokenEntity: refreshTokenEntity);
    }

    private async Task RevokeAllActiveSessionsAsync(
        Guid userId,
        string reason,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTimeOffset.UtcNow;

        await _dbContext.RefreshTokens
            .Where(token =>
                token.UserId == userId &&
                !token.IsRevoked &&
                token.ExpiresAt > utcNow)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        token => token.IsRevoked,
                        true)
                    .SetProperty(
                        token => token.RevokedAt,
                        utcNow)
                    .SetProperty(
                        token => token.RevocationReason,
                        reason),
                cancellationToken);

        await _dbContext.Users
            .Where(user => user.Id == userId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        user => user.TokenVersion,
                        user => user.TokenVersion + 1)
                    .SetProperty(
                        user => user.UpdatedAt,
                        utcNow),
                cancellationToken);
    }

    private static CurrentUserDto MapCurrentUser(
        ApplicationUser user,
        IEnumerable<string> roles)
    {
        DepartmentDto? department = null;

        if (user.Department is not null)
        {
            department = new DepartmentDto(
                Id: user.Department.Id,
                Name: user.Department.Name);
        }

        return new CurrentUserDto(
            Id: user.Id,
            Email: user.Email ?? string.Empty,
            DisplayName: user.DisplayName ?? user.FullName,
            StudentCode: user.StudentCode,
            Generation: user.Generation,
            AvatarUrl: user.AvatarUrl,
            Status: user.Status.ToString(),
            Department: department,
            Roles: roles
                .Distinct(StringComparer.Ordinal)
                .ToArray());
    }

    private sealed record TokenPair(
        string AccessToken,
        string RawRefreshToken,
        int ExpiresInSeconds,
        RefreshToken RefreshTokenEntity);
}
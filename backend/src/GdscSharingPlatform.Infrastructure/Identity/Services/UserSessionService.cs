using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Features.Auth.Models;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Infrastructure.Identity.Options;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.Infrastructure.Identity.Services;

public sealed class UserSessionService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator tokenGenerator,
    IOptions<JwtOptions> jwtOptions
) : IUserSessionService
{
    public async Task<AuthResponse> CreateAsync(
        ApplicationUser user,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken
    )
    {
        if (user.IsDeleted ||
            user.Status != UserStatus.Active ||
            await userManager.IsLockedOutAsync(user))
        {
            throw new AuthenticationException(
                "Account cannot sign in"
            );
        }
        await dbContext.Entry(user)
            .Reference(current => current.Department)
            .LoadAsync(cancellationToken);
        
        var roles = await userManager.GetRolesAsync(user);

        var accessToken = tokenGenerator.GenerateAccessToken(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            roles,
            user.DepartmentId,
            user.Status.ToString(),
            user.TokenVersion
        );

        var rawRefreshToken = tokenGenerator.GenerateRefreshToken();
        var utcNow = DateTimeOffset.UtcNow;

        var refreshToken = new RefreshToken(
            userId: user.Id,
            tokenHash: tokenGenerator.HashToken(rawRefreshToken),
            createdAt: utcNow,
            expiresAt: utcNow.AddDays(
                jwtOptions.Value.RefreshTokenExpirationDays
            ),
            createdByIp: ipAddress,
            userAgent: userAgent
        );

        user.LastLoginAt = utcNow;
        user.UpdatedAt = utcNow;

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            AccessToken: accessToken.Token,
            RefreshToken: rawRefreshToken,
            TokenType: "Bearer",
            ExpiresIn: accessToken.ExpiresInSeconds,
            User: CurrentUserMapper.Map(user, roles)
        );
    }
}

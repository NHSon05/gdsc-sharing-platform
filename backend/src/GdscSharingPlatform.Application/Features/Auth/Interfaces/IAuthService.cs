using GdscSharingPlatform.Application.Features.Auth.Models;

namespace GdscSharingPlatform.Application.Features.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);

    Task<TokenResponse> RefreshTokenAsync(
        string refreshToken,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);

    Task LogoutAsync(
        string refreshToken,
        Guid? currentUserId,
        CancellationToken cancellationToken);

    Task LogoutAllAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<CurrentUserDto> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
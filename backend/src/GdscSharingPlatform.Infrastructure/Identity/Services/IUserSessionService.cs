using GdscSharingPlatform.Application.Features.Auth.Models;

namespace GdscSharingPlatform.Infrastructure.Identity.Services;

public interface IUserSessionService
{
    Task<AuthResponse> CreateAsync(
        ApplicationUser user,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);
}
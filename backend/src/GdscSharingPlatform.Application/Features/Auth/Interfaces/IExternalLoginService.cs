using GdscSharingPlatform.Application.Features.Auth.Models;

namespace GdscSharingPlatform.Application.Features.Auth.Interfaces;

public interface IExternalLoginService
{
    Task<AuthResponse> LoginAsync(
        VerifiedExternalIdentity identity,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken
    );
}

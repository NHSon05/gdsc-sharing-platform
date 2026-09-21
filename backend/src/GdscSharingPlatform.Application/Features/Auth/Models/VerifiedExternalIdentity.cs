namespace GdscSharingPlatform.Application.Features.Auth.Models;

public sealed record VerifiedExternalIdentity(
    string Provider,
    string Subject,
    string? Email,
    bool EmailVerified,
    string? DisplayName
);
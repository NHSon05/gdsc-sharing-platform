namespace GdscSharingPlatform.Application.Features.Profile.Models;

public sealed record UpdateProfileRequest(
    string? DisplayName = null,
    string? Email = null,
    string? PhoneNumber = null,
    string? StudentCode = null,
    string? GithubUrl = null,
    string? Bio = null);

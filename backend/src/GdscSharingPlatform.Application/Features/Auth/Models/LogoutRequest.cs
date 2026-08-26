namespace GdscSharingPlatform.Application.Features.Auth.Models;

public sealed record LogoutRequest(
    string RefreshToken
);
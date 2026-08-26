namespace GdscSharingPlatform.Application.Features.Auth.Models;

public sealed record LoginRequest(
    string Email,
    string Password
);

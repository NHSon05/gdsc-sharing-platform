namespace GdscSharingPlatform.Application.Features.Auth.Models;

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresIn);
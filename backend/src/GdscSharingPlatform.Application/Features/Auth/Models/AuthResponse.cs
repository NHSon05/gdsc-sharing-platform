namespace GdscSharingPlatform.Application.Features.Auth.Models;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresIn,
    CurrentUserDto User);
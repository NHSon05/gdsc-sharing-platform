namespace GdscSharingPlatform.Application.Features.Auth.Models;

public sealed record CurrentUserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? StudentCode,
    string? Generation,
    string? AvatarUrl,
    string Status,
    DepartmentDto? Department,
    IReadOnlyCollection<string> Roles);
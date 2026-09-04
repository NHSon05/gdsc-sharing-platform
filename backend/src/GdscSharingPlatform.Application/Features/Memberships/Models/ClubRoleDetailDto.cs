namespace GdscSharingPlatform.Application.Features.Memberships.Models;

public sealed record ClubRoleDetailDto(
    Guid Id,
    string Code,
    string Name,
    int Level,
    bool IsActive);

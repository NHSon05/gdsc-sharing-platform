namespace GdscSharingPlatform.Application.Features.Profile.Models;

public sealed record ProfileDto(
    Guid Id,
    string DisplayName,
    string Email,
    string? PhoneNumber,
    string? StudentCode,
    string? GithubUrl,
    string? Bio,
    string? AvatarUrl,
    string SystemRoles,
    IReadOnlyList<UserGenerationMembershipDto> Memberships,
    int ProfileCompletionPercentage,
    IReadOnlyList<string> MissingProfileFields,
    DateTimeOffset? UpdatedAtUtc);

public sealed record UserGenerationMembershipDto(
    Guid Id,
    UserGenerationSummaryDto Generation,
    bool IsActive,
    IReadOnlyList<UserDepartmentMembershipDto> Departments);

public sealed record UserGenerationSummaryDto(
    Guid Id,
    int Number,
    string Name);

public sealed record UserDepartmentMembershipDto(
    Guid Id,
    UserDepartmentSummaryDto Department,
    bool IsPrimary,
    IReadOnlyList<UserClubRoleDto> Roles);

public sealed record UserDepartmentSummaryDto(
    Guid Id,
    string Name,
    string Slug);

public sealed record UserClubRoleDto(
    Guid Id,
    string Code,
    string Name);

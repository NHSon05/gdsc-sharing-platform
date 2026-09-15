using GdscSharingPlatform.Domain.Enums;

namespace GdscSharingPlatform.Application.Features.Memberships.Models;

public sealed record AssignMemberToGenRequest(
    Guid GenerationId,
    DateOnly JoinedAt);

public sealed record AddMemberToDepartmentRequest(
    Guid DepartmentId,
    bool IsPrimary,
    IReadOnlyList<Guid> RoleIds);

public sealed record UpdateDepartmentMembershipRequest(
    bool IsPrimary,
    bool IsActive);

public sealed record ReplaceRolesRequest(
    IReadOnlyList<Guid> RoleIds);

public sealed record DepartmentMembershipSummaryDto(
    Guid Id,
    Guid ClubMembershipId,
    Guid DepartmentId,
    bool IsPrimary,
    bool IsActive,
    DateOnly? JoinedAt,
    DateOnly? LeftAt,
    IReadOnlyList<ClubRoleDetailDto> Roles);

public sealed record ClubMembershipSummaryDto(
    Guid Id,
    Guid UserId,
    Guid GenerationId,
    bool IsActive,
    DateOnly? JoinedAt,
    DateOnly? LeftAt);

public sealed record AdminMemberListItemDto(
    Guid Id,
    string Email,
    string FullName,
    string? DisplayName,
    string? StudentCode,
    string? AvatarUrl,
    UserStatus Status,
    IReadOnlyList<string> SystemRoles,
    IReadOnlyList<string> DepartmentNames,
    IReadOnlyList<int> GenerationNumbers,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastLoginAt);

public sealed record AdminMemberPageDto(
    IReadOnlyList<AdminMemberListItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed class AdminMemberListQuery
{
    public string? Search { get; set; }
    public Guid? GenerationId { get; set; }
    public Guid? DepartmentId { get; set; }
    public UserStatus? Status { get; set; }
    public string? SystemRole { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed record UpdateUserStatusRequest(UserStatus Status);

public sealed record UpdateUserSystemRolesRequest(IReadOnlyList<string> Roles);

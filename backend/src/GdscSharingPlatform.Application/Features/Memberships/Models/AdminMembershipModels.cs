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

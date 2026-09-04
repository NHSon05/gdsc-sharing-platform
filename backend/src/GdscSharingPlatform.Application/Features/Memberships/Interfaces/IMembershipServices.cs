using GdscSharingPlatform.Application.Features.Memberships.Models;

namespace GdscSharingPlatform.Application.Features.Memberships.Interfaces;

public interface ILookupService
{
    Task<IReadOnlyList<GenerationDto>> GetGenerationsAsync(
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DepartmentDetailDto>> GetDepartmentsAsync(
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClubRoleDetailDto>> GetClubRolesAsync(
        bool includeInactive,
        CancellationToken cancellationToken = default);
}

public interface IDepartmentService
{
    Task<DepartmentDetailDto> CreateDepartmentAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken = default);

    Task<DepartmentDetailDto> UpdateDepartmentAsync(
        Guid departmentId,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default);

    Task DeactivateDepartmentAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default);

    Task<DepartmentDetailDto> ActivateDepartmentAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default);
}

public interface IGenerationService
{
    Task<GenerationDto> CreateGenerationAsync(
        CreateGenerationRequest request,
        CancellationToken cancellationToken = default);

    Task<GenerationDto> UpdateGenerationAsync(
        Guid generationId,
        UpdateGenerationRequest request,
        CancellationToken cancellationToken = default);

    Task DeactivateGenerationAsync(
        Guid generationId,
        CancellationToken cancellationToken = default);
}

public interface IMemberMembershipService
{
    Task<ClubMembershipSummaryDto> AssignMemberToGenAsync(
        Guid userId,
        AssignMemberToGenRequest request,
        CancellationToken cancellationToken = default);

    Task<DepartmentMembershipSummaryDto> AddMemberToDepartmentAsync(
        Guid userId,
        Guid clubMembershipId,
        AddMemberToDepartmentRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    Task<DepartmentMembershipSummaryDto> UpdateDepartmentMembershipAsync(
        Guid userId,
        Guid departmentMembershipId,
        UpdateDepartmentMembershipRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClubRoleDetailDto>> ReplaceRolesAsync(
        Guid userId,
        Guid departmentMembershipId,
        ReplaceRolesRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    Task EndDepartmentMembershipAsync(
        Guid userId,
        Guid departmentMembershipId,
        CancellationToken cancellationToken = default);

    Task EndClubMembershipAsync(
        Guid userId,
        Guid clubMembershipId,
        CancellationToken cancellationToken = default);
}

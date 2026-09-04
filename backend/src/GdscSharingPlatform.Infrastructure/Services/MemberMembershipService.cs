using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Memberships.Interfaces;
using GdscSharingPlatform.Application.Features.Memberships.Models;
using GdscSharingPlatform.Domain.Memberships;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GdscSharingPlatform.Infrastructure.Services;

public sealed class MemberMembershipService : IMemberMembershipService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MemberMembershipService> _logger;

    public MemberMembershipService(
        ApplicationDbContext dbContext,
        ILogger<MemberMembershipService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ClubMembershipSummaryDto> AssignMemberToGenAsync(
        Guid userId,
        AssignMemberToGenRequest request,
        CancellationToken cancellationToken = default)
    {
        var userExists = await _dbContext.Users
            .AnyAsync(u => u.Id == userId, cancellationToken);
        if (!userExists)
        {
            throw new NotFoundException("User", userId);
        }

        var generation = await _dbContext.ClubGenerations
            .SingleOrDefaultAsync(g => g.Id == request.GenerationId, cancellationToken);
        if (generation is null)
        {
            throw new NotFoundException(nameof(ClubGeneration), request.GenerationId);
        }

        if (!generation.IsActive)
        {
            throw new ApplicationValidationException("generationId", "Cannot assign member to an inactive generation.");
        }

        var alreadyAssigned = await _dbContext.ClubMemberships
            .AnyAsync(cm => cm.UserId == userId && cm.GenerationId == request.GenerationId, cancellationToken);
        if (alreadyAssigned)
        {
            throw new ConflictException("Member is already assigned to this generation.");
        }

        var clubMembership = new ClubMembership(userId, request.GenerationId, request.JoinedAt);

        _dbContext.ClubMemberships.Add(clubMembership);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Assigned user {UserId} to Gen {GenNumber} ({GenId})", userId, generation.Number, generation.Id);

        return new ClubMembershipSummaryDto(
            clubMembership.Id,
            clubMembership.UserId,
            clubMembership.GenerationId,
            clubMembership.IsActive,
            clubMembership.JoinedAt,
            clubMembership.LeftAt);
    }

    public async Task<DepartmentMembershipSummaryDto> AddMemberToDepartmentAsync(
        Guid userId,
        Guid clubMembershipId,
        AddMemberToDepartmentRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var clubMembership = await _dbContext.ClubMemberships
            .SingleOrDefaultAsync(cm => cm.Id == clubMembershipId && cm.UserId == userId, cancellationToken);
        if (clubMembership is null)
        {
            throw new NotFoundException(nameof(ClubMembership), clubMembershipId);
        }

        if (!clubMembership.IsActive)
        {
            throw new ApplicationValidationException("clubMembershipId", "Club membership is inactive.");
        }

        var department = await _dbContext.Departments
            .SingleOrDefaultAsync(d => d.Id == request.DepartmentId, cancellationToken);
        if (department is null)
        {
            throw new NotFoundException("Department", request.DepartmentId);
        }

        if (!department.IsActive || department.IsDeleted)
        {
            throw new ApplicationValidationException("departmentId", "Cannot add member to an inactive department.");
        }

        var alreadyInDept = await _dbContext.DepartmentMemberships
            .AnyAsync(dm => dm.ClubMembershipId == clubMembershipId && dm.DepartmentId == request.DepartmentId && dm.IsActive, cancellationToken);
        if (alreadyInDept)
        {
            throw new ConflictException("Member is already active in this department for this generation.");
        }

        var targetRoleIds = request.RoleIds.Distinct().ToList();
        var roles = await _dbContext.ClubRoles
            .Where(r => targetRoleIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

        if (roles.Count != targetRoleIds.Count)
        {
            throw new NotFoundException(nameof(ClubRole), string.Join(",", targetRoleIds));
        }

        if (roles.Any(r => !r.IsActive))
        {
            throw new ApplicationValidationException("roleIds", "Cannot assign inactive roles.");
        }

        if (request.IsPrimary)
        {
            var siblingDepts = await _dbContext.DepartmentMemberships
                .Where(dm => dm.ClubMembershipId == clubMembershipId)
                .ToListAsync(cancellationToken);

            foreach (var sibling in siblingDepts)
            {
                sibling.SetPrimary(false);
            }
        }

        var deptMembership = new DepartmentMembership(clubMembershipId, request.DepartmentId, request.IsPrimary);
        _dbContext.DepartmentMemberships.Add(deptMembership);

        var assignments = new List<RoleAssignment>();
        foreach (var role in roles)
        {
            var assignment = new RoleAssignment(deptMembership.Id, role.Id, currentUserId);
            assignments.Add(assignment);
            _dbContext.RoleAssignments.Add(assignment);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added user {UserId} to department {DepartmentName} in ClubMembership {ClubMembershipId}", userId, department.Name, clubMembershipId);

        return new DepartmentMembershipSummaryDto(
            deptMembership.Id,
            deptMembership.ClubMembershipId,
            deptMembership.DepartmentId,
            deptMembership.IsPrimary,
            deptMembership.IsActive,
            deptMembership.JoinedAt,
            deptMembership.LeftAt,
            roles.Select(r => new ClubRoleDetailDto(r.Id, r.Code, r.Name, r.Level, r.IsActive)).ToList());
    }

    public async Task<DepartmentMembershipSummaryDto> UpdateDepartmentMembershipAsync(
        Guid userId,
        Guid departmentMembershipId,
        UpdateDepartmentMembershipRequest request,
        CancellationToken cancellationToken = default)
    {
        var deptMembership = await _dbContext.DepartmentMemberships
            .Include(dm => dm.ClubMembership)
            .Include(dm => dm.Department)
            .SingleOrDefaultAsync(dm => dm.Id == departmentMembershipId && dm.ClubMembership.UserId == userId, cancellationToken);

        if (deptMembership is null)
        {
            throw new NotFoundException(nameof(DepartmentMembership), departmentMembershipId);
        }

        if (request.IsPrimary && !deptMembership.IsPrimary)
        {
            var siblingDepts = await _dbContext.DepartmentMemberships
                .Where(dm => dm.ClubMembershipId == deptMembership.ClubMembershipId && dm.Id != departmentMembershipId)
                .ToListAsync(cancellationToken);

            foreach (var sibling in siblingDepts)
            {
                sibling.SetPrimary(false);
            }

            deptMembership.SetPrimary(true);
        }
        else if (!request.IsPrimary && deptMembership.IsPrimary)
        {
            deptMembership.SetPrimary(false);
        }

        if (request.IsActive != deptMembership.IsActive)
        {
            if (!request.IsActive)
            {
                deptMembership.End(DateOnly.FromDateTime(DateTime.UtcNow));

                var activeRoles = await _dbContext.RoleAssignments
                    .Where(ra => ra.DepartmentMembershipId == departmentMembershipId && ra.IsActive)
                    .ToListAsync(cancellationToken);

                foreach (var ra in activeRoles)
                {
                    ra.End();
                }
            }
            else
            {
                deptMembership.Reactivate();
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var roles = await _dbContext.RoleAssignments
            .Where(ra => ra.DepartmentMembershipId == departmentMembershipId && ra.IsActive)
            .Select(ra => ra.ClubRole)
            .Select(r => new ClubRoleDetailDto(r.Id, r.Code, r.Name, r.Level, r.IsActive))
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Updated department membership {DeptMembershipId} for user {UserId}", departmentMembershipId, userId);

        return new DepartmentMembershipSummaryDto(
            deptMembership.Id,
            deptMembership.ClubMembershipId,
            deptMembership.DepartmentId,
            deptMembership.IsPrimary,
            deptMembership.IsActive,
            deptMembership.JoinedAt,
            deptMembership.LeftAt,
            roles);
    }

    public async Task<IReadOnlyList<ClubRoleDetailDto>> ReplaceRolesAsync(
        Guid userId,
        Guid departmentMembershipId,
        ReplaceRolesRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var deptMembership = await _dbContext.DepartmentMemberships
            .Include(dm => dm.ClubMembership)
            .SingleOrDefaultAsync(dm => dm.Id == departmentMembershipId && dm.ClubMembership.UserId == userId, cancellationToken);

        if (deptMembership is null)
        {
            throw new NotFoundException(nameof(DepartmentMembership), departmentMembershipId);
        }

        if (!deptMembership.IsActive)
        {
            throw new ApplicationValidationException("departmentMembershipId", "Cannot modify roles of an inactive department membership.");
        }

        var targetRoleIds = request.RoleIds.Distinct().ToList();

        var targetRoles = await _dbContext.ClubRoles
            .Where(r => targetRoleIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

        if (targetRoles.Count != targetRoleIds.Count)
        {
            throw new NotFoundException(nameof(ClubRole), string.Join(",", targetRoleIds));
        }

        if (targetRoles.Any(r => !r.IsActive))
        {
            throw new ApplicationValidationException("roleIds", "Cannot assign inactive roles.");
        }

        // Execute in transaction
        var activeAssignments = await _dbContext.RoleAssignments
            .Where(ra => ra.DepartmentMembershipId == departmentMembershipId && ra.IsActive)
            .ToListAsync(cancellationToken);

        // 1. End assignments no longer in target list
        foreach (var assignment in activeAssignments)
        {
            if (!targetRoleIds.Contains(assignment.ClubRoleId))
            {
                assignment.End();
            }
        }

        // 2. Add new assignments for roles not already active
        var activeRoleIds = activeAssignments.Where(ra => ra.IsActive).Select(ra => ra.ClubRoleId).ToHashSet();
        foreach (var roleId in targetRoleIds)
        {
            if (!activeRoleIds.Contains(roleId))
            {
                var newAssignment = new RoleAssignment(departmentMembershipId, roleId, currentUserId);
                _dbContext.RoleAssignments.Add(newAssignment);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Replaced roles for DepartmentMembership {DeptMembershipId}, user {UserId}. Active role count: {Count}", departmentMembershipId, userId, targetRoles.Count);

        return targetRoles
            .OrderBy(r => r.Level)
            .Select(r => new ClubRoleDetailDto(r.Id, r.Code, r.Name, r.Level, r.IsActive))
            .ToList();
    }

    public async Task EndDepartmentMembershipAsync(
        Guid userId,
        Guid departmentMembershipId,
        CancellationToken cancellationToken = default)
    {
        var deptMembership = await _dbContext.DepartmentMemberships
            .Include(dm => dm.ClubMembership)
            .SingleOrDefaultAsync(dm => dm.Id == departmentMembershipId && dm.ClubMembership.UserId == userId, cancellationToken);

        if (deptMembership is null)
        {
            throw new NotFoundException(nameof(DepartmentMembership), departmentMembershipId);
        }

        deptMembership.End(DateOnly.FromDateTime(DateTime.UtcNow));

        var activeRoles = await _dbContext.RoleAssignments
            .Where(ra => ra.DepartmentMembershipId == departmentMembershipId && ra.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var ra in activeRoles)
        {
            ra.End();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ended department membership {DeptMembershipId} for user {UserId}", departmentMembershipId, userId);
    }

    public async Task EndClubMembershipAsync(
        Guid userId,
        Guid clubMembershipId,
        CancellationToken cancellationToken = default)
    {
        var clubMembership = await _dbContext.ClubMemberships
            .Include(cm => cm.DepartmentMemberships)
                .ThenInclude(dm => dm.RoleAssignments)
            .SingleOrDefaultAsync(cm => cm.Id == clubMembershipId && cm.UserId == userId, cancellationToken);

        if (clubMembership is null)
        {
            throw new NotFoundException(nameof(ClubMembership), clubMembershipId);
        }

        clubMembership.End(DateOnly.FromDateTime(DateTime.UtcNow));

        foreach (var dm in clubMembership.DepartmentMemberships)
        {
            if (dm.IsActive)
            {
                dm.End(DateOnly.FromDateTime(DateTime.UtcNow));
            }

            foreach (var ra in dm.RoleAssignments)
            {
                if (ra.IsActive)
                {
                    ra.End();
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ended club membership {ClubMembershipId} and all sub-assignments for user {UserId}", clubMembershipId, userId);
    }
}

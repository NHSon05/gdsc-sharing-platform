using FluentValidation;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Memberships.Interfaces;
using GdscSharingPlatform.Application.Features.Memberships.Models;
using GdscSharingPlatform.Application.Features.Profile.Interfaces;
using GdscSharingPlatform.Application.Features.Profile.Models;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.Api.Controllers;

[ApiController]
[Route("api/admin/members")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public sealed class AdminMemberMembershipsController : ControllerBase
{
    private readonly IMemberMembershipService _membershipService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<AssignMemberToGenRequest> _assignGenValidator;
    private readonly IValidator<AddMemberToDepartmentRequest> _addDeptValidator;
    private readonly IValidator<ReplaceRolesRequest> _replaceRolesValidator;
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IProfileService _profileService;

    public AdminMemberMembershipsController(
        IMemberMembershipService membershipService,
        ICurrentUserService currentUserService,
        IValidator<AssignMemberToGenRequest> assignGenValidator,
        IValidator<AddMemberToDepartmentRequest> addDeptValidator,
        IValidator<ReplaceRolesRequest> replaceRolesValidator,
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IProfileService profileService)
    {
        _membershipService = membershipService;
        _currentUserService = currentUserService;
        _assignGenValidator = assignGenValidator;
        _addDeptValidator = addDeptValidator;
        _replaceRolesValidator = replaceRolesValidator;
        _dbContext = dbContext;
        _userManager = userManager;
        _profileService = profileService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(AdminMemberPageDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminMemberPageDto>> GetMembers(
        [FromQuery] AdminMemberListQuery query,
        CancellationToken cancellationToken)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);

        var usersQuery = _dbContext.Users
            .Include(u => u.ClubMemberships)
                .ThenInclude(cm => cm.Generation)
            .Include(u => u.ClubMemberships)
                .ThenInclude(cm => cm.DepartmentMemberships)
                    .ThenInclude(dm => dm.Department)
            .AsNoTracking()
            .Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            usersQuery = usersQuery.Where(u =>
                (u.FullName != null && u.FullName.ToLower().Contains(search)) ||
                (u.DisplayName != null && u.DisplayName.ToLower().Contains(search)) ||
                (u.Email != null && u.Email.ToLower().Contains(search)) ||
                (u.StudentCode != null && u.StudentCode.ToLower().Contains(search)));
        }

        if (query.GenerationId.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.ClubMemberships.Any(cm => cm.GenerationId == query.GenerationId.Value && cm.IsActive));
        }

        if (query.DepartmentId.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.ClubMemberships.Any(cm =>
                cm.DepartmentMemberships.Any(dm => dm.DepartmentId == query.DepartmentId.Value && dm.IsActive)));
        }

        if (query.Status.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.Status == query.Status.Value);
        }

        var totalCount = await usersQuery.CountAsync(cancellationToken);

        var users = await usersQuery
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<AdminMemberListItemDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            if (!string.IsNullOrWhiteSpace(query.SystemRole) && !roles.Any(r => r.Equals(query.SystemRole, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var deptNames = user.ClubMemberships
                .SelectMany(cm => cm.DepartmentMemberships)
                .Where(dm => dm.IsActive && dm.Department != null)
                .Select(dm => dm.Department!.Name)
                .Distinct()
                .ToList();

            var genNumbers = user.ClubMemberships
                .Where(cm => cm.IsActive && cm.Generation != null)
                .Select(cm => cm.Generation!.Number)
                .Distinct()
                .OrderByDescending(n => n)
                .ToList();

            items.Add(new AdminMemberListItemDto(
                user.Id,
                user.Email ?? string.Empty,
                user.FullName,
                user.DisplayName,
                user.StudentCode,
                user.AvatarUrl,
                user.Status,
                roles.ToList(),
                deptNames,
                genNumbers,
                user.CreatedAt,
                user.LastLoginAt));
        }

        return Ok(new AdminMemberPageDto(items, totalCount, page, pageSize));
    }

    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileDto>> GetMemberProfile(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        var profile = await _profileService.GetMyProfileAsync(userId, cancellationToken);
        return Ok(profile);
    }

    [HttpPatch("{userId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        [FromRoute] Guid userId,
        [FromBody] UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted)
        {
            return NotFound();
        }

        user.Status = request.Status;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);
        return Ok(new { user.Id, user.Status });
    }

    [HttpPost("{userId:guid}/system-roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSystemRoles(
        [FromRoute] Guid userId,
        [FromBody] UpdateUserSystemRolesRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted)
        {
            return NotFound();
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var toRemove = currentRoles.Except(request.Roles).ToList();
        var toAdd = request.Roles.Except(currentRoles).ToList();

        if (toRemove.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, toRemove);
        }

        if (toAdd.Count > 0)
        {
            await _userManager.AddToRolesAsync(user, toAdd);
        }

        var newRoles = await _userManager.GetRolesAsync(user);
        return Ok(new { user.Id, Roles = newRoles });
    }

    [HttpPost("{userId:guid}/memberships")]
    [ProducesResponseType(typeof(ClubMembershipSummaryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClubMembershipSummaryDto>> AssignMemberToGen(
        [FromRoute] Guid userId,
        [FromBody] AssignMemberToGenRequest request,
        CancellationToken cancellationToken)
    {
        await _assignGenValidator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _membershipService.AssignMemberToGenAsync(userId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("{userId:guid}/memberships/{clubMembershipId:guid}/departments")]
    [ProducesResponseType(typeof(DepartmentMembershipSummaryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DepartmentMembershipSummaryDto>> AddMemberToDepartment(
        [FromRoute] Guid userId,
        [FromRoute] Guid clubMembershipId,
        [FromBody] AddMemberToDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        await _addDeptValidator.ValidateAndThrowAsync(request, cancellationToken);
        var currentUserId = GetCurrentUserId();
        var result = await _membershipService.AddMemberToDepartmentAsync(userId, clubMembershipId, request, currentUserId, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPatch("{userId:guid}/department-memberships/{departmentMembershipId:guid}")]
    [ProducesResponseType(typeof(DepartmentMembershipSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentMembershipSummaryDto>> UpdateDepartmentMembership(
        [FromRoute] Guid userId,
        [FromRoute] Guid departmentMembershipId,
        [FromBody] UpdateDepartmentMembershipRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _membershipService.UpdateDepartmentMembershipAsync(userId, departmentMembershipId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{userId:guid}/department-memberships/{departmentMembershipId:guid}/roles")]
    [ProducesResponseType(typeof(IReadOnlyList<ClubRoleDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ClubRoleDetailDto>>> ReplaceRoles(
        [FromRoute] Guid userId,
        [FromRoute] Guid departmentMembershipId,
        [FromBody] ReplaceRolesRequest request,
        CancellationToken cancellationToken)
    {
        await _replaceRolesValidator.ValidateAndThrowAsync(request, cancellationToken);
        var currentUserId = GetCurrentUserId();
        var result = await _membershipService.ReplaceRolesAsync(userId, departmentMembershipId, request, currentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{userId:guid}/department-memberships/{departmentMembershipId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EndDepartmentMembership(
        [FromRoute] Guid userId,
        [FromRoute] Guid departmentMembershipId,
        CancellationToken cancellationToken)
    {
        await _membershipService.EndDepartmentMembershipAsync(userId, departmentMembershipId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{userId:guid}/memberships/{clubMembershipId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EndClubMembership(
        [FromRoute] Guid userId,
        [FromRoute] Guid clubMembershipId,
        CancellationToken cancellationToken)
    {
        await _membershipService.EndClubMembershipAsync(userId, clubMembershipId, cancellationToken);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        return _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }
}

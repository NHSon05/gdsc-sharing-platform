using FluentValidation;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Memberships.Interfaces;
using GdscSharingPlatform.Application.Features.Memberships.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    public AdminMemberMembershipsController(
        IMemberMembershipService membershipService,
        ICurrentUserService currentUserService,
        IValidator<AssignMemberToGenRequest> assignGenValidator,
        IValidator<AddMemberToDepartmentRequest> addDeptValidator,
        IValidator<ReplaceRolesRequest> replaceRolesValidator)
    {
        _membershipService = membershipService;
        _currentUserService = currentUserService;
        _assignGenValidator = assignGenValidator;
        _addDeptValidator = addDeptValidator;
        _replaceRolesValidator = replaceRolesValidator;
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

    [HttpPut("{userId:guid}/department-memberships/{departmentMembershipId:guid}")]
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

    [HttpPut("{userId:guid}/department-memberships/{departmentMembershipId:guid}/roles")]
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

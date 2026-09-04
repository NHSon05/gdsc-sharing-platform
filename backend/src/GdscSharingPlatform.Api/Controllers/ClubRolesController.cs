using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Memberships.Interfaces;
using GdscSharingPlatform.Application.Features.Memberships.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers;

[ApiController]
[Route("api/club-roles")]
[Authorize(Policy = AuthPolicies.RequireActiveUser)]
public sealed class ClubRolesController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public ClubRolesController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClubRoleDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ClubRoleDetailDto>>> GetClubRoles(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var canIncludeInactive = User.IsInRole(RoleNames.Admin);
        var shouldIncludeInactive = canIncludeInactive && includeInactive;

        var roles = await _lookupService.GetClubRolesAsync(shouldIncludeInactive, cancellationToken);
        return Ok(roles);
    }
}

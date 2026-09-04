using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Memberships.Interfaces;
using GdscSharingPlatform.Application.Features.Memberships.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize(Policy = AuthPolicies.RequireActiveUser)]
public sealed class DepartmentsController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public DepartmentsController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DepartmentDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<DepartmentDetailDto>>> GetDepartments(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var canIncludeInactive = User.IsInRole(RoleNames.Admin);
        var shouldIncludeInactive = canIncludeInactive && includeInactive;

        var departments = await _lookupService.GetDepartmentsAsync(shouldIncludeInactive, cancellationToken);
        return Ok(departments);
    }
}

using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Memberships.Interfaces;
using GdscSharingPlatform.Application.Features.Memberships.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers;

[ApiController]
[Route("api/generations")]
[Authorize(Policy = AuthPolicies.RequireActiveUser)]
public sealed class GenerationsController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public GenerationsController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<GenerationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<GenerationDto>>> GetGenerations(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var canIncludeInactive = User.IsInRole(RoleNames.Admin);
        var shouldIncludeInactive = canIncludeInactive && includeInactive;

        var generations = await _lookupService.GetGenerationsAsync(shouldIncludeInactive, cancellationToken);
        return Ok(generations);
    }
}

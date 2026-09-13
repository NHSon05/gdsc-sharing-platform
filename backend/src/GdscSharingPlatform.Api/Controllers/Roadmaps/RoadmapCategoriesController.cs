using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Roadmaps.Interfaces;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers.Roadmaps;

[Route("api/roadmap-categories")]
[Authorize(Policy = AuthPolicies.RequireActiveUser)]
[Authorize(Roles = RoleNames.Admin + "," + RoleNames.Member)]
public sealed class RoadmapCategoriesController(IRoadmapCategoryService categories) : RoadmapApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> List(CancellationToken ct)
        => Ok(await categories.ListAsync(false, ct));
}

using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Roadmaps.Interfaces;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers.Roadmaps;

[Route("api/roadmaps")]
[Authorize(Policy = AuthPolicies.RequireActiveUser)]
[Authorize(Roles = RoleNames.Admin + "," + RoleNames.Member)]
public sealed class RoadmapsController(IRoadmapService roadmaps, IRoadmapNodeService nodes) : RoadmapApiController
{
    [HttpGet]
    public async Task<ActionResult<PageResponse<RoadmapSummary>>> List([FromQuery] RoadmapQuery query, CancellationToken ct)
        => Ok(await roadmaps.ListAsync(query, false, ct));

    [HttpGet("{slug}")]
    public async Task<ActionResult<RoadmapResponse>> Get(string slug, CancellationToken ct)
        => Ok(await roadmaps.GetBySlugAsync(slug, ct));

    [HttpGet("{roadmapId:guid}/nodes/{nodeId:guid}")]
    public async Task<ActionResult<NodeDetailResponse>> Node(Guid roadmapId, Guid nodeId, CancellationToken ct)
        => Ok(await nodes.GetAsync(roadmapId, nodeId, ct));
}

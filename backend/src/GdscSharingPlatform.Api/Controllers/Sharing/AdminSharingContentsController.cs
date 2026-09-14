using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Sharing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GdscSharingPlatform.Api.Controllers.Sharing;

[Route("api/admin/sharing/contents")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public sealed class AdminSharingContentsController(ISharingContentService service) : SharingApiController
{
    [HttpGet]
    public async Task<ActionResult<SharingPage<ContentSummary>>> List([FromQuery] ContentQuery query, CancellationToken ct)
        => Ok(await service.ListAsync(query, false, true, ct));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContentResponse>> Get(Guid id, CancellationToken ct)
    { var result = await service.GetAsync(id, false, true, ct); ETag(result.Content.Version); return Ok(result); }
    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ContentResponse>> Approve(Guid id, CancellationToken ct)
    { var result = await service.TransitionAsync(id, ContentAction.Approve, Version(), null, ct); ETag(result.Content.Version); return Ok(result); }
    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ContentResponse>> Reject(Guid id, ReviewRequest request, CancellationToken ct)
    { var result = await service.TransitionAsync(id, ContentAction.Reject, Version(), request.ReviewNote, ct); ETag(result.Content.Version); return Ok(result); }
    [HttpPost("{id:guid}/return-to-draft")]
    public async Task<ActionResult<ContentResponse>> ReturnToDraft(Guid id, CancellationToken ct)
    { var result = await service.TransitionAsync(id, ContentAction.ReturnToDraft, Version(), null, ct); ETag(result.Content.Version); return Ok(result); }
    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<ContentResponse>> Archive(Guid id, CancellationToken ct)
    { var result = await service.TransitionAsync(id, ContentAction.Archive, Version(), null, ct); ETag(result.Content.Version); return Ok(result); }
}

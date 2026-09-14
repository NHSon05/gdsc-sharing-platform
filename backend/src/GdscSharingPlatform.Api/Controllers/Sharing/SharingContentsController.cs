using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Sharing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GdscSharingPlatform.Api.Controllers.Sharing;

[Route("api/sharing/contents")]
[Authorize(Policy = AuthPolicies.RequireActiveUser)]
public sealed class SharingContentsController(ISharingContentService service) : SharingApiController
{
    [HttpGet]
    public async Task<ActionResult<SharingPage<ContentSummary>>> List([FromQuery] ContentQuery query, CancellationToken ct)
        => Ok(await service.ListAsync(query, false, false, ct));
    [HttpGet("mine")]
    public async Task<ActionResult<SharingPage<ContentSummary>>> Mine([FromQuery] ContentQuery query, CancellationToken ct)
        => Ok(await service.ListAsync(query, true, false, ct));
    [HttpGet("mine/{id:guid}")]
    public async Task<ActionResult<ContentResponse>> GetMine(Guid id, CancellationToken ct)
    { var result = await service.GetAsync(id, true, false, ct); ETag(result.Content.Version); return Ok(result); }
    [HttpGet("{slug}")]
    public async Task<ActionResult<ContentResponse>> Get(string slug, CancellationToken ct)
    { var result = await service.GetBySlugAsync(slug, ct); ETag(result.Content.Version); return Ok(result); }
    [HttpPost]
    [ProducesResponseType(typeof(ContentResponse), 201)]
    public async Task<ActionResult<ContentResponse>> Create(ContentRequest request, CancellationToken ct)
    { var result = await service.CreateAsync(request, ct); ETag(result.Content.Version); return CreatedAtAction(nameof(GetMine), new { id = result.Content.Id }, result); }
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ContentResponse>> Update(Guid id, ContentRequest request, CancellationToken ct)
    { var result = await service.UpdateAsync(id, request, Version(), ct); ETag(result.Content.Version); return Ok(result); }
    [HttpPost("{id:guid}/submit")]
    [EnableRateLimiting("sharing-submit")]
    public async Task<ActionResult<ContentResponse>> Submit(Guid id, CancellationToken ct)
    { var result = await service.TransitionAsync(id, ContentAction.Submit, Version(), null, ct); ETag(result.Content.Version); return Ok(result); }
    [HttpPost("{id:guid}/withdraw")]
    
    public async Task<ActionResult<ContentResponse>> Withdraw(Guid id, CancellationToken ct)
    { var result = await service.TransitionAsync(id, ContentAction.Withdraw, Version(), null, ct); ETag(result.Content.Version); return Ok(result); }
}

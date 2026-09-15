using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Sharing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GdscSharingPlatform.Api.Controllers.Sharing;

[Route("api/sharing")]
[Authorize(Policy = AuthPolicies.RequireActiveUser)]
[ProducesResponseType(typeof(ProblemDetails), 413)]
[ProducesResponseType(typeof(ProblemDetails), 415)]
public sealed class SharingResourcesController(ISharingResourceService service) : SharingApiController
{
    [HttpGet("contents/{contentId:guid}/resources")]
    public async Task<ActionResult<IReadOnlyList<ResourceResponse>>> List(Guid contentId, CancellationToken ct)
        => Ok(await service.ListAsync(contentId, ct));
    [HttpPost("contents/{contentId:guid}/resources/links")]
    [ProducesResponseType(typeof(ResourceMutationResponse), 201)]
    public async Task<ActionResult<ResourceMutationResponse>> CreateLink(Guid contentId, ResourceRequest request, CancellationToken ct)
    { var result = await service.CreateAsync(contentId, request, null, Version(), ct); ETag(result.Version); return CreatedAtAction(nameof(List), new { contentId }, result); }
    [HttpPost("contents/{contentId:guid}/resources/files")]
    [EnableRateLimiting("sharing-upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(21 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 21 * 1024 * 1024)]
    [ProducesResponseType(typeof(ResourceMutationResponse), 201)]
    public async Task<ActionResult<ResourceMutationResponse>> CreateFile(Guid contentId, [FromForm] SharingFileForm form, CancellationToken ct)
    {
        await using var stream = form.File.OpenReadStream();
        var result = await service.CreateAsync(contentId, new(form.Title, form.Description, null, form.SortOrder),
            new(stream, form.File.FileName, form.File.ContentType, form.File.Length), Version(), ct);
        ETag(result.Version); return CreatedAtAction(nameof(List), new { contentId }, result);
    }
    [HttpPatch("resources/{id:guid}")]
    public async Task<ActionResult<ResourceMutationResponse>> Update(Guid id, ResourceRequest request, CancellationToken ct)
    { var result = await service.UpdateAsync(id, request, Version(), ct); ETag(result.Version); return Ok(result); }
    [HttpPost("resources/{id:guid}/replace-file")]
    [EnableRateLimiting("sharing-upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(21 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 21 * 1024 * 1024)]
    public async Task<ActionResult<ResourceMutationResponse>> Replace(Guid id, [FromForm] SharingReplaceFileForm form, CancellationToken ct)
    {
        await using var stream = form.File.OpenReadStream();
        var result = await service.ReplaceAsync(id, new(stream, form.File.FileName, form.File.ContentType, form.File.Length), Version(), ct);
        ETag(result.Version); return Ok(result);
    }
    [HttpDelete("resources/{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    { ETag(await service.DeleteAsync(id, Version(), ct)); return NoContent(); }
    [HttpPatch("contents/{contentId:guid}/resources/reorder")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Reorder(Guid contentId, ReorderResourcesRequest request, CancellationToken ct)
    { ETag(await service.ReorderAsync(contentId, request, Version(), ct)); return NoContent(); }
    [HttpGet("resources/{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var file = await service.DownloadAsync(id, ct);
        Response.Headers.CacheControl = "private, no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(file.Content, file.ContentType, file.FileName);
    }
}
public sealed class SharingFileForm
{
    [System.ComponentModel.DataAnnotations.Required] public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    [System.ComponentModel.DataAnnotations.Required] public IFormFile File { get; set; } = null!;
}
public sealed class SharingReplaceFileForm
{
    [System.ComponentModel.DataAnnotations.Required] public IFormFile File { get; set; } = null!;
}

using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Infrastructure.Storage;
using GdscSharingPlatform.Api.Filters;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Roadmaps.Interfaces;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers.Roadmaps;

public sealed class ResourceFileForm
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public IFormFile File { get; set; } = null!;
}
public sealed class ReplaceResourceFileForm
{
    public IFormFile File { get; set; } = null!;
}

[Route("api/admin")]
[Authorize(Policy = AuthPolicies.AdminPolicy)]
public sealed class AdminRoadmapResourcesController(ILearningResourceService resources) : RoadmapApiController
{
    [HttpGet("roadmap-nodes/{nodeId:guid}/resources")]
    public async Task<ActionResult<IReadOnlyList<ResourceResponse>>> List(Guid nodeId, CancellationToken ct)
        => Ok(await resources.ListAsync(nodeId, ct));

    [HttpPost("roadmap-nodes/{nodeId:guid}/resources/links")]
    [ProducesResponseType(typeof(ResourceResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResourceResponse>> Link(Guid nodeId, LinkResourceRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await resources.CreateLinkAsync(nodeId, request, ct));

    [HttpPost("roadmap-nodes/{nodeId:guid}/resources/files")]
    [Consumes("multipart/form-data")]
    [RoadmapUploadLimit]
    [RequestSizeLimit(RoadmapStorageOptions.MaximumRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = RoadmapStorageOptions.MaximumRequestBytes)]
    [ProducesResponseType(typeof(ResourceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<ActionResult<ResourceResponse>> Upload(Guid nodeId, [FromForm] ResourceFileForm form, CancellationToken ct)
    {
        await using var stream = form.File.OpenReadStream();
        var result = await resources.CreateFileAsync(nodeId, new(form.Title, form.Description, form.SortOrder),
            new FileUpload(stream, form.File.FileName, form.File.ContentType, form.File.Length), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPatch("roadmap-resources/{id:guid}")]
    public async Task<ActionResult<ResourceResponse>> Update(Guid id, UpdateResourceRequest request, CancellationToken ct)
        => Ok(await resources.UpdateAsync(id, request, ct));

    [HttpPost("roadmap-resources/{id:guid}/replace-file")]
    [Consumes("multipart/form-data")]
    [RoadmapUploadLimit]
    [RequestSizeLimit(RoadmapStorageOptions.MaximumRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = RoadmapStorageOptions.MaximumRequestBytes)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<ActionResult<ResourceResponse>> Replace(Guid id, [FromForm] ReplaceResourceFileForm form, CancellationToken ct)
    {
        await using var stream = form.File.OpenReadStream();
        return Ok(await resources.ReplaceFileAsync(id,
            new FileUpload(stream, form.File.FileName, form.File.ContentType, form.File.Length), ct));
    }

    [HttpPatch("roadmap-resources/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Status(Guid id, ActiveStatusRequest request, CancellationToken ct)
    {
        await resources.SetStatusAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("roadmap-resources/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await resources.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPatch("roadmap-nodes/{nodeId:guid}/resources/reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reorder(Guid nodeId, ReorderRequest request, CancellationToken ct)
    {
        await resources.ReorderAsync(nodeId, request, ct);
        return NoContent();
    }
}

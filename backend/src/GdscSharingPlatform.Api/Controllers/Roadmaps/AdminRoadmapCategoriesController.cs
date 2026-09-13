using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Roadmaps.Interfaces;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers.Roadmaps;

[Route("api/admin/roadmap-categories")]
[Authorize(Policy = AuthPolicies.AdminPolicy)]
public sealed class AdminRoadmapCategoriesController(IRoadmapCategoryService categories) : RoadmapApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> List(CancellationToken ct)
        => Ok(await categories.ListAsync(true, ct));

    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CategoryResponse>> Create(CategoryRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await categories.CreateAsync(request, ct));

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<CategoryResponse>> Update(Guid id, CategoryRequest request, CancellationToken ct)
        => Ok(await categories.UpdateAsync(id, request, ct));

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Status(Guid id, ActiveStatusRequest request, CancellationToken ct)
    {
        await categories.SetStatusAsync(id, request, ct);
        return NoContent();
    }
}

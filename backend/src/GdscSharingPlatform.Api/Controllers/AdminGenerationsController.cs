using FluentValidation;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Memberships.Interfaces;
using GdscSharingPlatform.Application.Features.Memberships.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers;

[ApiController]
[Route("api/admin/generations")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public sealed class AdminGenerationsController : ControllerBase
{
    private readonly IGenerationService _generationService;
    private readonly IValidator<CreateGenerationRequest> _createValidator;
    private readonly IValidator<UpdateGenerationRequest> _updateValidator;

    public AdminGenerationsController(
        IGenerationService generationService,
        IValidator<CreateGenerationRequest> createValidator,
        IValidator<UpdateGenerationRequest> updateValidator)
    {
        _generationService = generationService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(GenerationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GenerationDto>> CreateGeneration(
        [FromBody] CreateGenerationRequest request,
        CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var created = await _generationService.CreateGenerationAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpPut("{generationId:guid}")]
    [ProducesResponseType(typeof(GenerationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenerationDto>> UpdateGeneration(
        [FromRoute] Guid generationId,
        [FromBody] UpdateGenerationRequest request,
        CancellationToken cancellationToken)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var updated = await _generationService.UpdateGenerationAsync(generationId, request, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{generationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateGeneration(
        [FromRoute] Guid generationId,
        CancellationToken cancellationToken)
    {
        await _generationService.DeactivateGenerationAsync(generationId, cancellationToken);
        return NoContent();
    }
}

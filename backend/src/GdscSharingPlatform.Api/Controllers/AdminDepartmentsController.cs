using FluentValidation;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Memberships.Interfaces;
using GdscSharingPlatform.Application.Features.Memberships.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers;

[ApiController]
[Route("api/admin/departments")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public sealed class AdminDepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly IValidator<CreateDepartmentRequest> _createValidator;
    private readonly IValidator<UpdateDepartmentRequest> _updateValidator;

    public AdminDepartmentsController(
        IDepartmentService departmentService,
        IValidator<CreateDepartmentRequest> createValidator,
        IValidator<UpdateDepartmentRequest> updateValidator)
    {
        _departmentService = departmentService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(DepartmentDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DepartmentDetailDto>> CreateDepartment(
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var created = await _departmentService.CreateDepartmentAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpPut("{departmentId:guid}")]
    [ProducesResponseType(typeof(DepartmentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DepartmentDetailDto>> UpdateDepartment(
        [FromRoute] Guid departmentId,
        [FromBody] UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var updated = await _departmentService.UpdateDepartmentAsync(departmentId, request, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{departmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateDepartment(
        [FromRoute] Guid departmentId,
        CancellationToken cancellationToken)
    {
        await _departmentService.DeactivateDepartmentAsync(departmentId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{departmentId:guid}/activate")]
    [ProducesResponseType(typeof(DepartmentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentDetailDto>> ActivateDepartment(
        [FromRoute] Guid departmentId,
        CancellationToken cancellationToken)
    {
        var activated = await _departmentService.ActivateDepartmentAsync(departmentId, cancellationToken);
        return Ok(activated);
    }
}

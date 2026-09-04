using FluentValidation;
using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Profile.Interfaces;
using GdscSharingPlatform.Application.Features.Profile.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize(Policy = AuthPolicies.RequireActiveUser)]
public sealed class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<UpdateProfileRequest> _updateProfileValidator;
    private readonly IValidator<ChangeEmailRequest> _changeEmailValidator;

    public ProfileController(
        IProfileService profileService,
        ICurrentUserService currentUserService,
        IValidator<UpdateProfileRequest> updateProfileValidator,
        IValidator<ChangeEmailRequest> changeEmailValidator)
    {
        _profileService = profileService;
        _currentUserService = currentUserService;
        _updateProfileValidator = updateProfileValidator;
        _changeEmailValidator = changeEmailValidator;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileDto>> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var profile = await _profileService.GetMyProfileAsync(userId, cancellationToken);
        return Ok(profile);
    }

    [HttpPatch("me")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProfileDto>> UpdateMyProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        await _updateProfileValidator.ValidateAndThrowAsync(request, cancellationToken);

        var userId = GetCurrentUserId();
        var updatedProfile = await _profileService.UpdateMyProfileAsync(userId, request, cancellationToken);
        return Ok(updatedProfile);
    }

    [HttpPatch("me/email")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProfileDto>> ChangeEmail(
        [FromBody] ChangeEmailRequest request,
        CancellationToken cancellationToken)
    {
        await _changeEmailValidator.ValidateAndThrowAsync(request, cancellationToken);

        var userId = GetCurrentUserId();
        var updatedProfile = await _profileService.ChangeEmailAsync(userId, request, cancellationToken);
        return Ok(updatedProfile);
    }

    [HttpPost("me/avatar")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AvatarUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public async Task<ActionResult<AvatarUploadResponse>> UploadAvatar(
        IFormFile? avatar,
        CancellationToken cancellationToken)
    {
        if (avatar is null || avatar.Length == 0)
        {
            throw new ApplicationValidationException("avatar", "Avatar file is required.");
        }

        var userId = GetCurrentUserId();
        await using var stream = avatar.OpenReadStream();

        var response = await _profileService.UploadAvatarAsync(
            userId,
            stream,
            avatar.FileName,
            avatar.ContentType,
            cancellationToken);

        return Ok(response);
    }

    [HttpDelete("me/avatar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAvatar(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await _profileService.DeleteAvatarAsync(userId, cancellationToken);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        return _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }
}

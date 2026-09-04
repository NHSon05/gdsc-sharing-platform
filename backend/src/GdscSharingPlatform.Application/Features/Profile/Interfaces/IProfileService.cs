using GdscSharingPlatform.Application.Features.Profile.Models;

namespace GdscSharingPlatform.Application.Features.Profile.Interfaces;

public interface IProfileService
{
    Task<ProfileDto> GetMyProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ProfileDto> UpdateMyProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<ProfileDto> ChangeEmailAsync(
        Guid userId,
        ChangeEmailRequest request,
        CancellationToken cancellationToken = default);

    Task<AvatarUploadResponse> UploadAvatarAsync(
        Guid userId,
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteAvatarAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

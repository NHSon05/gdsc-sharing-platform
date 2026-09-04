namespace GdscSharingPlatform.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAvatarAsync(
        Guid userId,
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteAvatarAsync(
        string avatarUrl,
        CancellationToken cancellationToken = default);
}

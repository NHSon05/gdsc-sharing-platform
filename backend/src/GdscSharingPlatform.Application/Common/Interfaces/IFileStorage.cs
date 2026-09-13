namespace GdscSharingPlatform.Application.Common.Interfaces;

public sealed record FileUpload(Stream Content, string FileName, string ContentType, long Length);
public sealed record StoredFile(string OriginalFileName, string StoredFileName, string StorageKey,
    long FileSize, string ContentType);

/// <summary>Private learning-resource storage. Keys are never public URLs.</summary>
public interface IFileStorage
{
    Task<StoredFile> SaveAsync(FileUpload upload, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}

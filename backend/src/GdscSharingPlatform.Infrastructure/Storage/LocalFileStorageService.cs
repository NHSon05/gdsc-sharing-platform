using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace GdscSharingPlatform.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalFileStorageService> _logger;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> UploadAvatarAsync(
        Guid userId,
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (stream.Length > MaxFileSize)
        {
            throw new PayloadTooLargeException("File size exceeds 5 MB limit.");
        }

        if (!AllowedContentTypes.Contains(contentType))
        {
            throw new UnsupportedMediaTypeException("Only JPEG, PNG, or WebP images are supported.");
        }

        // Validate magic bytes
        var buffer = new byte[12];
        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
        stream.Position = 0; // Reset position

        if (!IsValidImageSignature(buffer, bytesRead, out var extension))
        {
            throw new UnsupportedMediaTypeException("Invalid image file signature.");
        }

        var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var userAvatarDir = Path.Combine(webRootPath, "uploads", "avatars", userId.ToString());

        if (!Directory.Exists(userAvatarDir))
        {
            Directory.CreateDirectory(userAvatarDir);
        }

        var newFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(userAvatarDir, newFileName);

        await using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await stream.CopyToAsync(fileStream, cancellationToken);
        }

        var avatarUrl = $"/uploads/avatars/{userId}/{newFileName}";
        _logger.LogInformation("Uploaded avatar for user {UserId} at {AvatarUrl}", userId, avatarUrl);

        return avatarUrl;
    }

    public Task DeleteAvatarAsync(string avatarUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl) ||
            !avatarUrl.StartsWith("/uploads/avatars/") ||
            avatarUrl.Contains(".."))
        {
            return Task.CompletedTask;
        }

        try
        {
            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var relativePath = avatarUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(webRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Deleted avatar file at {FullPath}", fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete avatar file at {AvatarUrl}", avatarUrl);
        }

        return Task.CompletedTask;
    }

    private static bool IsValidImageSignature(byte[] header, int length, out string extension)
    {
        extension = string.Empty;
        if (length < 4)
        {
            return false;
        }

        // JPEG: FF D8 FF
        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            extension = ".jpg";
            return true;
        }

        // PNG: 89 50 4E 47
        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
        {
            extension = ".png";
            return true;
        }

        // WebP: RIFF .... WEBP
        if (length >= 12 &&
            header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
            header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
        {
            extension = ".webp";
            return true;
        }

        return false;
    }
}

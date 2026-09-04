using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Infrastructure.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;

namespace GdscSharingPlatform.UnitTests.Infrastructure.Storage;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _tempWebRoot;
    private readonly LocalFileStorageService _service;

    public LocalFileStorageServiceTests()
    {
        _tempWebRoot = Path.Combine(Path.GetTempPath(), "gdsc_test_webroot_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempWebRoot);

        var fakeEnvironment = new TestHostEnvironment { WebRootPath = _tempWebRoot };
        _service = new LocalFileStorageService(fakeEnvironment, NullLogger<LocalFileStorageService>.Instance);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempWebRoot))
        {
            try { Directory.Delete(_tempWebRoot, true); } catch { }
        }
    }

    [Fact]
    public async Task UploadAvatar_ValidJpeg_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var jpegBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01 };
        using var stream = new MemoryStream(jpegBytes);

        var url = await _service.UploadAvatarAsync(userId, stream, "avatar.jpg", "image/jpeg");

        Assert.StartsWith($"/uploads/avatars/{userId}/", url);
        Assert.EndsWith(".jpg", url);
    }

    [Fact]
    public async Task UploadAvatar_ValidPng_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D };
        using var stream = new MemoryStream(pngBytes);

        var url = await _service.UploadAvatarAsync(userId, stream, "avatar.png", "image/png");

        Assert.StartsWith($"/uploads/avatars/{userId}/", url);
        Assert.EndsWith(".png", url);
    }

    [Fact]
    public async Task UploadAvatar_ExceedingSize_ShouldThrowPayloadTooLargeException()
    {
        var userId = Guid.NewGuid();
        var oversizedBytes = new byte[5 * 1024 * 1024 + 1];
        using var stream = new MemoryStream(oversizedBytes);

        await Assert.ThrowsAsync<PayloadTooLargeException>(() =>
            _service.UploadAvatarAsync(userId, stream, "large.png", "image/png"));
    }

    [Fact]
    public async Task UploadAvatar_InvalidSignature_ShouldThrowUnsupportedMediaTypeException()
    {
        var userId = Guid.NewGuid();
        var fakeBytes = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
        using var stream = new MemoryStream(fakeBytes);

        await Assert.ThrowsAsync<UnsupportedMediaTypeException>(() =>
            _service.UploadAvatarAsync(userId, stream, "fake.jpg", "image/jpeg"));
    }

    [Fact]
    public async Task DeleteAvatar_ExistingFile_ShouldDeleteFromDisk()
    {
        var userId = Guid.NewGuid();
        var jpegBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01 };
        using var stream = new MemoryStream(jpegBytes);

        var url = await _service.UploadAvatarAsync(userId, stream, "avatar.jpg", "image/jpeg");
        var filePath = Path.Combine(_tempWebRoot, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(filePath));

        await _service.DeleteAvatarAsync(url);
        Assert.False(File.Exists(filePath));
    }

    private sealed class TestHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string ApplicationName { get; set; } = "Test";
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string ContentRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Test";
    }
}

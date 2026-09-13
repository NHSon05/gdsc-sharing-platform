using System.IO.Compression;
using System.Text;
using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Infrastructure.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.UnitTests.Infrastructure.Storage;

public sealed partial class LocalRoadmapFileStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"roadmap_storage_{Guid.NewGuid():N}");
    private LocalRoadmapFileStorage Storage(long limit = 4096) => new(new EnvironmentStub
    {
        ContentRootPath = _root, WebRootPath = Path.Combine(_root, "wwwroot")
    }, Options.Create(new RoadmapStorageOptions { RootPath = "private", MaxFileBytes = limit }));
    private string[] Files() => Directory.Exists(Path.Combine(_root, "private")) ? Directory.GetFiles(Path.Combine(_root, "private")) : [];
    public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }

    [Fact]
    public async Task SaveReadDelete_UsesPrivateGeneratedKeyAndSanitizedOriginalName()
    {
        var storage = Storage();
        var bytes = "%PDF-1.7\ncontent"u8.ToArray();
        await using var input = new MemoryStream(bytes);
        var file = await storage.SaveAsync(new(input, "../../guide.pdf", "application/pdf", bytes.Length));
        Assert.Equal("guide.pdf", file.OriginalFileName);
        Assert.Matches("^[a-f0-9]{32}\\.pdf$", file.StorageKey);
        Assert.DoesNotContain("wwwroot", Assert.Single(Files()));
        await using (var output = await storage.OpenReadAsync(file.StorageKey))
        {
            using var copy = new MemoryStream(); await output.CopyToAsync(copy);
            Assert.Equal(bytes, copy.ToArray());
        }
        await storage.DeleteAsync(file.StorageKey);
        await storage.DeleteAsync(file.StorageKey);
        Assert.Empty(Files());
        await Assert.ThrowsAsync<NotFoundException>(() => storage.OpenReadAsync(file.StorageKey));
        await Assert.ThrowsAsync<NotFoundException>(() => storage.OpenReadAsync("../secret.pdf"));
    }

    [Theory]
    [InlineData("fake.pdf", "application/pdf", "Not PDF")]
    [InlineData("fake.png", "image/png", "%PDF-1.7")]
    [InlineData("guide.pdf", "image/jpeg", "%PDF-1.7")]
    [InlineData("script.html", "text/html", "<html>")]
    [InlineData("fake.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "PKfake")]
    public async Task InvalidExtensionMimeOrSignature_LeavesNoFiles(string name, string type, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        await using var input = new MemoryStream(bytes);
        await Assert.ThrowsAsync<UnsupportedMediaTypeException>(() => Storage().SaveAsync(new(input, name, type, bytes.Length)));
        Assert.Empty(Files());
    }

    [Fact]
    public async Task ActualSizeIsBoundedEvenWhenReportedLengthIsSmaller_AndPartialFilesAreRemoved()
    {
        await using var input = new MemoryStream(new byte[100]);
        await Assert.ThrowsAsync<PayloadTooLargeException>(() => Storage(50).SaveAsync(new(input, "guide.pdf", "application/pdf", 10)));
        Assert.Empty(Files());
    }

    [Fact]
    public async Task OfficeDocumentRequiresExpectedZipEntries()
    {
        using var input = new MemoryStream();
        using (var archive = new ZipArchive(input, ZipArchiveMode.Create, true))
        {
            archive.CreateEntry("[Content_Types].xml");
            archive.CreateEntry("word/document.xml");
        }
        input.Position = 0;
        var file = await Storage().SaveAsync(new(input, "guide.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", input.Length));
        Assert.EndsWith(".docx", file.StorageKey);
    }

    [Fact]
    public void StorageCannotBeConfiguredInsideWebRoot()
    {
        var environment = new EnvironmentStub { ContentRootPath = _root, WebRootPath = Path.Combine(_root, "wwwroot") };
        Assert.Throws<InvalidOperationException>(() => new LocalRoadmapFileStorage(environment,
            Options.Create(new RoadmapStorageOptions { RootPath = "wwwroot/private" })));
    }

    [Fact]
    public async Task BinaryContentDisguisedAsTextIsRejected()
    {
        await using var input = new MemoryStream([255, 0, 1]);
        await Assert.ThrowsAsync<UnsupportedMediaTypeException>(() => Storage().SaveAsync(new(input, "guide.txt", "text/plain", 3)));
        Assert.Empty(Files());
    }

    private sealed class EnvironmentStub : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string ApplicationName { get; set; } = "Test";
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string ContentRootPath { get; set; } = "";
        public string EnvironmentName { get; set; } = "Test";
    }
}

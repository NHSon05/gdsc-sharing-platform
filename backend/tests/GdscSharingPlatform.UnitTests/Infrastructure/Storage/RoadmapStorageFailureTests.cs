using System.IO.Compression;
using System.Text;
using GdscSharingPlatform.Application.Common.Exceptions;

namespace GdscSharingPlatform.UnitTests.Infrastructure.Storage;

public sealed partial class LocalRoadmapFileStorageTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InterruptedStreamRemovesPartialUpload(bool cancelled)
    {
        using var cancellation = new CancellationTokenSource();
        await using var stream = new InterruptedStream(cancelled, cancellation);
        var error = await Record.ExceptionAsync(() => Storage().SaveAsync(new(stream, "guide.pdf", "application/pdf", 100), cancellation.Token));
        if (cancelled) Assert.IsAssignableFrom<OperationCanceledException>(error);
        else Assert.IsType<IOException>(error);
        Assert.Empty(Files());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    public async Task EmptyOrIncorrectReportedLengthIsRejectedAndCleaned(int reported)
    {
        await using var stream = new MemoryStream("%PDF-1.7"u8.ToArray());
        await Assert.ThrowsAsync<ApplicationValidationException>(() => Storage().SaveAsync(new(stream, "guide.pdf", "application/pdf", reported)));
        Assert.Empty(Files());
    }

    [Fact]
    public async Task ExactLimitAndNonSeekableUploadWork()
    {
        var bytes = Encoding.UTF8.GetBytes("Học lập trình");
        await using var stream = new NonSeekableStream(bytes);
        var file = await Storage(bytes.Length).SaveAsync(new(stream, "guide.TXT", "text/plain; charset=utf-8", bytes.Length));
        Assert.Equal(bytes.Length, file.FileSize);
        Assert.Equal("text/plain", file.ContentType);
        Assert.EndsWith(".txt", file.StorageKey);
    }

    [Theory]
    [InlineData(".docx", "word/document.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData(".pptx", "ppt/presentation.xml", "application/vnd.openxmlformats-officedocument.presentationml.presentation")]
    [InlineData(".xlsx", "xl/workbook.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public async Task OfficeFormatsRequireMatchingPackageAndRejectMacros(string extension, string entry, string mime)
    {
        foreach (var mode in new[] { "valid", "missing-types", "wrong-document", "macro" })
        {
            using var stream = new MemoryStream();
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                if (mode != "missing-types") zip.CreateEntry("[Content_Types].xml");
                zip.CreateEntry(mode == "wrong-document" ? "other/document.xml" : entry);
                if (mode == "macro") zip.CreateEntry("word/vbaProject.bin");
            }
            stream.Position = 0;
            var storage = Storage();
            if (mode == "valid")
            {
                var file = await storage.SaveAsync(new(stream, "guide" + extension, mime, stream.Length));
                await storage.DeleteAsync(file.StorageKey);
            }
            else await Assert.ThrowsAsync<UnsupportedMediaTypeException>(() => storage.SaveAsync(new(stream, "guide" + extension, mime, stream.Length)));
            Assert.Empty(Files());
        }
    }

    [Theory]
    [InlineData("../outside.pdf")]
    [InlineData("/tmp/outside.pdf")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.pdf/../secret")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.exe")]
    public async Task InvalidStorageKeysCannotReadOrDeleteFiles(string key)
    {
        var storage = Storage();
        await Assert.ThrowsAsync<NotFoundException>(() => storage.OpenReadAsync(key));
        await Assert.ThrowsAsync<NotFoundException>(() => storage.DeleteAsync(key));
    }

    private sealed class NonSeekableStream(byte[] bytes) : MemoryStream(bytes)
    {
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    }

    private sealed class InterruptedStream(bool cancelled, CancellationTokenSource cancellation) : MemoryStream
    {
        private bool _started;
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (!_started)
            {
                _started = true;
                "%PDF-1.7"u8.CopyTo(buffer.Span);
                return ValueTask.FromResult(8);
            }
            if (cancelled)
            {
                cancellation.Cancel();
                cancellationToken.ThrowIfCancellationRequested();
            }
            throw new IOException("Injected stream read failure.");
        }
    }
}

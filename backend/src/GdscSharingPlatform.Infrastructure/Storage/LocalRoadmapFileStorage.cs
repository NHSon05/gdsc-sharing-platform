using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.Infrastructure.Storage;

public sealed class LocalRoadmapFileStorage : IFileStorage
{
    private readonly string _root;
    private readonly long _maxFileBytes;
    private static readonly IReadOnlyDictionary<string, string> ContentTypes = new Dictionary<string, string>
    {
        [".pdf"] = "application/pdf", [".png"] = "image/png", [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg",
        [".txt"] = "text/plain", [".md"] = "text/markdown",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };

    public LocalRoadmapFileStorage(IWebHostEnvironment environment, IOptions<RoadmapStorageOptions> options)
    {
        _root = Path.GetFullPath(options.Value.RootPath, environment.ContentRootPath);
        var webRoot = Path.GetFullPath(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"));
        var relative = Path.GetRelativePath(webRoot, _root);
        if (relative == "." || (!relative.StartsWith(".." + Path.DirectorySeparatorChar) && relative != ".." && !Path.IsPathRooted(relative)))
            throw new InvalidOperationException("Roadmap storage must be outside wwwroot.");
        _maxFileBytes = options.Value.MaxFileBytes;
    }

    public async Task<StoredFile> SaveAsync(FileUpload upload, CancellationToken cancellationToken = default)
    {
        if (upload.Length > _maxFileBytes) throw new PayloadTooLargeException();
        if (upload.Length <= 0) throw new ApplicationValidationException("file", "A non-empty file is required.");
        var originalName = Path.GetFileName(upload.FileName.Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(originalName) || originalName.Length > 255 || originalName.Any(char.IsControl))
            throw new ApplicationValidationException("file", "The file name is invalid.");
        var extension = Path.GetExtension(originalName).ToLowerInvariant();
        if (!ContentTypes.TryGetValue(extension, out var contentType)
            || !string.Equals(upload.ContentType.Split(';')[0].Trim(), contentType, StringComparison.OrdinalIgnoreCase))
            throw new UnsupportedMediaTypeException("The file extension and MIME type must match an allowed format: PDF, PNG, JPEG, TXT, MD, DOCX, PPTX or XLSX.");
        try { Directory.CreateDirectory(_root); }
        catch (UnauthorizedAccessException exception) { throw new IOException("Unable to access resource storage.", exception); }
        var key = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(_root, key);
        var temporary = path + ".upload";
        try
        {
            long length = 0;
            await using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 81920, true))
            {
                var buffer = new byte[81920];
                int read;
                while ((read = await upload.Content.ReadAsync(buffer, cancellationToken)) != 0)
                {
                    length += read;
                    if (length > _maxFileBytes) throw new PayloadTooLargeException();
                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }
                if (length != upload.Length) throw new ApplicationValidationException("file", "File length does not match the submitted metadata.");
                output.Position = 0;
                await ValidateContentAsync(output, extension, cancellationToken);
            }
            File.Move(temporary, path);
            return new(originalName, key, key, length, contentType);
        }
        catch (Exception exception)
        {
            try { File.Delete(temporary); }
            catch (UnauthorizedAccessException cleanupException) { throw new IOException("Unable to clean partial resource file.", cleanupException); }
            if (exception is UnauthorizedAccessException) throw new IOException("Unable to write resource storage.", exception);
            throw;
        }
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = Resolve(storageKey);
        try { return Task.FromResult<Stream>(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, 81920, true)); }
        catch (FileNotFoundException) { throw new NotFoundException("Resource file", "missing file"); }
        catch (DirectoryNotFoundException) { throw new NotFoundException("Resource file", "missing file"); }
        catch (UnauthorizedAccessException exception) { throw new IOException("Unable to read resource storage.", exception); }
    }
    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        File.Delete(Resolve(storageKey));
        return Task.CompletedTask;
    }
    private string Resolve(string key)
    {
        if (!Regex.IsMatch(key, "^[a-f0-9]{32}\\.[a-z0-9]+$") || !ContentTypes.ContainsKey(Path.GetExtension(key)))
            throw new NotFoundException("Resource file", "invalid key");
        return Path.Combine(_root, key);
    }
    private static async Task ValidateContentAsync(Stream stream, string extension, CancellationToken ct)
    {
        var header = new byte[8];
        var count = await stream.ReadAtLeastAsync(header, 8, false, ct);
        stream.Position = 0;
        bool valid;
        switch (extension)
        {
            case ".pdf": valid = count >= 5 && header.AsSpan(0, 5).SequenceEqual("%PDF-"u8); break;
            case ".png": valid = count == 8 && header.AsSpan().SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }); break;
            case ".jpg": case ".jpeg": valid = count >= 3 && header[0] == 255 && header[1] == 216 && header[2] == 255; break;
            case ".txt": case ".md":
                try
                {
                    using var reader = new StreamReader(stream, new UTF8Encoding(false, true), false, 4096, true);
                    var text = new char[4096];
                    int read;
                    while ((read = await reader.ReadAsync(text.AsMemory(), ct)) > 0)
                        if (text.AsSpan(0, read).Contains('\0')) throw new DecoderFallbackException();
                    valid = true;
                }
                catch (DecoderFallbackException) { valid = false; }
                break;
            default:
                valid = count >= 4 && header[0] == 80 && header[1] == 75 && header[2] == 3 && header[3] == 4;
                if (valid)
                {
                    try
                    {
                        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);
                        var document = extension switch { ".docx" => "word/document.xml", ".pptx" => "ppt/presentation.xml", _ => "xl/workbook.xml" };
                        valid = archive.GetEntry("[Content_Types].xml") is not null && archive.GetEntry(document) is not null
                            && !archive.Entries.Any(x => x.FullName.EndsWith("vbaProject.bin", StringComparison.OrdinalIgnoreCase));
                    }
                    catch (InvalidDataException) { valid = false; }
                }
                break;
        }
        if (!valid) throw new UnsupportedMediaTypeException("File content does not match its extension and MIME type.");
    }
}

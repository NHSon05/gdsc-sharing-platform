using GdscSharingPlatform.Domain.Common;
using GdscSharingPlatform.Domain.Enums;

namespace GdscSharingPlatform.Domain.Roadmaps;

public sealed class LearningResource : BaseEntity
{
    private LearningResource() { }

    private LearningResource(Guid roadmapNodeId, string title, Guid createdByUserId, int sortOrder)
    {
        RoadmapNodeId = RoadmapValidation.RequiredId(roadmapNodeId, nameof(roadmapNodeId));
        Title = RoadmapValidation.RequiredText(title, 150, nameof(title));
        CreatedByUserId = RoadmapValidation.RequiredId(createdByUserId, nameof(createdByUserId));
        SortOrder = RoadmapValidation.SortOrder(sortOrder);
    }

    public Guid RoadmapNodeId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; set; }
    public ResourceType ResourceType { get; private set; }
    public string? ExternalUrl { get; private set; }
    public string? OriginalFileName { get; private set; }
    public string? StoredFileName { get; private set; }
    public string? StorageKey { get; private set; }
    public long? FileSize { get; private set; }
    public string? ContentType { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid CreatedByUserId { get; private set; }
    public RoadmapNode RoadmapNode { get; set; } = null!;

    public static LearningResource CreateLink(Guid roadmapNodeId, string title, string externalUrl,
        Guid createdByUserId, int sortOrder = 0)
    {
        externalUrl = RoadmapValidation.RequiredText(externalUrl, 2048, nameof(externalUrl));
        if (!Uri.TryCreate(externalUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || string.IsNullOrWhiteSpace(uri.Host))
        {
            throw new ArgumentException("External URL must be a valid HTTP or HTTPS URL.", nameof(externalUrl));
        }

        return new LearningResource(roadmapNodeId, title, createdByUserId, sortOrder)
        {
            ResourceType = ResourceType.Link,
            ExternalUrl = externalUrl
        };
    }

    // Call only after storage has successfully saved the file.
    public static LearningResource CreateFile(Guid roadmapNodeId, string title,
        string originalFileName, string storedFileName, string storageKey, long fileSize,
        string contentType, Guid createdByUserId, int sortOrder = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(fileSize);
        return new LearningResource(roadmapNodeId, title, createdByUserId, sortOrder)
        {
            ResourceType = ResourceType.File,
            OriginalFileName = RoadmapValidation.RequiredText(originalFileName, 255, nameof(originalFileName)),
            StoredFileName = RoadmapValidation.RequiredText(storedFileName, 255, nameof(storedFileName)),
            StorageKey = RoadmapValidation.RequiredText(storageKey, 1024, nameof(storageKey)),
            FileSize = fileSize,
            ContentType = RoadmapValidation.RequiredText(contentType, 255, nameof(contentType))
        };
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
    public void Update(string title, string? description, string? externalUrl, int sortOrder)
    {
        var validTitle = RoadmapValidation.RequiredText(title, 150, nameof(title));
        var order = RoadmapValidation.SortOrder(sortOrder);
        if (ResourceType == ResourceType.Link)
            externalUrl = CreateLink(RoadmapNodeId, title, externalUrl!, CreatedByUserId, sortOrder).ExternalUrl;
        else if (externalUrl is not null)
            throw new ArgumentException("File resources cannot contain an external URL.", nameof(externalUrl));
        Title = validTitle;
        Description = description;
        ExternalUrl = externalUrl;
        SortOrder = order;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void ReplaceFile(string originalFileName, string storedFileName, string storageKey,
        long fileSize, string contentType)
    {
        if (ResourceType != ResourceType.File)
            throw new InvalidOperationException("Only file resources can have their file replaced.");
        var validated = CreateFile(RoadmapNodeId, Title, originalFileName, storedFileName,
            storageKey, fileSize, contentType, CreatedByUserId, SortOrder);
        OriginalFileName = validated.OriginalFileName;
        StoredFileName = validated.StoredFileName;
        StorageKey = validated.StorageKey;
        FileSize = validated.FileSize;
        ContentType = validated.ContentType;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Reorder(int sortOrder)
    {
        SortOrder = RoadmapValidation.SortOrder(sortOrder);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

}

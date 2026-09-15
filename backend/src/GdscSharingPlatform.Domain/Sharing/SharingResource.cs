using GdscSharingPlatform.Domain.Common;
using GdscSharingPlatform.Domain.Enums;

namespace GdscSharingPlatform.Domain.Sharing;

public sealed class SharingResource : BaseEntity
{
    private SharingResource() { }
    public SharingResource(Guid sharingContentId, string title, ResourceType resourceType, int sortOrder, Guid createdByUserId)
    {
        SharingContentId = Required(sharingContentId, nameof(sharingContentId)); Title = Required(title, 200, nameof(title)); ResourceType = resourceType;
        SortOrder = sortOrder < 0 ? throw new ArgumentOutOfRangeException(nameof(sortOrder)) : sortOrder; CreatedByUserId = Required(createdByUserId, nameof(createdByUserId));
    }
    public Guid SharingContentId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ResourceType ResourceType { get; private set; }
    public string? ExternalUrl { get; private set; }
    public string? OriginalFileName { get; private set; }
    public string? StorageKey { get; private set; }
    public long? FileSize { get; private set; }
    public string? ContentType { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid CreatedByUserId { get; private set; }
    public void Update(string title, string? description, string? externalUrl, int sortOrder)
    {
        Title = Required(title, 200, nameof(title));
        Description = description;
        if (ResourceType == ResourceType.Link)
        {
            if (!Uri.TryCreate(externalUrl, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
                throw new ArgumentException("A link requires an HTTP(S) URL.");
            ExternalUrl = externalUrl;
        }
        else if (externalUrl is not null) throw new ArgumentException("File resources cannot have an external URL.");
        Reorder(sortOrder);
    }
    public void SetFile(string name, string key, long size, string contentType)
    {
        if (ResourceType != ResourceType.File || size <= 0) throw new ArgumentException("Invalid file metadata.");
        OriginalFileName = Required(name, 255, nameof(name));
        StorageKey = Required(key, 500, nameof(key));
        ContentType = Required(contentType, 255, nameof(contentType));
        FileSize = size;
    }
    public void Deactivate() { IsActive = false; UpdatedAtUtc = DateTimeOffset.UtcNow; }
    public void Reorder(int order)
    {
        if (order < 0) throw new ArgumentOutOfRangeException(nameof(order));
        SortOrder = order;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
    private static Guid Required(Guid id, string name) => id == Guid.Empty ? throw new ArgumentException("A valid id is required.", name) : id;
    private static string Required(string value, int max, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("A value is required.", name) : value.Trim().Length > max ? throw new ArgumentOutOfRangeException(name) : value.Trim();
}

using GdscSharingPlatform.Domain.Common;
using GdscSharingPlatform.Domain.Enums;

namespace GdscSharingPlatform.Domain.Roadmaps;

public sealed class Roadmap : BaseEntity
{
    private Roadmap() { }

    public Roadmap(Guid categoryId, string title, string slug, string shortDescription,
        RoadmapLevel level, Guid createdByUserId, int sortOrder = 0)
    {
        CategoryId = RoadmapValidation.RequiredId(categoryId, nameof(categoryId));
        Title = RoadmapValidation.RequiredText(title, 150, nameof(title));
        Slug = RoadmapValidation.Slug(slug);
        ShortDescription = RoadmapValidation.RequiredText(shortDescription, 500, nameof(shortDescription));
        Level = RoadmapValidation.DefinedEnum(level);
        CreatedByUserId = RoadmapValidation.RequiredId(createdByUserId, nameof(createdByUserId));
        SortOrder = RoadmapValidation.SortOrder(sortOrder);
        PublishedAtUtc = CreatedAtUtc;
    }

    public Guid CategoryId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string ShortDescription { get; private set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public RoadmapLevel Level { get; private set; }
    public string? EstimatedDuration { get; set; }
    public string? Prerequisites { get; set; }
    public RoadmapStatus Status { get; private set; } = RoadmapStatus.Published;
    public int SortOrder { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }
    public DateTimeOffset? PublishedAtUtc { get; private set; }
    public RoadmapCategory Category { get; set; } = null!;
    public ICollection<RoadmapNode> Nodes { get; private set; } = new List<RoadmapNode>();
    public ICollection<RoadmapEdge> Edges { get; private set; } = new List<RoadmapEdge>();
    public void Update(Guid categoryId, string title, string slug, string shortDescription,
        RoadmapLevel level, int sortOrder, Guid updatedByUserId)
    {
        var validated = new Roadmap(categoryId, title, slug, shortDescription, level, CreatedByUserId, sortOrder);
        Touch(updatedByUserId);
        CategoryId = validated.CategoryId;
        Title = validated.Title;
        Slug = validated.Slug;
        ShortDescription = validated.ShortDescription;
        Level = validated.Level;
        SortOrder = validated.SortOrder;
    }

    public void ChangeStatus(RoadmapStatus status, bool hasActiveNodes, Guid updatedByUserId)
    {
        RoadmapValidation.DefinedEnum(status);
        if (status == RoadmapStatus.Published && Status != status && !hasActiveNodes)
            throw new InvalidOperationException("A roadmap needs an active node before it can be published.");
        Touch(updatedByUserId);
        Status = status;
        if (status == RoadmapStatus.Published) PublishedAtUtc ??= DateTimeOffset.UtcNow;
    }

    public void Reorder(int sortOrder, Guid updatedByUserId)
    {
        var order = RoadmapValidation.SortOrder(sortOrder);
        Touch(updatedByUserId);
        SortOrder = order;
    }

    private void Touch(Guid userId)
    {
        UpdatedByUserId = RoadmapValidation.RequiredId(userId, nameof(userId));
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

}

using GdscSharingPlatform.Domain.Common;

namespace GdscSharingPlatform.Domain.Roadmaps;

public sealed class RoadmapCategory : BaseEntity
{
    private RoadmapCategory() { }

    public RoadmapCategory(string name, string slug, int sortOrder = 0)
    {
        Name = RoadmapValidation.RequiredText(name, 150, nameof(name));
        Slug = RoadmapValidation.Slug(slug);
        SortOrder = RoadmapValidation.SortOrder(sortOrder);
    }

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public ICollection<Roadmap> Roadmaps { get; private set; } = new List<Roadmap>();

    public void Update(string name, string slug, int sortOrder)
    {
        var validName = RoadmapValidation.RequiredText(name, 150, nameof(name));
        var validSlug = RoadmapValidation.Slug(slug);
        var validOrder = RoadmapValidation.SortOrder(sortOrder);
        Name = validName;
        Slug = validSlug;
        SortOrder = validOrder;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}

using GdscSharingPlatform.Domain.Common;
using GdscSharingPlatform.Domain.Enums;

namespace GdscSharingPlatform.Domain.Roadmaps;

public sealed class RoadmapNode : BaseEntity
{
    private RoadmapNode() { }

    public RoadmapNode(Guid roadmapId, string title, string slug,
        RoadmapNodeType nodeType = RoadmapNodeType.Topic, decimal positionX = 0,
        decimal positionY = 0, decimal? width = null, int sortOrder = 0)
    {
        RoadmapId = RoadmapValidation.RequiredId(roadmapId, nameof(roadmapId));
        Title = RoadmapValidation.RequiredText(title, 150, nameof(title));
        Slug = RoadmapValidation.Slug(slug);
        NodeType = RoadmapValidation.DefinedEnum(nodeType);
        ValidateLayout(positionX, positionY, width);
        PositionX = positionX;
        PositionY = positionY;
        Width = width;
        SortOrder = RoadmapValidation.SortOrder(sortOrder);
    }

    public Guid RoadmapId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; set; }
    public string? LearningObjectives { get; set; }
    public string? EstimatedDuration { get; set; }
    public RoadmapNodeType NodeType { get; private set; }
    public decimal PositionX { get; private set; }
    public decimal PositionY { get; private set; }
    public decimal? Width { get; private set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Roadmap Roadmap { get; set; } = null!;
    public ICollection<RoadmapEdge> OutgoingEdges { get; private set; } = new List<RoadmapEdge>();
    public ICollection<RoadmapEdge> IncomingEdges { get; private set; } = new List<RoadmapEdge>();
    public ICollection<LearningResource> Resources { get; private set; } = new List<LearningResource>();

    public void SetPosition(decimal positionX, decimal positionY, decimal? width = null)
    {
        ValidateLayout(positionX, positionY, width);
        PositionX = positionX;
        PositionY = positionY;
        Width = width;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static void ValidateLayout(decimal x, decimal y, decimal? width)
    {
        // Matches numeric(18, 4); negative canvas coordinates are valid.
        const decimal maxValue = 99999999999999.9999m;
        if (x < -maxValue || x > maxValue || y < -maxValue || y > maxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(x), "Coordinates exceed the supported canvas range.");
        }

        if (width is <= 0 or > maxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive and within the supported range.");
        }
    }
    public void Update(string title, string slug, RoadmapNodeType nodeType,
        decimal positionX, decimal positionY, decimal? width, int sortOrder)
    {
        var validated = new RoadmapNode(RoadmapId, title, slug, nodeType, positionX, positionY, width, sortOrder);
        Title = validated.Title;
        Slug = validated.Slug;
        NodeType = validated.NodeType;
        PositionX = validated.PositionX;
        PositionY = validated.PositionY;
        Width = validated.Width;
        SortOrder = validated.SortOrder;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

}

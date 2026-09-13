using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;

namespace GdscSharingPlatform.UnitTests.Domain;

public class RoadmapTests
{
    private static Roadmap CreateRoadmap(string title = "Frontend", string slug = "frontend", int sortOrder = 0)
        => new(Guid.NewGuid(), title, slug, "Learn frontend", RoadmapLevel.Beginner, Guid.NewGuid(), sortOrder);

    [Fact]
    public void NewRoadmap_IsPublishedWithFirstPublicationTimestamp()
    {
        var roadmap = CreateRoadmap();
        Assert.Equal(RoadmapStatus.Published, roadmap.Status);
        Assert.Equal(roadmap.CreatedAtUtc, roadmap.PublishedAtUtc);
        Assert.Null(roadmap.UpdatedAtUtc);
        Assert.Empty(roadmap.Nodes);
        Assert.Empty(roadmap.Edges);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Roadmap_RejectsBlankTitle(string title)
        => Assert.Throws<ArgumentException>(() => CreateRoadmap(title));

    [Fact]
    public void Roadmap_RejectsLongTitleAndNegativeOrder()
    {
        Assert.Throws<ArgumentException>(() => CreateRoadmap(new string('a', 151)));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateRoadmap(sortOrder: -1));
    }

    [Theory]
    [InlineData("  Lộ trình Frontend  ", "lo-trinh-frontend")]
    [InlineData("Đường---Dẫn / HTML", "duong-dan-html")]
    public void Slugs_AreNormalizedAcrossEntities(string input, string expected)
    {
        Assert.Equal(expected, CreateRoadmap(slug: input).Slug);
        Assert.Equal(expected, new RoadmapCategory("Category", input).Slug);
        Assert.Equal(expected, new RoadmapNode(Guid.NewGuid(), "Node", input).Slug);
    }

    [Fact]
    public void Entities_RejectMissingIdsAndUnknownEnums()
    {
        Assert.Throws<ArgumentException>(() => new Roadmap(Guid.Empty, "Title", "slug", "Summary", RoadmapLevel.Beginner, Guid.NewGuid()));
        Assert.Throws<ArgumentException>(() => new RoadmapNode(Guid.Empty, "Node", "node"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Roadmap(Guid.NewGuid(), "Title", "slug", "Summary", (RoadmapLevel)99, Guid.NewGuid()));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RoadmapNode(Guid.NewGuid(), "Node", "node", (RoadmapNodeType)99));
        Assert.Throws<ArgumentException>(() => new RoadmapCategory("Category", "!!!"));
    }

    [Fact]
    public void Node_AllowsNegativeCoordinatesAndRejectsInvalidLayout()
    {
        var node = new RoadmapNode(Guid.NewGuid(), "Node", "node", positionX: -120.5m, positionY: 320.25m);
        Assert.Equal(-120.5m, node.PositionX);
        node.SetPosition(10.5m, -20.25m, 200m);
        Assert.Equal(-20.25m, node.PositionY);
        Assert.Equal(200m, node.Width);
        Assert.Throws<ArgumentOutOfRangeException>(() => node.SetPosition(0, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => node.SetPosition(decimal.MaxValue, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => node.SetPosition(0, decimal.MinValue));
        Assert.Equal(10.5m, node.PositionX);
    }

    [Fact]
    public void Edge_RejectsSelfLoopAndDashedRequiredRelation()
    {
        var nodeId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => new RoadmapEdge(Guid.NewGuid(), nodeId, nodeId));
        Assert.Throws<ArgumentException>(() => new RoadmapEdge(Guid.NewGuid(), nodeId, Guid.NewGuid(), lineStyle: RoadmapLineStyle.Dashed));
        var edge = new RoadmapEdge(Guid.NewGuid(), nodeId, Guid.NewGuid(), RoadmapRelationType.Optional, RoadmapLineStyle.Dashed);
        Assert.Equal(RoadmapLineStyle.Dashed, edge.LineStyle);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("file:///tmp/test.pdf")]
    [InlineData("ftp://example.com/file")]
    [InlineData("/relative/path")]
    [InlineData("https://")]
    public void Link_RejectsInvalidOrNonHttpUrls(string url)
        => Assert.Throws<ArgumentException>(() => LearningResource.CreateLink(Guid.NewGuid(), "Title", url, Guid.NewGuid()));

    [Theory]
    [InlineData("https://example.com/document")]
    [InlineData("http://example.com/document")]
    public void Link_HasNoFileMetadata(string url)
    {
        var link = LearningResource.CreateLink(Guid.NewGuid(), "Title", url, Guid.NewGuid());
        Assert.Equal(ResourceType.Link, link.ResourceType);
        Assert.Equal(url, link.ExternalUrl);
        Assert.Null(link.OriginalFileName);
        Assert.Null(link.StoredFileName);
        Assert.Null(link.StorageKey);
        Assert.Null(link.FileSize);
        Assert.Null(link.ContentType);
    }

    [Fact]
    public void File_RequiresMetadataAndHasNoExternalUrl()
    {
        var file = LearningResource.CreateFile(Guid.NewGuid(), "Title", "Guide.pdf", "stored.pdf", "resources/stored.pdf", 123, "application/pdf", Guid.NewGuid());
        Assert.Equal(ResourceType.File, file.ResourceType);
        Assert.Null(file.ExternalUrl);
        Assert.Equal(123L, file.FileSize);
        Assert.Throws<ArgumentException>(() => LearningResource.CreateFile(Guid.NewGuid(), "Title", "Guide.pdf", "stored.pdf", "", 123, "application/pdf", Guid.NewGuid()));
        Assert.Throws<ArgumentOutOfRangeException>(() => LearningResource.CreateFile(Guid.NewGuid(), "Title", "Guide.pdf", "stored.pdf", "key", -1, "application/pdf", Guid.NewGuid()));
    }

    [Fact]
    public void Deactivation_PreservesRelatedContent()
    {
        var roadmap = CreateRoadmap();
        var node = new RoadmapNode(roadmap.Id, "Node", "node");
        var resource = LearningResource.CreateLink(node.Id, "Guide", "https://example.com", Guid.NewGuid());
        var edge = new RoadmapEdge(roadmap.Id, node.Id, Guid.NewGuid());
        roadmap.Nodes.Add(node);
        roadmap.Edges.Add(edge);
        node.Resources.Add(resource);
        node.OutgoingEdges.Add(edge);
        node.SetActive(false);
        Assert.False(node.IsActive);
        Assert.Single(node.Resources);
        Assert.Single(node.OutgoingEdges);
        Assert.Single(roadmap.Nodes);
        Assert.Single(roadmap.Edges);
        Assert.True(resource.IsActive);
        Assert.True(edge.IsActive);
    }
}

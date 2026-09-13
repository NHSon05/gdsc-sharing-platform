using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;

namespace GdscSharingPlatform.UnitTests.Domain;

public sealed class RoadmapMutationTests
{
    [Fact]
    public void RejectedMetadataUpdateLeavesRoadmapAndAuditUnchanged()
    {
        var author = Guid.NewGuid();
        var roadmap = new Roadmap(Guid.NewGuid(), "Original", "original", "Summary", RoadmapLevel.Beginner, author);
        var originalId = roadmap.Id;
        Assert.Throws<ArgumentException>(() => roadmap.Update(roadmap.CategoryId, "New title", "new-slug", "Summary", RoadmapLevel.Advanced, 1, Guid.Empty));
        Assert.Equal("Original", roadmap.Title); Assert.Equal("original", roadmap.Slug);
        Assert.Null(roadmap.UpdatedAtUtc); Assert.Null(roadmap.UpdatedByUserId);
        Assert.Throws<ArgumentOutOfRangeException>(() => roadmap.Reorder(-1, author));
        Assert.Equal(0, roadmap.SortOrder);
        Assert.Equal(originalId, roadmap.Id);
    }

    [Fact]
    public void RejectedNodeAndEdgeUpdatesDoNotPartiallyMutateGraph()
    {
        var roadmap = Guid.NewGuid(); var source = Guid.NewGuid(); var target = Guid.NewGuid();
        var node = new RoadmapNode(roadmap, "Original", "original", positionX: 10, width: 200);
        Assert.Throws<ArgumentOutOfRangeException>(() => node.Update("New", "new", RoadmapNodeType.Group, 99, 0, -1, 0));
        Assert.Equal("Original", node.Title); Assert.Equal(10, node.PositionX); Assert.Equal(200, node.Width);
        Assert.Null(node.UpdatedAtUtc);
        var edge = new RoadmapEdge(roadmap, source, target);
        Assert.Throws<ArgumentException>(() => edge.Update(target, target, RoadmapRelationType.Optional, RoadmapLineStyle.Dashed, 1));
        Assert.Equal(source, edge.SourceNodeId); Assert.Equal(target, edge.TargetNodeId);
        Assert.Equal(RoadmapRelationType.Required, edge.RelationType); Assert.Null(edge.UpdatedAtUtc);
    }

    [Theory]
    [InlineData("original")]
    [InlineData("stored")]
    [InlineData("key")]
    [InlineData("mime")]
    public void MissingFileMetadataPreventsCreationAndReplacement(string missing)
    {
        var resource = LearningResource.CreateFile(Guid.NewGuid(), "PDF", "old.pdf", "old.pdf", "old-key", 10, "application/pdf", Guid.NewGuid());
        var original = missing == "original" ? "" : "new.pdf";
        var stored = missing == "stored" ? "" : "new.pdf";
        var key = missing == "key" ? "" : "new-key";
        var mime = missing == "mime" ? "" : "application/pdf";
        Assert.Throws<ArgumentException>(() => LearningResource.CreateFile(resource.RoadmapNodeId, "PDF", original, stored, key, 20, mime, resource.CreatedByUserId));
        Assert.Throws<ArgumentException>(() => resource.ReplaceFile(original, stored, key, 20, mime));
        Assert.Equal("old-key", resource.StorageKey); Assert.Equal(10, resource.FileSize);
        Assert.Null(resource.UpdatedAtUtc);
    }

    [Fact]
    public void RejectedLinkUpdatePreservesOldUrlAndTitle()
    {
        var resource = LearningResource.CreateLink(Guid.NewGuid(), "Original", "https://example.com", Guid.NewGuid());
        Assert.Throws<ArgumentException>(() => resource.Update("New", "New description", "javascript:alert(1)", 1));
        Assert.Equal("Original", resource.Title); Assert.Equal("https://example.com", resource.ExternalUrl);
        Assert.Null(resource.Description); Assert.Null(resource.UpdatedAtUtc);
    }

    [Theory]
    [InlineData(RoadmapStatus.Draft)]
    [InlineData(RoadmapStatus.Archived)]
    public void FailedPublishPreservesStateAndFirstPublicationTimestamp(RoadmapStatus previous)
    {
        var author = Guid.NewGuid();
        var roadmap = new Roadmap(Guid.NewGuid(), "Roadmap", "roadmap", "Summary", RoadmapLevel.Beginner, author);
        var published = roadmap.PublishedAtUtc;
        roadmap.ChangeStatus(previous, false, author);
        var updated = roadmap.UpdatedAtUtc;
        Assert.Throws<InvalidOperationException>(() => roadmap.ChangeStatus(RoadmapStatus.Published, false, author));
        Assert.Equal(previous, roadmap.Status); Assert.Equal(updated, roadmap.UpdatedAtUtc);
        Assert.Equal(published, roadmap.PublishedAtUtc);
    }
}

using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using GdscSharingPlatform.Application.Features.Roadmaps.Rules;
using GdscSharingPlatform.Application.Features.Roadmaps.Validators;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;

namespace GdscSharingPlatform.UnitTests.Features.Roadmaps;

public class RoadmapApplicationTests
{
    [Fact]
    public void PublishRequiresNodes_ArchivePreservesContentAndFirstPublicationDate()
    {
        var author = Guid.NewGuid();
        var roadmap = new Roadmap(Guid.NewGuid(), "Roadmap", "roadmap", "Summary", RoadmapLevel.AllLevels, author);
        var published = roadmap.PublishedAtUtc;
        roadmap.ChangeStatus(RoadmapStatus.Draft, false, author);
        Assert.Throws<InvalidOperationException>(() => roadmap.ChangeStatus(RoadmapStatus.Published, false, author));
        roadmap.Nodes.Add(new RoadmapNode(roadmap.Id, "Node", "node"));
        roadmap.ChangeStatus(RoadmapStatus.Published, true, author);
        roadmap.ChangeStatus(RoadmapStatus.Archived, true, author);
        Assert.Single(roadmap.Nodes);
        Assert.Equal(published, roadmap.PublishedAtUtc);
        Assert.Equal(author, roadmap.UpdatedByUserId);
        Assert.True(RoadmapRules.IsMemberVisible(roadmap.Status));
        Assert.Throws<ConflictException>(() => RoadmapRules.EnsureCanAdd(roadmap.Status));
    }

    [Fact]
    public void RequiredCycleDetection_HandlesBranchesAndExistingUnrelatedCycles()
    {
        var a = Guid.NewGuid(); var b = Guid.NewGuid(); var c = Guid.NewGuid(); var d = Guid.NewGuid();
        (Guid, Guid)[] edges = [(a, b), (b, c), (b, d), (d, b)];
        Assert.True(RoadmapRules.CreatesRequiredCycle(edges, c, a));
        Assert.False(RoadmapRules.CreatesRequiredCycle(edges, a, c));
        Assert.False(RoadmapRules.CreatesRequiredCycle(edges, Guid.NewGuid(), a));
    }

    [Fact]
    public void ReorderRejectsDuplicatesMissingAndForeignIds()
    {
        var a = Guid.NewGuid(); var b = Guid.NewGuid();
        Assert.Throws<ApplicationValidationException>(() => RoadmapRules.EnsureCompleteReorder([a, b], [a, a]));
        Assert.Throws<ApplicationValidationException>(() => RoadmapRules.EnsureCompleteReorder([a, b], [a]));
        Assert.Throws<ApplicationValidationException>(() => RoadmapRules.EnsureCompleteReorder([a, b], [a, Guid.NewGuid()]));
        RoadmapRules.EnsureCompleteReorder([a, b], [b, a]);
        Assert.False(new ReorderRequestValidator().Validate(new ReorderRequest([a, a])).IsValid);
    }

    [Fact]
    public void ValidatorsRejectInvalidInputsBeforeDomainOrPersistence()
    {
        Assert.False(new CategoryRequestValidator().Validate(new CategoryRequest("", "!!!", SortOrder: -1)).IsValid);
        Assert.False(new RoadmapRequestValidator().Validate(new RoadmapRequest(Guid.Empty, new string('x', 151), "r", "", (RoadmapLevel)999)).IsValid);
        Assert.False(new NodeRequestValidator().Validate(new NodeRequest("node", "node", Width: 0)).IsValid);
        Assert.False(new NodeRequestValidator().Validate(new NodeRequest("node", "node", PositionX: decimal.MaxValue)).IsValid);
        var node = Guid.NewGuid();
        Assert.False(new EdgeRequestValidator().Validate(new EdgeRequest(node, node)).IsValid);
        Assert.False(new EdgeRequestValidator().Validate(new EdgeRequest(node, Guid.NewGuid(), LineStyle: RoadmapLineStyle.Dashed)).IsValid);
        Assert.False(new RoadmapStatusRequestValidator().Validate(new RoadmapStatusRequest(null)).IsValid);
        Assert.False(new ActiveStatusRequestValidator().Validate(new ActiveStatusRequest(null)).IsValid);
        Assert.False(new NodePositionsRequestValidator().Validate(new NodePositionsRequest([new(node, 0, 0), new(node, 1, 2)])).IsValid);
        Assert.False(new NodePositionsRequestValidator().Validate(new NodePositionsRequest([null!])).IsValid);
        Assert.False(new RoadmapQueryValidator().Validate(new RoadmapQuery() { PageSize = 101 }).IsValid);
        Assert.False(new FileResourceRequestValidator().Validate(new FileResourceRequest("", SortOrder: -1)).IsValid);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,hello")]
    [InlineData("//example.com")]
    [InlineData("file:///tmp/a")]
    public void ResourceValidatorRejectsUnsafeUrls(string url)
        => Assert.False(new LinkResourceRequestValidator().Validate(new LinkResourceRequest("Guide", url)).IsValid);

    [Fact]
    public void ResourceUpdateCannotMixLinkAndFileMetadata()
    {
        var node = Guid.NewGuid(); var author = Guid.NewGuid();
        var file = LearningResource.CreateFile(node, "PDF", "a.pdf", "a.pdf", "a.pdf", 10, "application/pdf", author);
        Assert.Throws<ArgumentException>(() => file.Update("PDF", null, "https://example.com", 0));
        var link = LearningResource.CreateLink(node, "Guide", "https://example.com", author);
        Assert.Throws<InvalidOperationException>(() => link.ReplaceFile("a.pdf", "a.pdf", "a.pdf", 10, "application/pdf"));
    }
}

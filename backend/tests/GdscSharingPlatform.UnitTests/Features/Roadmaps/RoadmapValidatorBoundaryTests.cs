using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using GdscSharingPlatform.Application.Features.Roadmaps.Validators;
using GdscSharingPlatform.Domain.Enums;

namespace GdscSharingPlatform.UnitTests.Features.Roadmaps;

public sealed class RoadmapValidatorBoundaryTests
{
    private static RoadmapRequest ValidRoadmap() => new(Guid.NewGuid(), "Roadmap", "roadmap", "Summary", RoadmapLevel.Beginner);

    [Theory]
    [InlineData(0, false)]
    [InlineData(150, true)]
    [InlineData(151, false)]
    public void TitleLimits_AreConsistentAcrossCategoryRoadmapNodeAndResources(int length, bool valid)
    {
        var title = new string('a', length);
        Assert.Equal(valid, new CategoryRequestValidator().Validate(new CategoryRequest(title, "category")).IsValid);
        Assert.Equal(valid, new RoadmapRequestValidator().Validate(ValidRoadmap() with { Title = title }).IsValid);
        Assert.Equal(valid, new NodeRequestValidator().Validate(new NodeRequest(title, "node")).IsValid);
        Assert.Equal(valid, new LinkResourceRequestValidator().Validate(new LinkResourceRequest(title, "https://example.com")).IsValid);
        Assert.Equal(valid, new FileResourceRequestValidator().Validate(new FileResourceRequest(title)).IsValid);
        Assert.Equal(valid, new UpdateResourceRequestValidator().Validate(new UpdateResourceRequest(title)).IsValid);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("---", false)]
    [InlineData("Đường dẫn", true)]
    [InlineData("HTML---CSS", true)]
    public void SlugValidationUsesNormalizedValue(string slug, bool valid)
    {
        Assert.Equal(valid, new CategoryRequestValidator().Validate(new CategoryRequest("Category", slug)).IsValid);
        Assert.Equal(valid, new RoadmapRequestValidator().Validate(ValidRoadmap() with { Slug = slug }).IsValid);
        Assert.Equal(valid, new NodeRequestValidator().Validate(new NodeRequest("Node", slug)).IsValid);
    }

    [Theory]
    [InlineData(150, true)]
    [InlineData(151, false)]
    public void NormalizedSlugLength_IsBounded(int length, bool valid)
        => Assert.Equal(valid, new RoadmapRequestValidator().Validate(ValidRoadmap() with { Slug = new string('a', length) }).IsValid);

    [Theory]
    [InlineData(0, false)]
    [InlineData(500, true)]
    [InlineData(501, false)]
    public void ShortDescriptionLength_IsBounded(int length, bool valid)
        => Assert.Equal(valid, new RoadmapRequestValidator().Validate(ValidRoadmap() with { ShortDescription = new string('a', length) }).IsValid);

    [Theory]
    [InlineData(-1)]
    [InlineData(99)]
    public void EveryEnumRejectsUndefinedValues(int value)
    {
        Assert.False(new RoadmapRequestValidator().Validate(ValidRoadmap() with { Level = (RoadmapLevel)value }).IsValid);
        Assert.False(new RoadmapStatusRequestValidator().Validate(new RoadmapStatusRequest((RoadmapStatus)value)).IsValid);
        Assert.False(new NodeRequestValidator().Validate(new NodeRequest("Node", "node", (RoadmapNodeType)value)).IsValid);
        Assert.False(new EdgeRequestValidator().Validate(new EdgeRequest(Guid.NewGuid(), Guid.NewGuid(), (RoadmapRelationType)value)).IsValid);
        Assert.False(new EdgeRequestValidator().Validate(new EdgeRequest(Guid.NewGuid(), Guid.NewGuid(), RoadmapRelationType.Optional, (RoadmapLineStyle)value)).IsValid);
        Assert.False(new RoadmapQueryValidator().Validate(new RoadmapQuery { Level = (RoadmapLevel)value }).IsValid);
        Assert.False(new RoadmapQueryValidator().Validate(new RoadmapQuery { Status = (RoadmapStatus)value }).IsValid);
    }

    [Fact]
    public void ValidEnumsAndFalseStatus_AreAccepted()
    {
        foreach (var level in Enum.GetValues<RoadmapLevel>())
            Assert.True(new RoadmapRequestValidator().Validate(ValidRoadmap() with { Level = level }).IsValid);
        foreach (var status in Enum.GetValues<RoadmapStatus>())
            Assert.True(new RoadmapStatusRequestValidator().Validate(new RoadmapStatusRequest(status)).IsValid);
        foreach (var type in Enum.GetValues<RoadmapNodeType>())
            Assert.True(new NodeRequestValidator().Validate(new NodeRequest("Node", "node", type)).IsValid);
        Assert.True(new ActiveStatusRequestValidator().Validate(new ActiveStatusRequest(false)).IsValid);
        Assert.True(new RoadmapQueryValidator().Validate(new RoadmapQuery()).IsValid);
    }

    [Theory]
    [InlineData("https://example.com/guide?q=1", true)]
    [InlineData("http://example.com", true)]
    [InlineData("https://", false)]
    [InlineData("/relative", false)]
    [InlineData("   ", false)]
    public void LinkAndResourceUpdateValidateUrls(string url, bool valid)
    {
        Assert.Equal(valid, new LinkResourceRequestValidator().Validate(new LinkResourceRequest("Guide", url)).IsValid);
        Assert.Equal(valid, new UpdateResourceRequestValidator().Validate(new UpdateResourceRequest("Guide", ExternalUrl: url)).IsValid);
    }

    [Theory]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("//example.com/image.png", false)]
    [InlineData("/\\example.com/image.png", false)]
    [InlineData("/uploads/image.png", true)]
    [InlineData("https://example.com/image.png", true)]
    public void ThumbnailUrlsCannotUseUnsafeSchemesOrProtocolRelativePaths(string url, bool valid)
        => Assert.Equal(valid, new RoadmapRequestValidator().Validate(ValidRoadmap() with { ThumbnailUrl = url }).IsValid);

    [Fact]
    public void ReorderAndPositionsRejectNullEmptyAndForeignShapeBeforeUse()
    {
        var reorder = new ReorderRequestValidator();
        Assert.False(reorder.Validate(new ReorderRequest(null!)).IsValid);
        Assert.False(reorder.Validate(new ReorderRequest([])).IsValid);
        Assert.False(reorder.Validate(new ReorderRequest([Guid.Empty])).IsValid);
        var positions = new NodePositionsRequestValidator();
        Assert.False(positions.Validate(new NodePositionsRequest(null!)).IsValid);
        Assert.False(positions.Validate(new NodePositionsRequest([])).IsValid);
        Assert.False(positions.Validate(new NodePositionsRequest([new(Guid.Empty, 0, 0)])).IsValid);
        Assert.True(positions.Validate(new NodePositionsRequest([new(Guid.NewGuid(), -120.25m, 340.5m)])).IsValid);
    }

    [Fact]
    public void MetadataLengthLimits_AreEnforcedAcrossRequests()
    {
        var longNarrative = new string('x', 100001);
        Assert.True(new CategoryRequestValidator().Validate(new CategoryRequest("Category", "category",
            new string('d', 2000), new string('i', 2048), new string('c', 50))).IsValid);
        Assert.False(new CategoryRequestValidator().Validate(new CategoryRequest("Category", "category", Description: new string('d', 2001))).IsValid);
        Assert.False(new CategoryRequestValidator().Validate(new CategoryRequest("Category", "category", Icon: new string('i', 2049))).IsValid);
        Assert.False(new CategoryRequestValidator().Validate(new CategoryRequest("Category", "category", Color: new string('c', 51))).IsValid);
        Assert.False(new RoadmapRequestValidator().Validate(ValidRoadmap() with { Description = longNarrative }).IsValid);
        Assert.False(new RoadmapRequestValidator().Validate(ValidRoadmap() with { Prerequisites = longNarrative }).IsValid);
        Assert.False(new RoadmapRequestValidator().Validate(ValidRoadmap() with { EstimatedDuration = new string('h', 101) }).IsValid);
        Assert.False(new RoadmapRequestValidator().Validate(ValidRoadmap() with { ThumbnailUrl = "/" + new string('t', 2048) }).IsValid);
        Assert.False(new NodeRequestValidator().Validate(new NodeRequest("Node", "node", Description: longNarrative)).IsValid);
        Assert.False(new NodeRequestValidator().Validate(new NodeRequest("Node", "node", LearningObjectives: longNarrative)).IsValid);
        Assert.False(new NodeRequestValidator().Validate(new NodeRequest("Node", "node", EstimatedDuration: new string('h', 101))).IsValid);
        Assert.False(new NodeRequestValidator().Validate(new NodeRequest("Node", "node", Color: new string('c', 51))).IsValid);
        Assert.False(new NodeRequestValidator().Validate(new NodeRequest("Node", "node", Icon: new string('i', 2049))).IsValid);
        Assert.False(new EdgeRequestValidator().Validate(new EdgeRequest(Guid.NewGuid(), Guid.NewGuid(), Label: new string('l', 251))).IsValid);
        Assert.False(new LinkResourceRequestValidator().Validate(new LinkResourceRequest("Guide", "https://example.com", new string('d', 2001))).IsValid);
        Assert.False(new FileResourceRequestValidator().Validate(new FileResourceRequest("Guide", new string('d', 2001))).IsValid);
        Assert.False(new UpdateResourceRequestValidator().Validate(new UpdateResourceRequest("Guide", new string('d', 2001))).IsValid);
    }

    [Fact]
    public void BulkOperationValidatorsLimitPayloadSize()
    {
        var ids = Enumerable.Range(0, 10000).Select(_ => Guid.NewGuid()).ToArray();
        var tooManyIds = ids.Append(Guid.NewGuid()).ToArray();
        var nodes = ids.Select(id => new NodePositionRequest(id, 0, 0)).ToArray();
        var tooManyNodes = tooManyIds.Select(id => new NodePositionRequest(id, 0, 0)).ToArray();
        Assert.True(new ReorderRequestValidator().Validate(new ReorderRequest(ids)).IsValid);
        Assert.False(new ReorderRequestValidator().Validate(new ReorderRequest(tooManyIds)).IsValid);
        Assert.True(new NodePositionsRequestValidator().Validate(new NodePositionsRequest(nodes)).IsValid);
        Assert.False(new NodePositionsRequestValidator().Validate(new NodePositionsRequest(tooManyNodes)).IsValid);
    }

    [Theory]
    [InlineData(0, 12, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, 101, false)]
    [InlineData(1000001, 12, false)]
    [InlineData(1000000, 100, true)]
    public void PaginationHasBoundedOffsetAndSize(int page, int pageSize, bool valid)
        => Assert.Equal(valid, new RoadmapQueryValidator().Validate(new RoadmapQuery { Page = page, PageSize = pageSize }).IsValid);
}

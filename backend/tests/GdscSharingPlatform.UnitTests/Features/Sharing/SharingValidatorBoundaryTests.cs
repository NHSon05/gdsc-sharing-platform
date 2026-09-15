using GdscSharingPlatform.Application.Features.Sharing;
using GdscSharingPlatform.Domain.Enums;

namespace GdscSharingPlatform.UnitTests.Features.Sharing;

public sealed class SharingValidatorBoundaryTests
{
    [Theory]
    [InlineData(200, 1000, 100000, true)]
    [InlineData(201, 1000, 100000, false)]
    [InlineData(200, 1001, 100000, false)]
    [InlineData(200, 1000, 100001, false)]
    [InlineData(0, 1, 1, false)]
    public void ContentLengthBoundaries(int title, int summary, int body, bool valid)
    {
        var request = new ContentRequest(new('a', title), "slug", new('a', summary), new('a', body), [], []);
        Assert.Equal(valid, new ContentRequestValidator().Validate(request).IsValid);
    }

    [Theory]
    [InlineData(DeliveryMode.Online, null, "https://example.com", true)]
    [InlineData(DeliveryMode.Online, null, null, false)]
    [InlineData(DeliveryMode.Offline, "Room", null, true)]
    [InlineData(DeliveryMode.Offline, null, null, false)]
    [InlineData(DeliveryMode.Hybrid, "Room", "https://example.com", true)]
    [InlineData(DeliveryMode.Hybrid, "Room", null, false)]
    [InlineData(DeliveryMode.Hybrid, null, "https://example.com", false)]
    public void DeliveryFields(DeliveryMode mode, string? location, string? url, bool valid)
    {
        Assert.Equal(valid, new ScheduleRequestValidator().Validate(Schedule() with
        { DeliveryMode = mode, Location = location, MeetingUrl = url }).IsValid);
    }

    [Fact]
    public void ScheduleRejectsInvalidEnumsTimesAndNullCollections()
    {
        var request = Schedule();
        var validator = new ScheduleRequestValidator();
        Assert.False(validator.Validate(request with { EndsAtLocal = request.StartsAtLocal }).IsValid);
        Assert.False(validator.Validate(request with { DeliveryMode = (DeliveryMode)999 }).IsValid);
        Assert.False(validator.Validate(request with { SharingType = (SharingType)999 }).IsValid);
        Assert.False(validator.Validate(request with { Presenters = null! }).IsValid);
        Assert.False(validator.Validate(request with { ContentIds = null! }).IsValid);
        Assert.False(validator.Validate(request with { GenerationIds = null! }).IsValid);
    }

    [Theory]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void AssociationLimit(int count, bool valid)
    {
        var ids = Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToArray();
        Assert.Equal(valid, new ScheduleContentsRequestValidator().Validate(new ScheduleContentsRequest(ids)).IsValid);
        Assert.Equal(valid, new ContentRequestValidator().Validate(new ContentRequest("Title", "slug", "Summary", "Body", ids, [])).IsValid);
    }

    [Theory]
    [InlineData(0, 20, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, 101, false)]
    [InlineData(100000, 100, true)]
    public void QueryPaginationBounds(int page, int pageSize, bool valid)
    {
        Assert.Equal(valid, new ContentQueryValidator().Validate(new ContentQuery { Page = page, PageSize = pageSize }).IsValid);
        Assert.Equal(valid, new ScheduleQueryValidator().Validate(new ScheduleQuery { Page = page, PageSize = pageSize }).IsValid);
    }

    private static ScheduleRequest Schedule() => new("Talk", SharingType.TechTalk, DeliveryMode.Online,
        new(2026, 10, 1, 14, 0, 0), new(2026, 10, 1, 15, 0, 0), "Asia/Ho_Chi_Minh",
        AudienceScope.AllMembers, [], [], [], [], MeetingUrl: "https://example.com");
}

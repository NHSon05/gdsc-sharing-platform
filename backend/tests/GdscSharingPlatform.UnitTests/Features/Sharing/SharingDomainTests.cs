using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Sharing;

namespace GdscSharingPlatform.UnitTests.Features.Sharing;

public sealed class SharingDomainTests
{
    private static readonly Guid Actor = Guid.NewGuid();

    public static IEnumerable<object[]> ContentTransitions()
    {
        foreach (var state in Enum.GetValues<SharingContentStatus>())
        foreach (var action in new[] { "submit", "withdraw", "approve", "reject", "archive", "return" })
            yield return [state, action];
    }

    [Theory]
    [MemberData(nameof(ContentTransitions))]
    public void ContentTransitionMatrix(SharingContentStatus state, string action)
    {
        var content = ContentAt(state);
        SharingContentStatus? target = action switch
        {
            "submit" when state is SharingContentStatus.Draft or SharingContentStatus.Rejected => SharingContentStatus.PendingReview,
            "withdraw" when state == SharingContentStatus.PendingReview => SharingContentStatus.Draft,
            "approve" when state == SharingContentStatus.PendingReview => SharingContentStatus.Published,
            "reject" when state == SharingContentStatus.PendingReview => SharingContentStatus.Rejected,
            "archive" when state is SharingContentStatus.Draft or SharingContentStatus.Rejected or SharingContentStatus.Published => SharingContentStatus.Archived,
            "return" when state is SharingContentStatus.Published or SharingContentStatus.Archived => SharingContentStatus.Draft,
            _ => null
        };
        var version = content.Version;
        void Act()
        {
            switch (action)
            {
                case "submit": content.Submit(Actor); break;
                case "withdraw": content.Withdraw(Actor); break;
                case "approve": content.Approve(Actor); break;
                case "reject": content.Reject(Actor, "Needs examples"); break;
                case "archive": content.Archive(Actor); break;
                case "return": content.ReturnToDraft(Actor); break;
            }
        }
        if (target.HasValue) Act(); else Assert.Throws<InvalidOperationException>(Act);
        Assert.Equal(target ?? state, content.Status);
        Assert.Equal(version + (target.HasValue ? 1 : 0), content.Version);
    }

    public static IEnumerable<object[]> ScheduleTransitions()
    {
        foreach (var state in Enum.GetValues<SharingScheduleStatus>())
        foreach (var action in new[] { "publish", "start", "complete", "cancel" })
            yield return [state, action];
    }

    [Theory]
    [MemberData(nameof(ScheduleTransitions))]
    public void ScheduleTransitionMatrix(SharingScheduleStatus state, string action)
    {
        var schedule = ScheduleAt(state);
        SharingScheduleStatus? target = action switch
        {
            "publish" when state == SharingScheduleStatus.Draft => SharingScheduleStatus.Scheduled,
            "start" when state == SharingScheduleStatus.Scheduled => SharingScheduleStatus.Ongoing,
            "complete" when state == SharingScheduleStatus.Ongoing => SharingScheduleStatus.Completed,
            "cancel" when state is SharingScheduleStatus.Draft or SharingScheduleStatus.Scheduled => SharingScheduleStatus.Cancelled,
            _ => null
        };
        var version = schedule.Version;
        void Act()
        {
            switch (action)
            {
                case "publish": schedule.Publish(Actor); break;
                case "start": schedule.Start(Actor); break;
                case "complete": schedule.Complete(Actor); break;
                case "cancel": schedule.Cancel("Unavailable", Actor); break;
            }
        }
        if (target.HasValue) Act(); else Assert.Throws<InvalidOperationException>(Act);
        Assert.Equal(target ?? state, schedule.Status);
        Assert.Equal(version + (target.HasValue ? 1 : 0), schedule.Version);
    }

    [Fact]
    public void ContentOwnerReviewMetadataAndResubmission()
    {
        var content = ContentAt(SharingContentStatus.Draft);
        Assert.Equal(Actor, Assert.Single(content.Authors).UserId);
        Assert.Equal(SharingAuthorRole.Owner, Assert.Single(content.Authors).AuthorRole);
        content.Submit(Actor);
        Assert.NotNull(content.SubmittedAtUtc);
        Assert.Throws<ArgumentException>(() => content.Reject(Actor, " "));
        content.Reject(Actor, "Examples");
        Assert.Equal(Actor, content.ReviewedByUserId);
        Assert.NotNull(content.ReviewedAtUtc);
        content.Submit(Actor);
        Assert.Null(content.ReviewNote);
        content.Approve(Actor);
        var published = content.PublishedAtUtc;
        Assert.NotNull(published);
        content.Update("New title", "new-title", "Summary", "New body", Actor);
        Assert.Equal(published, content.PublishedAtUtc);
        Assert.Equal(SharingContentStatus.Published, content.Status);
    }

    [Theory]
    [InlineData(SharingScheduleStatus.Completed)]
    [InlineData(SharingScheduleStatus.Cancelled)]
    public void TerminalScheduleCannotBeUpdated(SharingScheduleStatus status)
    {
        var schedule = ScheduleAt(status);
        var version = schedule.Version;
        Assert.Throws<InvalidOperationException>(() => schedule.Update("Changed", schedule.SharingType,
            schedule.DeliveryMode, schedule.StartsAtUtc, schedule.EndsAtUtc, "UTC", schedule.AudienceScope,
            Actor, meetingUrl: "https://meet.example.com"));
        Assert.Equal(version, schedule.Version);
        Assert.Equal("Talk", schedule.Title);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ScheduleRejectsNonPositiveDuration(int hours)
    {
        var start = new DateTimeOffset(2026, 10, 1, 7, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentException>(() => new SharingSchedule("Talk", SharingType.TechTalk,
            DeliveryMode.Online, start, start.AddHours(hours), "UTC", AudienceScope.AllMembers,
            Actor, meetingUrl: "https://example.com"));
    }

    [Theory]
    [InlineData(DeliveryMode.Online, null, null)]
    [InlineData(DeliveryMode.Offline, null, null)]
    [InlineData(DeliveryMode.Hybrid, "Room", null)]
    [InlineData(DeliveryMode.Hybrid, null, "https://example.com")]
    public void ScheduleDomainRequiresDeliveryFields(DeliveryMode mode, string? location, string? url)
    {
        var start = new DateTimeOffset(2026, 10, 1, 7, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentException>(() => new SharingSchedule("Talk", SharingType.TechTalk,
            mode, start, start.AddHours(1), "UTC", AudienceScope.AllMembers, Actor, location, url));
    }

    [Fact]
    public void ScheduleRequiresPresenterAndCancellationReason()
    {
        var schedule = ScheduleAt(SharingScheduleStatus.Draft);
        schedule.Presenters.Clear();
        Assert.Throws<InvalidOperationException>(() => schedule.Publish(Actor));
        Assert.Throws<ArgumentException>(() => schedule.Cancel(" ", Actor));
        Assert.Equal(SharingScheduleStatus.Draft, schedule.Status);
        Assert.Equal(0, schedule.Version);
    }

    [Fact]
    public void ResourcesEnforceExclusiveMetadataAndRetainSoftDeletedFile()
    {
        var link = new SharingResource(Guid.NewGuid(), "Link", ResourceType.Link, 0, Actor);
        Assert.Throws<ArgumentException>(() => link.SetFile("x.pdf", "key", 1, "application/pdf"));
        Assert.Throws<ArgumentException>(() => link.Update("Link", null, "javascript:alert(1)", 0));
        var file = new SharingResource(Guid.NewGuid(), "File", ResourceType.File, 0, Actor);
        Assert.Throws<ArgumentException>(() => file.Update("File", null, "https://example.com", 0));
        Assert.Throws<ArgumentException>(() => file.SetFile("x.pdf", "key", 0, "application/pdf"));
        file.SetFile("x.pdf", "key", 1, "application/pdf");
        file.Deactivate();
        Assert.False(file.IsActive);
        Assert.Equal("key", file.StorageKey);
        Assert.Throws<ArgumentOutOfRangeException>(() => file.Reorder(-1));
    }

    private static SharingContent ContentAt(SharingContentStatus state)
    {
        var content = new SharingContent("Title", "title", "Summary", "Body", Actor);
        if (state == SharingContentStatus.Draft) return content;
        content.Submit(Actor);
        if (state == SharingContentStatus.Rejected) content.Reject(Actor, "Revise");
        if (state is SharingContentStatus.Published or SharingContentStatus.Archived) content.Approve(Actor);
        if (state == SharingContentStatus.Archived) content.Archive(Actor);
        return content;
    }

    private static SharingSchedule ScheduleAt(SharingScheduleStatus state)
    {
        var start = new DateTimeOffset(2026, 10, 1, 7, 0, 0, TimeSpan.Zero);
        var schedule = new SharingSchedule("Talk", SharingType.TechTalk, DeliveryMode.Online,
            start, start.AddHours(1), "UTC", AudienceScope.AllMembers, Actor, meetingUrl: "https://meet.example.com");
        schedule.Presenters.Add(new(schedule.Id, Actor, PresenterRole.Speaker, 0));
        if (state == SharingScheduleStatus.Cancelled) schedule.Cancel("Unavailable", Actor);
        if (state is SharingScheduleStatus.Scheduled or SharingScheduleStatus.Ongoing or SharingScheduleStatus.Completed) schedule.Publish(Actor);
        if (state is SharingScheduleStatus.Ongoing or SharingScheduleStatus.Completed) schedule.Start(Actor);
        if (state == SharingScheduleStatus.Completed) schedule.Complete(Actor);
        return schedule;
    }
}

using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Sharing;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Sharing;

namespace GdscSharingPlatform.UnitTests.Features.Sharing;

public sealed class SharingRulesTests
{
    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,test")]
    [InlineData("file:///etc/passwd")]
    [InlineData("https://user:password@example.com")]
    public void UnsafeUrlsRejected(string url) => Assert.False(SharingRules.HttpUrl(url));

    [Theory]
    [InlineData(2026, 3, 8, 2, 30)]
    [InlineData(2026, 11, 1, 1, 30)]
    public void MissingOrAmbiguousDstTimeRejected(int y, int m, int d, int h, int min) =>
        Assert.Throws<ApplicationValidationException>(() => SharingRules.ToUtc(new(y, m, d, h, min, 0), "America/New_York"));

    [Fact]
    public void LocalTimeConvertsAndRejectsNonIanaOrOffsetInput()
    {
        Assert.Equal(new DateTimeOffset(2026, 10, 1, 7, 0, 0, TimeSpan.Zero),
            SharingRules.ToUtc(new(2026, 10, 1, 14, 0, 0), "Asia/Ho_Chi_Minh"));
        Assert.Throws<ApplicationValidationException>(() => SharingRules.ToUtc(new(2026, 10, 1, 14, 0, 0), "Unknown/Zone"));
        Assert.Throws<ApplicationValidationException>(() => SharingRules.ToUtc(DateTime.UtcNow, "UTC"));
    }

    [Fact]
    public void AmbiguousEndTimeReportsTheEndField()
    {
        var error = Assert.Throws<ApplicationValidationException>(() =>
            SharingRules.ToUtc(new(2026, 11, 1, 1, 30, 0), "America/New_York", "endsAtLocal"));
        Assert.Contains("endsAtLocal", error.Errors.Keys);
        Assert.DoesNotContain("startsAtLocal", error.Errors.Keys);
    }

    [Fact]
    public void ContributorCannotReadOrEditDraft_AndCannotEditPublished()
    {
        var owner = Guid.NewGuid(); var contributor = Guid.NewGuid();
        var content = new SharingContent("Title", "title", "Summary", "Body", owner);
        content.Authors.Add(new(content.Id, contributor, SharingAuthorRole.Contributor, 1));
        Assert.False(SharingRules.CanRead(content, contributor, false));
        Assert.Throws<ForbiddenAccessException>(() => SharingRules.RequireEdit(content, contributor, false));
        content.Submit(owner); content.Approve(Guid.NewGuid());
        Assert.True(SharingRules.CanRead(content, contributor, false));
        Assert.Throws<ForbiddenAccessException>(() => SharingRules.RequireEdit(content, contributor, false));
        SharingRules.RequireEdit(content, owner, false);
        content.Update("Edited", "edited", "Summary", "New body", owner);
        Assert.Equal(SharingContentStatus.Published, content.Status);
    }
    [Fact]
    public void AudienceRequiresTargetsAndDisallowsTargetsForAllMembers()
    {
        var validator = new AudienceRequestValidator();
        Assert.False(validator.Validate(new AudienceRequest(AudienceScope.SelectedAudience, [], [])).IsValid);
        Assert.False(validator.Validate(new AudienceRequest(AudienceScope.AllMembers, [Guid.NewGuid()], [])).IsValid);
        Assert.True(validator.Validate(new AudienceRequest(AudienceScope.SelectedAudience, [], [Guid.NewGuid()])).IsValid);
    }
    [Fact]
    public void NullAndDuplicateRequestCollectionsReturnValidationErrors()
    {
        var validator = new ContentRequestValidator();
        Assert.False(validator.Validate(new ContentRequest("Title", "slug", "Summary", "Body", null!, [])).IsValid);
        var id = Guid.NewGuid();
        Assert.False(validator.Validate(new ContentRequest("Title", "slug", "Summary", "Body", [id, id], [])).IsValid);
        Assert.False(new PresentersRequestValidator().Validate(new PresentersRequest([null!])).IsValid);
        Assert.False(new PresentersRequestValidator().Validate(new PresentersRequest([new(id, PresenterRole.Host), new(id, PresenterRole.Host)])).IsValid);
    }
}

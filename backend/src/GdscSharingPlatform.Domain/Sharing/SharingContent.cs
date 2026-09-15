using GdscSharingPlatform.Domain.Common;
using GdscSharingPlatform.Domain.Enums;

namespace GdscSharingPlatform.Domain.Sharing;

public sealed class SharingContent : BaseEntity
{
    private SharingContent() { }

    public SharingContent(string title, string slug, string summary, string bodyMarkdown, Guid createdByUserId)
    {
        Title = Required(title, 200, nameof(title));
        Slug = Required(slug, 200, nameof(slug));
        Summary = Required(summary, 1000, nameof(summary));
        BodyMarkdown = Required(bodyMarkdown, 100_000, nameof(bodyMarkdown));
        CreatedByUserId = RequiredId(createdByUserId, nameof(createdByUserId));
        Authors.Add(new SharingContentAuthor(Id, createdByUserId, SharingAuthorRole.Owner, 0));
    }

    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string BodyMarkdown { get; private set; } = string.Empty;
    public string? CoverImageUrl { get; private set; }
    public SharingContentStatus Status { get; private set; } = SharingContentStatus.Draft;
    public string? ReviewNote { get; private set; }
    public DateTimeOffset? SubmittedAtUtc { get; private set; }
    public DateTimeOffset? ReviewedAtUtc { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTimeOffset? PublishedAtUtc { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public long Version { get; private set; }
    public ICollection<SharingContentAuthor> Authors { get; private set; } = new List<SharingContentAuthor>();
    public ICollection<SharingResource> Resources { get; private set; } = new List<SharingResource>();
    public ICollection<SharingContentTag> Tags { get; private set; } = new List<SharingContentTag>();
    public ICollection<SharingScheduleContent> Schedules { get; private set; } = new List<SharingScheduleContent>();

    public void Update(string title, string slug, string summary, string bodyMarkdown, Guid actorUserId)
    {
        if (Status == SharingContentStatus.PendingReview || Status == SharingContentStatus.Archived)
            throw new InvalidOperationException("Content cannot be edited in its current status.");
        Title = Required(title, 200, nameof(title));
        Slug = Required(slug, 200, nameof(slug));
        Summary = Required(summary, 1000, nameof(summary));
        BodyMarkdown = Required(bodyMarkdown, 100_000, nameof(bodyMarkdown));
        Touch(actorUserId);
    }

    public void Submit(Guid actorUserId)
    {
        RequireStatus(SharingContentStatus.Draft, SharingContentStatus.Rejected);
        Status = SharingContentStatus.PendingReview;
        ReviewNote = null;
        SubmittedAtUtc = DateTimeOffset.UtcNow;
        Touch(actorUserId);
    }

    public void Withdraw(Guid actorUserId)
    {
        RequireStatus(SharingContentStatus.PendingReview);
        Status = SharingContentStatus.Draft;
        Touch(actorUserId);
    }

    public void Approve(Guid reviewerUserId)
    {
        RequireStatus(SharingContentStatus.PendingReview);
        if (!Authors.Any(x => x.AuthorRole == SharingAuthorRole.Owner) || string.IsNullOrWhiteSpace(BodyMarkdown))
            throw new InvalidOperationException("Content must have an owner and body before approval.");
        Status = SharingContentStatus.Published;
        ReviewNote = null;
        ReviewedByUserId = RequiredId(reviewerUserId, nameof(reviewerUserId));
        ReviewedAtUtc = DateTimeOffset.UtcNow;
        PublishedAtUtc = DateTimeOffset.UtcNow;
        Touch(reviewerUserId);
    }

    public void Reject(Guid reviewerUserId, string reviewNote)
    {
        RequireStatus(SharingContentStatus.PendingReview);
        ReviewNote = Required(reviewNote, 4000, nameof(reviewNote));
        Status = SharingContentStatus.Rejected;
        ReviewedByUserId = RequiredId(reviewerUserId, nameof(reviewerUserId));
        ReviewedAtUtc = DateTimeOffset.UtcNow;
        Touch(reviewerUserId);
    }

    public void Archive(Guid actorUserId)
    {
        if (Status is SharingContentStatus.PendingReview or SharingContentStatus.Archived)
            throw new InvalidOperationException("Content cannot be archived in its current status.");
        Status = SharingContentStatus.Archived;
        Touch(actorUserId);
    }

    private void RequireStatus(params SharingContentStatus[] allowed)
    {
        if (!allowed.Contains(Status)) throw new InvalidOperationException("Invalid content status transition.");
    }

    public void ReturnToDraft(Guid actorUserId)
    {
        RequireStatus(SharingContentStatus.Published, SharingContentStatus.Archived);
        Status = SharingContentStatus.Draft;
        Touch(actorUserId);
    }

    public void SetCoverImage(string? url) => CoverImageUrl = url;

    // Child mutations participate in the aggregate's concurrency check.
    public void Touch(Guid userId) { RequiredId(userId, nameof(userId)); Version++; UpdatedAtUtc = DateTimeOffset.UtcNow; }
    private static Guid RequiredId(Guid value, string name) => value == Guid.Empty ? throw new ArgumentException("A valid id is required.", name) : value;
    private static string Required(string value, int max, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("A value is required.", name) : value.Trim().Length > max ? throw new ArgumentOutOfRangeException(name) : value.Trim();
}

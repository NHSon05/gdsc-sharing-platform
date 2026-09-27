using GdscSharingPlatform.Domain.Common;
using GdscSharingPlatform.Domain.Enums;

namespace GdscSharingPlatform.Domain.Sharing;

public sealed class ContentLike : BaseEntity
{
    private ContentLike() { }

    public ContentLike(Guid sharingContentId, Guid userId)
    {
        SharingContentId = RequiredId(sharingContentId, nameof(sharingContentId));
        UserId = RequiredId(userId, nameof(userId));
    }

    public Guid SharingContentId { get; private set; }
    public Guid UserId { get; private set; }
    private static Guid RequiredId(Guid value, string name) => value == Guid.Empty
        ? throw new ArgumentException("A valid id is required.", name) : value;
}

public sealed class SavedContent : BaseEntity
{
    private SavedContent() { }

    public SavedContent(Guid sharingContentId, Guid userId)
    {
        SharingContentId = RequiredId(sharingContentId, nameof(sharingContentId));
        UserId = RequiredId(userId, nameof(userId));
    }

    public Guid SharingContentId { get; private set; }
    public Guid UserId { get; private set; }
    private static Guid RequiredId(Guid value, string name) => value == Guid.Empty
        ? throw new ArgumentException("A valid id is required.", name) : value;
}

public sealed class ContentComment : BaseEntity
{
    private ContentComment() { }

    public ContentComment(Guid sharingContentId, Guid authorUserId, string bodyMarkdown, Guid? parentCommentId = null)
    {
        SharingContentId = RequiredId(sharingContentId, nameof(sharingContentId));
        AuthorUserId = RequiredId(authorUserId, nameof(authorUserId));
        ParentCommentId = parentCommentId is { } id ? RequiredId(id, nameof(parentCommentId)) : null;
        BodyMarkdown = RequiredBody(bodyMarkdown);
    }

    public Guid SharingContentId { get; private set; }
    public Guid AuthorUserId { get; private set; }
    public Guid? ParentCommentId { get; private set; }
    public string BodyMarkdown { get; private set; } = string.Empty;
    public ContentCommentStatus Status { get; private set; } = ContentCommentStatus.Active;
    public string? ModerationReason { get; private set; }
    public Guid? ModeratedByUserId { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public long Version { get; private set; }

    public void Update(string bodyMarkdown, Guid actorUserId)
    {
        RequireActive();
        BodyMarkdown = RequiredBody(bodyMarkdown);
        Touch(actorUserId);
    }

    public void Delete(Guid actorUserId)
    {
        RequireActive();
        Status = ContentCommentStatus.Deleted;
        DeletedAtUtc = DateTimeOffset.UtcNow;
        Touch(actorUserId);
    }

    public void Hide(string reason, Guid moderatorUserId)
    {
        if (Status != ContentCommentStatus.Active)
            throw new InvalidOperationException("Only active comments can be hidden.");
        ModerationReason = RequiredReason(reason);
        ModeratedByUserId = RequiredId(moderatorUserId, nameof(moderatorUserId));
        Status = ContentCommentStatus.Hidden;
        Touch(moderatorUserId);
    }

    public void Restore(Guid moderatorUserId)
    {
        if (Status != ContentCommentStatus.Hidden)
            throw new InvalidOperationException("Only hidden comments can be restored.");
        Status = ContentCommentStatus.Active;
        ModerationReason = null;
        ModeratedByUserId = RequiredId(moderatorUserId, nameof(moderatorUserId));
        Touch(moderatorUserId);
    }

    private void RequireActive()
    {
        if (Status != ContentCommentStatus.Active)
            throw new InvalidOperationException("Comment is not active.");
    }

    private void Touch(Guid actorUserId)
    {
        RequiredId(actorUserId, nameof(actorUserId));
        Version++;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string RequiredBody(string value)
    {
        var body = value?.Trim() ?? string.Empty;
        return body.Length is >= 1 and <= 2_000 ? body
            : throw new ArgumentOutOfRangeException(nameof(value), "Comment must contain 1 to 2,000 characters.");
    }

    private static string RequiredReason(string value)
    {
        var reason = value?.Trim() ?? string.Empty;
        return reason.Length is >= 1 and <= 2_000 ? reason
            : throw new ArgumentOutOfRangeException(nameof(value), "Moderation reason must contain 1 to 2,000 characters.");
    }

    private static Guid RequiredId(Guid value, string name) => value == Guid.Empty
        ? throw new ArgumentException("A valid id is required.", name) : value;
}

public sealed class ScheduleRsvp : BaseEntity
{
    private ScheduleRsvp() { }

    public ScheduleRsvp(Guid sharingScheduleId, Guid userId, ScheduleRsvpStatus status)
    {
        SharingScheduleId = RequiredId(sharingScheduleId, nameof(sharingScheduleId));
        UserId = RequiredId(userId, nameof(userId));
        Status = ValidStatus(status);
        RespondedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid SharingScheduleId { get; private set; }
    public Guid UserId { get; private set; }
    public ScheduleRsvpStatus Status { get; private set; }
    public DateTimeOffset RespondedAtUtc { get; private set; }
    public long Version { get; private set; }

    public void Update(ScheduleRsvpStatus status)
    {
        status = ValidStatus(status);
        if (Status == status) return;
        Status = status;
        Version++;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static Guid RequiredId(Guid value, string name) => value == Guid.Empty
        ? throw new ArgumentException("A valid id is required.", name) : value;
    private static ScheduleRsvpStatus ValidStatus(ScheduleRsvpStatus value) => Enum.IsDefined(value)
        ? value : throw new ArgumentOutOfRangeException(nameof(value));
}

public sealed class Notification : BaseEntity
{
    private Notification() { }

    public Notification(Guid recipientUserId, Guid? actorUserId, NotificationType type, string entityType,
        Guid entityId, string title, string message, Guid eventId, string? route = null)
    {
        RecipientUserId = RequiredId(recipientUserId, nameof(recipientUserId));
        ActorUserId = actorUserId is { } actor ? RequiredId(actor, nameof(actorUserId)) : null;
        Type = Enum.IsDefined(type) ? type : throw new ArgumentOutOfRangeException(nameof(type));
        EntityType = Required(entityType, 80, nameof(entityType));
        EntityId = RequiredId(entityId, nameof(entityId));
        Title = Required(title, 200, nameof(title));
        Message = Required(message, 1_000, nameof(message));
        EventId = RequiredId(eventId, nameof(eventId));
        Route = route?.Trim();
    }

    public Guid RecipientUserId { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? Route { get; private set; }
    public Guid EventId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTimeOffset? ReadAtUtc { get; private set; }

    public void MarkRead()
    {
        if (IsRead) return;
        IsRead = true;
        ReadAtUtc = DateTimeOffset.UtcNow;
    }

    private static string Required(string value, int maxLength, string name)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        return trimmed.Length > 0 && trimmed.Length <= maxLength ? trimmed
            : throw new ArgumentOutOfRangeException(name);
    }

    private static Guid RequiredId(Guid value, string name) => value == Guid.Empty
        ? throw new ArgumentException("A valid id is required.", name) : value;
}

public sealed class OutboxMessage
{
    private OutboxMessage() { }

    public OutboxMessage(Guid id, string type, string payloadJson, DateTimeOffset occurredAtUtc)
    {
        Id = RequiredId(id, nameof(id));
        Type = Required(type, 120, nameof(type));
        PayloadJson = Required(payloadJson, 64_000, nameof(payloadJson));
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string PayloadJson { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public DateTimeOffset? ProcessedAtUtc { get; private set; }
    public int RetryCount { get; private set; }
    public DateTimeOffset? NextAttemptAtUtc { get; private set; }
    public string? LastError { get; private set; }
    public DateTimeOffset? DeadLetteredAtUtc { get; private set; }

    public void DeadLetter(DateTimeOffset now) { DeadLetteredAtUtc = now; NextAttemptAtUtc = null; }

    public void MarkProcessed() { ProcessedAtUtc = DateTimeOffset.UtcNow; NextAttemptAtUtc = null; LastError = null; }
    public void RecordFailure(DateTimeOffset nextAttemptAtUtc, string error)
    {
        RetryCount++;
        NextAttemptAtUtc = nextAttemptAtUtc;
        LastError = Required(error, 2_000, nameof(error));
    }

    private static string Required(string value, int maxLength, string name)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        return trimmed.Length is >= 1 && trimmed.Length <= maxLength ? trimmed
            : throw new ArgumentOutOfRangeException(name);
    }
    private static Guid RequiredId(Guid value, string name) => value == Guid.Empty
        ? throw new ArgumentException("A valid id is required.", name) : value;
}

using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Application.Common.Interfaces;

namespace GdscSharingPlatform.Application.Features.Sharing;

public sealed record ContentRequest(string Title, string Slug, string Summary, string BodyMarkdown,
    IReadOnlyList<Guid> TagIds, IReadOnlyList<Guid> ContributorUserIds, string? CoverImageUrl = null);
public sealed record ReviewRequest(string ReviewNote);
public sealed record CancelScheduleRequest(string Reason);
public sealed record TagRequest(string Name, string Slug, string? Color = null);
public sealed record TagStatusRequest(bool? IsActive);
public sealed record ResourceRequest(string Title, string? Description = null, string? ExternalUrl = null, int SortOrder = 0);
public sealed record ReorderResourcesRequest(IReadOnlyList<Guid> Ids);
public sealed record PresenterRequest(Guid UserId, PresenterRole Role, int SortOrder = 0);
public sealed record PresentersRequest(IReadOnlyList<PresenterRequest> Presenters);
public sealed record ScheduleContentsRequest(IReadOnlyList<Guid> ContentIds);
public sealed record AudienceRequest(AudienceScope AudienceScope, IReadOnlyList<Guid> GenerationIds, IReadOnlyList<Guid> DepartmentIds);
public sealed record ScheduleRequest(string Title, SharingType SharingType, DeliveryMode DeliveryMode,
    DateTime StartsAtLocal, DateTime EndsAtLocal, string TimeZoneId, AudienceScope AudienceScope,
    IReadOnlyList<PresenterRequest> Presenters, IReadOnlyList<Guid> ContentIds,
    IReadOnlyList<Guid> GenerationIds, IReadOnlyList<Guid> DepartmentIds,
    string? Description = null, string? Location = null, string? MeetingUrl = null);

public sealed class ContentQuery
{
    public string? Search { get; set; }
    public Guid? TagId { get; set; }
    public Guid? AuthorId { get; set; }
    public SharingContentStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string Sort { get; set; } = "newest";
}
public sealed class ScheduleQuery
{
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public SharingScheduleStatus? Status { get; set; }
    public DeliveryMode? DeliveryMode { get; set; }
    public SharingType? SharingType { get; set; }
    public Guid? PresenterId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
public sealed record SharingPage<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
public sealed record TagResponse(Guid Id, string Name, string Slug, string? Color, bool IsActive);
public sealed record AuthorResponse(Guid UserId, string FullName, SharingAuthorRole Role, int SortOrder);
public sealed record PresenterResponse(Guid UserId, string FullName, PresenterRole Role, int SortOrder);
public sealed record ContentSummary(Guid Id, string Title, string Slug, string Summary, string? CoverImageUrl,
    SharingContentStatus Status, DateTimeOffset? PublishedAtUtc, long Version)
{
    public IReadOnlyList<AuthorResponse> Authors { get; init; } = [];
    public IReadOnlyList<TagResponse> Tags { get; init; } = [];
    public int LikeCount { get; init; }
    public int CommentCount { get; init; }
    public bool IsLikedByCurrentUser { get; init; }
    public bool IsSavedByCurrentUser { get; init; }
}
public sealed record ContentReference(Guid Id, string Title, string Slug, int SortOrder);
public sealed record ScheduleReference(Guid Id, string Title, DateTimeOffset StartsAtUtc, SharingScheduleStatus Status);
public sealed record ContentResponse(ContentSummary Content, string BodyMarkdown, string? ReviewNote,
    DateTimeOffset? SubmittedAtUtc, DateTimeOffset? ReviewedAtUtc, Guid? ReviewedByUserId,
    IReadOnlyList<AuthorResponse> Authors, IReadOnlyList<TagResponse> Tags,
    IReadOnlyList<ResourceResponse> Resources, IReadOnlyList<ScheduleReference> Schedules);
public sealed record ResourceResponse(Guid Id, Guid SharingContentId, string Title, string? Description,
    ResourceType ResourceType, string? ExternalUrl, string? OriginalFileName, long? FileSize,
    string? ContentType, int SortOrder, bool IsActive);
public sealed record ResourceMutationResponse(ResourceResponse Resource, long Version);
public sealed record SharingDownload(Stream Content, string FileName, string ContentType);
public sealed record ScheduleResponse(Guid Id, string Title, string? Description, SharingType SharingType,
    DeliveryMode DeliveryMode, DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc, string TimeZoneId,
    string? Location, string? MeetingUrl, SharingScheduleStatus Status, AudienceScope AudienceScope,
    string? CancellationReason, Guid CreatedByUserId, long Version, IReadOnlyList<PresenterResponse> Presenters,
    IReadOnlyList<ContentReference> Contents, IReadOnlyList<Guid> GenerationIds, IReadOnlyList<Guid> DepartmentIds);

public sealed record InteractionResponse(Guid ContentId, bool IsLikedByCurrentUser, int LikeCount);
public sealed record SavedContentResponse(Guid ContentId, bool IsSavedByCurrentUser);
public sealed record SavedContentItem(ContentSummary Content, DateTimeOffset SavedAtUtc);
public sealed record CreateCommentRequest(string BodyMarkdown);
public sealed record UpdateCommentRequest(string BodyMarkdown, [property: System.Text.Json.Serialization.JsonRequired] long Version);
public sealed record HideCommentRequest(string Reason);
public sealed record CommentResponse(Guid Id, Guid SharingContentId, Guid AuthorUserId, Guid? ParentCommentId,
    string? BodyMarkdown, ContentCommentStatus Status, string? ModerationReason, Guid? ModeratedByUserId,
    DateTimeOffset CreatedAtUtc, DateTimeOffset? UpdatedAtUtc, DateTimeOffset? DeletedAtUtc, long Version);
public sealed class CommentQuery
{
    public string? Cursor { get; set; }
    public int PageSize { get; set; } = 20;
    public string Sort { get; set; } = "oldest";
}
public sealed record RsvpRequest([property: System.Text.Json.Serialization.JsonRequired] ScheduleRsvpStatus Status, long? Version = null);
public sealed record RsvpResponse(Guid ScheduleId, ScheduleRsvpStatus Status, DateTimeOffset UpdatedAtUtc, long Version);
public sealed record RsvpSummary(Guid ScheduleId, int Going, int Maybe, int NotGoing, int NoResponse);
public sealed record ScheduleRsvpResponse(Guid UserId, ScheduleRsvpStatus Status, DateTimeOffset RespondedAtUtc,
    DateTimeOffset? UpdatedAtUtc, long Version);
public sealed class NotificationQuery
{
    public string? Cursor { get; set; }
    public int PageSize { get; set; } = 20;
    public bool? IsRead { get; set; }
    public NotificationType? Type { get; set; }
}
public sealed record NotificationResponse(Guid Id, NotificationType Type, Guid? ActorUserId, string EntityType,
    Guid EntityId, string Title, string Message, string? Route, bool IsRead, DateTimeOffset CreatedAtUtc);
public sealed record NotificationPage(IReadOnlyList<NotificationResponse> Items, string? NextCursor);
public sealed record UnreadNotificationCount(int Count);

public static class SocialEventNames
{
    public const string NotificationCreated = "notification.created";
    public const string ContentInteractionUpdated = "content.interaction.updated";
    public const string CommentCreated = "comment.created";
    public const string CommentUpdated = "comment.updated";
    public const string CommentDeleted = "comment.deleted";
    public const string CommentHidden = "comment.hidden";
    public const string ScheduleRsvpUpdated = "schedule.rsvp.updated";
}

public enum ContentAction { Submit, Withdraw, Approve, Reject, ReturnToDraft, Archive }
public enum ScheduleAction { Publish, Start, Complete, Cancel }

public interface ISharingContentService
{
    Task<SharingPage<ContentSummary>> ListAsync(ContentQuery query, bool mine, bool admin, CancellationToken ct);
    Task<ContentResponse> GetAsync(Guid id, bool mine, bool admin, CancellationToken ct);
    Task<ContentResponse> GetBySlugAsync(string slug, CancellationToken ct);
    Task<ContentResponse> CreateAsync(ContentRequest request, CancellationToken ct);
    Task<ContentResponse> UpdateAsync(Guid id, ContentRequest request, long version, CancellationToken ct);
    Task<ContentResponse> TransitionAsync(Guid id, ContentAction action, long version, string? note, CancellationToken ct);
}
public interface ISharingTagService
{
    Task<IReadOnlyList<TagResponse>> ListAsync(bool admin, CancellationToken ct);
    Task<TagResponse> SaveAsync(Guid? id, TagRequest request, CancellationToken ct);
    Task<TagResponse> SetStatusAsync(Guid id, TagStatusRequest request, CancellationToken ct);
}
public interface ISharingResourceService
{
    Task<IReadOnlyList<ResourceResponse>> ListAsync(Guid contentId, CancellationToken ct);
    Task<ResourceMutationResponse> CreateAsync(Guid contentId, ResourceRequest request, FileUpload? file, long version, CancellationToken ct);
    Task<ResourceMutationResponse> UpdateAsync(Guid id, ResourceRequest request, long version, CancellationToken ct);
    Task<ResourceMutationResponse> ReplaceAsync(Guid id, FileUpload file, long version, CancellationToken ct);
    Task<long> DeleteAsync(Guid id, long version, CancellationToken ct);
    Task<long> ReorderAsync(Guid contentId, ReorderResourcesRequest request, long version, CancellationToken ct);
    Task<SharingDownload> DownloadAsync(Guid id, CancellationToken ct);
}
public interface ISharingScheduleService
{
    Task<SharingPage<ScheduleResponse>> ListAsync(ScheduleQuery query, bool mine, bool admin, CancellationToken ct);
    Task<ScheduleResponse> GetAsync(Guid id, bool admin, CancellationToken ct);
    Task<ScheduleResponse> CreateAsync(ScheduleRequest request, CancellationToken ct);
    Task<ScheduleResponse> UpdateAsync(Guid id, ScheduleRequest request, long version, CancellationToken ct);
    Task<ScheduleResponse> SetPresentersAsync(Guid id, PresentersRequest request, long version, CancellationToken ct);
    Task<ScheduleResponse> SetContentsAsync(Guid id, ScheduleContentsRequest request, long version, CancellationToken ct);
    Task<ScheduleResponse> SetAudienceAsync(Guid id, AudienceRequest request, long version, CancellationToken ct);
    Task<ScheduleResponse> TransitionAsync(Guid id, ScheduleAction action, long version, string? reason, CancellationToken ct);
    Task DeleteAsync(Guid id, long version, CancellationToken ct);
}

public interface IContentInteractionService
{
    Task<InteractionResponse> LikeAsync(Guid contentId, CancellationToken ct);
    Task<InteractionResponse> UnlikeAsync(Guid contentId, CancellationToken ct);
    Task<SavedContentResponse> SaveAsync(Guid contentId, CancellationToken ct);
    Task<SavedContentResponse> UnsaveAsync(Guid contentId, CancellationToken ct);
    Task<SharingPage<SavedContentItem>> ListSavedAsync(ContentQuery query, CancellationToken ct);
}

public interface IContentCommentService
{
    Task<CursorPage<CommentResponse>> ListAsync(Guid contentId, CommentQuery query, CancellationToken ct);
    Task<CommentResponse> CreateAsync(Guid contentId, CreateCommentRequest request, CancellationToken ct);
    Task<CommentResponse> ReplyAsync(Guid commentId, CreateCommentRequest request, CancellationToken ct);
    Task<CommentResponse> UpdateAsync(Guid commentId, UpdateCommentRequest request, CancellationToken ct);
    Task DeleteAsync(Guid commentId, CancellationToken ct);
    Task<CommentResponse> HideAsync(Guid commentId, HideCommentRequest request, CancellationToken ct);
    Task<CommentResponse> RestoreAsync(Guid commentId, CancellationToken ct);
}

public interface IScheduleRsvpService
{
    Task<RsvpResponse> UpsertAsync(Guid scheduleId, RsvpRequest request, CancellationToken ct);
    Task WithdrawAsync(Guid scheduleId, long version, CancellationToken ct);
    Task<RsvpResponse?> GetMineAsync(Guid scheduleId, CancellationToken ct);
    Task<CursorPage<ScheduleRsvpResponse>> ListForAdminAsync(Guid scheduleId, string? cursor, int pageSize, CancellationToken ct);
    Task<RsvpSummary> GetSummaryAsync(Guid scheduleId, CancellationToken ct);
}

public interface INotificationService
{
    Task<NotificationPage> ListAsync(NotificationQuery query, CancellationToken ct);
    Task<UnreadNotificationCount> GetUnreadCountAsync(CancellationToken ct);
    Task MarkReadAsync(Guid notificationId, CancellationToken ct);
    Task MarkAllReadAsync(CancellationToken ct);
}

public sealed record CursorPage<T>(IReadOnlyList<T> Items, string? NextCursor);

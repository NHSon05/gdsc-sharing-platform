using GdscSharingPlatform.Domain.Common;
using GdscSharingPlatform.Domain.Enums;

namespace GdscSharingPlatform.Domain.Sharing;

public sealed class SharingSchedule : BaseEntity
{
    private SharingSchedule() { }
    public SharingSchedule(string title, SharingType sharingType, DeliveryMode deliveryMode, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, string timeZoneId, AudienceScope audienceScope, Guid createdByUserId, string? location = null, string? meetingUrl = null)
    {
        Title = Required(title, 200, nameof(title)); SharingType = sharingType; DeliveryMode = deliveryMode; ValidateTimes(startsAtUtc, endsAtUtc); StartsAtUtc = startsAtUtc; EndsAtUtc = endsAtUtc; TimeZoneId = Required(timeZoneId, 100, nameof(timeZoneId)); AudienceScope = audienceScope; CreatedByUserId = Required(createdByUserId, nameof(createdByUserId));
        ValidateDelivery(deliveryMode, location, meetingUrl); Location = location?.Trim(); MeetingUrl = meetingUrl?.Trim();
    }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public SharingType SharingType { get; private set; }
    public DeliveryMode DeliveryMode { get; private set; }
    public DateTimeOffset StartsAtUtc { get; private set; }
    public DateTimeOffset EndsAtUtc { get; private set; }
    public string TimeZoneId { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public string? MeetingUrl { get; private set; }
    public SharingScheduleStatus Status { get; private set; } = SharingScheduleStatus.Draft;
    public AudienceScope AudienceScope { get; private set; }
    public string? CancellationReason { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public long Version { get; private set; }
    public ICollection<SharingSchedulePresenter> Presenters { get; private set; } = new List<SharingSchedulePresenter>();
    public ICollection<SharingScheduleContent> Contents { get; private set; } = new List<SharingScheduleContent>();
    public ICollection<SharingScheduleAudienceGeneration> AudienceGenerations { get; private set; } = new List<SharingScheduleAudienceGeneration>();
    public ICollection<SharingScheduleAudienceDepartment> AudienceDepartments { get; private set; } = new List<SharingScheduleAudienceDepartment>();

    public void Update(string title, SharingType sharingType, DeliveryMode deliveryMode, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, string timeZoneId, AudienceScope audienceScope, Guid actorUserId, string? location = null, string? meetingUrl = null)
    {
        if (Status is SharingScheduleStatus.Completed or SharingScheduleStatus.Cancelled) throw new InvalidOperationException("Schedule cannot be edited in its current status.");
        Title = Required(title, 200, nameof(title)); ValidateDelivery(deliveryMode, location, meetingUrl); ValidateTimes(startsAtUtc, endsAtUtc); SharingType = sharingType; DeliveryMode = deliveryMode; StartsAtUtc = startsAtUtc; EndsAtUtc = endsAtUtc; TimeZoneId = Required(timeZoneId, 100, nameof(timeZoneId)); AudienceScope = audienceScope; Location = location?.Trim(); MeetingUrl = meetingUrl?.Trim(); Touch(actorUserId);
    }
    public void Publish(Guid actorUserId) { RequireStatus(SharingScheduleStatus.Draft); if (!Presenters.Any()) throw new InvalidOperationException("A scheduled sharing needs a presenter."); Status = SharingScheduleStatus.Scheduled; Touch(actorUserId); }
    public void Start(Guid actorUserId) { RequireStatus(SharingScheduleStatus.Scheduled); Status = SharingScheduleStatus.Ongoing; Touch(actorUserId); }
    public void Complete(Guid actorUserId) { RequireStatus(SharingScheduleStatus.Ongoing); Status = SharingScheduleStatus.Completed; Touch(actorUserId); }
    public void Cancel(string reason, Guid actorUserId) { if (Status is not (SharingScheduleStatus.Draft or SharingScheduleStatus.Scheduled)) throw new InvalidOperationException("Schedule cannot be cancelled in its current status."); CancellationReason = Required(reason, 4000, nameof(reason)); Status = SharingScheduleStatus.Cancelled; Touch(actorUserId); }
    private void RequireStatus(SharingScheduleStatus status) { if (Status != status) throw new InvalidOperationException("Invalid schedule status transition."); }
    public void SetDescription(string? description) => Description = description;
    public void SetAudienceScope(AudienceScope scope) => AudienceScope = scope;
    public void Touch(Guid actorUserId) { Required(actorUserId, nameof(actorUserId)); Version++; UpdatedAtUtc = DateTimeOffset.UtcNow; }
    private static void ValidateDelivery(DeliveryMode mode, string? location, string? meetingUrl) { if (mode is DeliveryMode.Offline or DeliveryMode.Hybrid && string.IsNullOrWhiteSpace(location)) throw new ArgumentException("Location is required for this delivery mode.", nameof(location)); if (mode is DeliveryMode.Online or DeliveryMode.Hybrid && string.IsNullOrWhiteSpace(meetingUrl)) throw new ArgumentException("MeetingUrl is required for this delivery mode.", nameof(meetingUrl)); }
    private static void ValidateTimes(DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc) { if (endsAtUtc <= startsAtUtc) throw new ArgumentException("End time must be after start time.", nameof(endsAtUtc)); }
    private static Guid Required(Guid id, string name) => id == Guid.Empty ? throw new ArgumentException("A valid id is required.", name) : id;
    private static string Required(string value, int max, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("A value is required.", name) : value.Trim().Length > max ? throw new ArgumentOutOfRangeException(name) : value.Trim();
}

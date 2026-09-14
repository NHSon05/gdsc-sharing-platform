namespace GdscSharingPlatform.Infrastructure.Persistence;

public sealed class SharingAuditEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;
    public string TraceId { get; set; } = string.Empty;
    public string Metadata { get; set; } = "{}";
}

namespace GdscSharingPlatform.Domain.Sharing;

public sealed class SharingScheduleContent
{
    private SharingScheduleContent() { }
    public SharingScheduleContent(Guid sharingScheduleId, Guid sharingContentId, int sortOrder) { SharingScheduleId = sharingScheduleId; SharingContentId = sharingContentId; SortOrder = sortOrder; }
    public Guid SharingScheduleId { get; private set; }
    public Guid SharingContentId { get; private set; }
    public int SortOrder { get; private set; }
    public void Reorder(int order)
    { if (order < 0) throw new ArgumentOutOfRangeException(nameof(order)); SortOrder = order; }
    public SharingSchedule Schedule { get; set; } = null!;
    public SharingContent Content { get; set; } = null!;
}

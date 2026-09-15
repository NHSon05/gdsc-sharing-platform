using GdscSharingPlatform.Domain.Common;
using GdscSharingPlatform.Domain.Enums;

namespace GdscSharingPlatform.Domain.Sharing;

public sealed class SharingSchedulePresenter : BaseEntity
{
    private SharingSchedulePresenter() { }
    public SharingSchedulePresenter(Guid sharingScheduleId, Guid userId, PresenterRole presenterRole, int sortOrder) { SharingScheduleId = sharingScheduleId; UserId = userId; PresenterRole = presenterRole; SortOrder = sortOrder; }
    public Guid SharingScheduleId { get; private set; }
    public Guid UserId { get; private set; }
    public PresenterRole PresenterRole { get; private set; }
    public int SortOrder { get; private set; }
    public void Reorder(int order)
    { if (order < 0) throw new ArgumentOutOfRangeException(nameof(order)); SortOrder = order; }
    public SharingSchedule Schedule { get; set; } = null!;
}

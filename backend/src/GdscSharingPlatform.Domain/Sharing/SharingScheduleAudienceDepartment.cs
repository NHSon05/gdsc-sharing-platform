namespace GdscSharingPlatform.Domain.Sharing;

public sealed class SharingScheduleAudienceDepartment
{
    private SharingScheduleAudienceDepartment() { }
    public SharingScheduleAudienceDepartment(Guid sharingScheduleId, Guid departmentId) { SharingScheduleId = sharingScheduleId; DepartmentId = departmentId; }
    public Guid SharingScheduleId { get; private set; }
    public Guid DepartmentId { get; private set; }
    public SharingSchedule Schedule { get; set; } = null!;
}

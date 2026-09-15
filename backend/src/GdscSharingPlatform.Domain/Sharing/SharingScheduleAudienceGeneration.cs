namespace GdscSharingPlatform.Domain.Sharing;

public sealed class SharingScheduleAudienceGeneration
{
    private SharingScheduleAudienceGeneration() { }
    public SharingScheduleAudienceGeneration(Guid sharingScheduleId, Guid clubGenerationId) { SharingScheduleId = sharingScheduleId; ClubGenerationId = clubGenerationId; }
    public Guid SharingScheduleId { get; private set; }
    public Guid ClubGenerationId { get; private set; }
    public SharingSchedule Schedule { get; set; } = null!;
}

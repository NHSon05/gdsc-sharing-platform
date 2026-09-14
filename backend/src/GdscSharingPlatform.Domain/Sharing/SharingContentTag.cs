namespace GdscSharingPlatform.Domain.Sharing;

public sealed class SharingContentTag
{
    private SharingContentTag() { }
    public SharingContentTag(Guid sharingContentId, Guid sharingTagId) { SharingContentId = sharingContentId; SharingTagId = sharingTagId; }
    public Guid SharingContentId { get; private set; }
    public Guid SharingTagId { get; private set; }
    public SharingContent Content { get; set; } = null!;
    public SharingTag Tag { get; set; } = null!;
}

using GdscSharingPlatform.Domain.Common;
using GdscSharingPlatform.Domain.Enums;

namespace GdscSharingPlatform.Domain.Sharing;

public sealed class SharingContentAuthor : BaseEntity
{
    private SharingContentAuthor() { }
    public SharingContentAuthor(Guid sharingContentId, Guid userId, SharingAuthorRole authorRole, int sortOrder)
    {
        SharingContentId = Required(sharingContentId, nameof(sharingContentId)); UserId = Required(userId, nameof(userId));
        AuthorRole = authorRole; SortOrder = sortOrder < 0 ? throw new ArgumentOutOfRangeException(nameof(sortOrder)) : sortOrder;
    }
    public Guid SharingContentId { get; private set; }
    public Guid UserId { get; private set; }
    public SharingAuthorRole AuthorRole { get; private set; }
    public int SortOrder { get; private set; }
    private static Guid Required(Guid id, string name) => id == Guid.Empty ? throw new ArgumentException("A valid id is required.", name) : id;
}

using GdscSharingPlatform.Domain.Common;

namespace GdscSharingPlatform.Domain.Sharing;

public sealed class SharingTag : BaseEntity
{
    private SharingTag() { }
    public SharingTag(string name, string slug)
    {
        Name = Required(name, 100, nameof(name)); Slug = Required(slug, 100, nameof(slug));
    }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Color { get; private set; }
    public bool IsActive { get; private set; } = true;
    public void Update(string name, string slug, string? color)
    {
        Name = Required(name, 100, nameof(name));
        Slug = Required(slug, 100, nameof(slug));
        Color = color;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
    public void SetActive(bool active) { IsActive = active; UpdatedAtUtc = DateTimeOffset.UtcNow; }
    public ICollection<SharingContentTag> Contents { get; private set; } = new List<SharingContentTag>();
    private static string Required(string value, int max, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("A value is required.", name) : value.Trim().Length > max ? throw new ArgumentOutOfRangeException(name) : value.Trim();
}

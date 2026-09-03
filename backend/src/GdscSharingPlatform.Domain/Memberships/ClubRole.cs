using GdscSharingPlatform.Domain.Common;

namespace GdscSharingPlatform.Domain.Memberships;

public sealed class ClubRole : BaseEntity
{
    private ClubRole()
    {
    }

    public ClubRole(
        string code,
        string name,
        int level,
        bool isActive = true)
    {
        SetInformation(code, name, level);
        IsActive = isActive;
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public int Level { get; private set; }

    public bool IsActive { get; private set; } = true;

    public ICollection<RoleAssignment> RoleAssignments { get; private set; } = new List<RoleAssignment>();

    public void Update(string code, string name, int level)
    {
        SetInformation(code, name, level);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private void SetInformation(string code, string name, int level)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Level = level;
    }
}

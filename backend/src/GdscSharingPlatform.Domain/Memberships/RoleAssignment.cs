using GdscSharingPlatform.Domain.Common;

namespace GdscSharingPlatform.Domain.Memberships;

public sealed class RoleAssignment : BaseEntity
{
    private RoleAssignment()
    {
    }

    public RoleAssignment(
        Guid departmentMembershipId,
        Guid clubRoleId,
        Guid? assignedByUserId = null,
        DateTimeOffset? assignedAtUtc = null,
        bool isActive = true)
    {
        if (departmentMembershipId == Guid.Empty)
        {
            throw new ArgumentException("DepartmentMembershipId cannot be empty.", nameof(departmentMembershipId));
        }

        if (clubRoleId == Guid.Empty)
        {
            throw new ArgumentException("ClubRoleId cannot be empty.", nameof(clubRoleId));
        }

        DepartmentMembershipId = departmentMembershipId;
        ClubRoleId = clubRoleId;
        AssignedByUserId = assignedByUserId;
        AssignedAtUtc = assignedAtUtc ?? DateTimeOffset.UtcNow;
        IsActive = isActive;
    }

    public Guid DepartmentMembershipId { get; private set; }

    public Guid ClubRoleId { get; private set; }

    public DateTimeOffset AssignedAtUtc { get; private set; }

    public Guid? AssignedByUserId { get; private set; }

    public DateTimeOffset? EndedAtUtc { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DepartmentMembership DepartmentMembership { get; set; } = null!;

    public ClubRole ClubRole { get; set; } = null!;

    public void End(DateTimeOffset? endedAtUtc = null)
    {
        IsActive = false;
        EndedAtUtc = endedAtUtc ?? DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Reactivate()
    {
        IsActive = true;
        EndedAtUtc = null;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}

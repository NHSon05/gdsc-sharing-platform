using GdscSharingPlatform.Domain.Common;
using GdscSharingPlatform.Domain.Departments;

namespace GdscSharingPlatform.Domain.Memberships;

public sealed class DepartmentMembership : BaseEntity
{
    private DepartmentMembership()
    {
    }

    public DepartmentMembership(
        Guid clubMembershipId,
        Guid departmentId,
        bool isPrimary = false,
        DateOnly? joinedAt = null,
        DateOnly? leftAt = null,
        bool isActive = true)
    {
        if (clubMembershipId == Guid.Empty)
        {
            throw new ArgumentException("ClubMembershipId cannot be empty.", nameof(clubMembershipId));
        }

        if (departmentId == Guid.Empty)
        {
            throw new ArgumentException("DepartmentId cannot be empty.", nameof(departmentId));
        }

        ValidateDates(joinedAt, leftAt);

        ClubMembershipId = clubMembershipId;
        DepartmentId = departmentId;
        IsPrimary = isPrimary;
        JoinedAt = joinedAt;
        LeftAt = leftAt;
        IsActive = isActive;
    }

    public Guid ClubMembershipId { get; private set; }

    public Guid DepartmentId { get; private set; }

    public bool IsPrimary { get; private set; }

    public DateOnly? JoinedAt { get; private set; }

    public DateOnly? LeftAt { get; private set; }

    public bool IsActive { get; private set; } = true;

    public ClubMembership ClubMembership { get; set; } = null!;

    public Department Department { get; set; } = null!;

    public ICollection<RoleAssignment> RoleAssignments { get; private set; } = new List<RoleAssignment>();

    public void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void End(DateOnly? leftAt = null)
    {
        IsActive = false;
        LeftAt = leftAt ?? DateOnly.FromDateTime(DateTime.UtcNow);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Reactivate(DateOnly? joinedAt = null)
    {
        IsActive = true;
        LeftAt = null;
        if (joinedAt.HasValue)
        {
            JoinedAt = joinedAt;
        }

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateDates(DateOnly? joinedAt, DateOnly? leftAt)
    {
        ValidateDates(joinedAt, leftAt);
        JoinedAt = joinedAt;
        LeftAt = leftAt;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static void ValidateDates(DateOnly? joinedAt, DateOnly? leftAt)
    {
        if (joinedAt.HasValue && leftAt.HasValue && leftAt.Value < joinedAt.Value)
        {
            throw new ArgumentException("Left date must not be before joined date.");
        }
    }
}

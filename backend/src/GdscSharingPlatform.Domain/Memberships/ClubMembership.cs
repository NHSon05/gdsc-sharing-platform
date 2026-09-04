using GdscSharingPlatform.Domain.Common;

namespace GdscSharingPlatform.Domain.Memberships;

public sealed class ClubMembership : BaseEntity
{
    private ClubMembership()
    {
    }

    public ClubMembership(
        Guid userId,
        Guid generationId,
        DateOnly? joinedAt = null,
        DateOnly? leftAt = null,
        bool isActive = true)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        }

        if (generationId == Guid.Empty)
        {
            throw new ArgumentException("GenerationId cannot be empty.", nameof(generationId));
        }

        ValidateDates(joinedAt, leftAt);

        UserId = userId;
        GenerationId = generationId;
        JoinedAt = joinedAt;
        LeftAt = leftAt;
        IsActive = isActive;
    }

    public Guid UserId { get; private set; }

    public Guid GenerationId { get; private set; }

    public DateOnly? JoinedAt { get; private set; }

    public DateOnly? LeftAt { get; private set; }

    public bool IsActive { get; private set; } = true;

    public ClubGeneration Generation { get; set; } = null!;

    public ICollection<DepartmentMembership> DepartmentMemberships { get; private set; } = new List<DepartmentMembership>();

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

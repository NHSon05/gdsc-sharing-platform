using GdscSharingPlatform.Domain.Common;

namespace GdscSharingPlatform.Domain.Memberships;

public sealed class ClubGeneration : BaseEntity
{
    private ClubGeneration()
    {
    }

    public ClubGeneration(
        int number,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        SetInformation(number, startDate, endDate);
    }

    public int Number { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateOnly? StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public bool IsActive { get; private set; } = true;

    public ICollection<ClubMembership> Memberships { get; private set; } = new List<ClubMembership>();

    public void Update(
        int number,
        DateOnly? startDate,
        DateOnly? endDate)
    {
        SetInformation(number, startDate, endDate);
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

    private void SetInformation(
        int number,
        DateOnly? startDate,
        DateOnly? endDate)
    {
        if (number <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(number),
                "Generation number must be greater than zero.");
        }

        if (startDate.HasValue &&
            endDate.HasValue &&
            endDate.Value < startDate.Value)
        {
            throw new ArgumentException(
                "Generation end date must not be before start date.");
        }

        Number = number;
        Name = $"Gen {number}";
        StartDate = startDate;
        EndDate = endDate;
    }
}
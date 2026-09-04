using GdscSharingPlatform.Domain.Memberships;

namespace GdscSharingPlatform.Domain.Departments;

public class Department
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Color { get; set; }

    public string? Icon { get; set; }

    public Guid? LeaderId { get; set; }

    public int SortOrder { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int DisplayOrder
    {
        get => SortOrder;
        set => SortOrder = value;
    }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
        = DateTimeOffset.UtcNow;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTimeOffset CreatedAtUtc
    {
        get => CreatedAt;
        set => CreatedAt = value;
    }

    public DateTimeOffset? UpdatedAt { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTimeOffset? UpdatedAtUtc
    {
        get => UpdatedAt;
        set => UpdatedAt = value;
    }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTimeOffset? DeletedAtUtc
    {
        get => DeletedAt;
        set => DeletedAt = value;
    }

    public ICollection<DepartmentMembership> DepartmentMemberships { get; set; }
        = new List<DepartmentMembership>();
}
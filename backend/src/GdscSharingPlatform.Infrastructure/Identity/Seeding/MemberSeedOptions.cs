namespace GdscSharingPlatform.Infrastructure.Identity.Seeding;

public sealed class MemberSeedOptions
{
    public const string SectionName = "SeedMember";
    public bool Enabled { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = "SOFTWARE";
}

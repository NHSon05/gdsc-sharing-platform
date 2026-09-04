namespace GdscSharingPlatform.Infrastructure.Identity.Seeding;

public sealed class AdminSeedOptions
{
    public const string SectionName = "SeedAdmin";
    public bool Enabled { get; set; }
    public string Email { get; set; } = "admin@gdsc.dev";
    public string Password { get; set; } = "AdminPassword@123!";
    public string FullName { get; set; } = "System Administrator";
    public string? DisplayName { get; set; } = "System Administrator";
    public string DepartmentCode { get; set; } = "MANAGEMENT";
    public string? PhoneNumber { get; set; } = "0901234567";
    public string? StudentCode { get; set; } = "ADMIN001";
    public string? GithubUrl { get; set; } = "https://github.com/gdsc-admin";
    public string? Bio { get; set; } = "System Administrator of GDSC Sharing Platform";
    public string? AvatarUrl { get; set; }
    public int GenerationNumber { get; set; } = 8;
    public string RoleCode { get; set; } = "LEAD";
}
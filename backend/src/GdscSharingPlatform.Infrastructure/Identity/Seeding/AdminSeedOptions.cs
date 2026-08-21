namespace GdscSharingPlatform.Infrastructure.Identity.Seeding;

public sealed class AdminSeedOptions
{
    public const string SectionName = "SeedAdmin";
    public bool Enabled { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = "MANAGEMENT";
}

// Option patterns
// SeedAdmin Section
// -> Configuration Binder
// -> AdminSeedOptions object
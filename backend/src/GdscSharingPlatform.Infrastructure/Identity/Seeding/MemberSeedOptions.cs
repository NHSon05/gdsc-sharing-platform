namespace GdscSharingPlatform.Infrastructure.Identity.Seeding;

public sealed class MemberSeedOptions
{
    public const string SectionName = "SeedMember";
    public bool Enabled { get; set; }
    public string Email { get; set; } = "snguyenhong8@gmail.com";
    public string Password { get; set; } = "MemberPassword@123!";
    public string FullName { get; set; } = "Nguyễn Hồng Sơn";
    public string? DisplayName { get; set; } = "Nguyễn Hồng Sơn";
    public string DepartmentCode { get; set; } = "SOFTWARE";
    public string? PhoneNumber { get; set; } = "0387756949";
    public string? StudentCode { get; set; } = "123230167";
    public string? GithubUrl { get; set; } = "https://github.com/NHSon05";
    public string? Bio { get; set; } = "FrontEnd";
    public string? AvatarUrl { get; set; }
    public int GenerationNumber { get; set; } = 8;
    public string RoleCode { get; set; } = "LEAD";
}

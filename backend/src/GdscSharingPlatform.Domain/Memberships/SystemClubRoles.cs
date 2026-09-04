namespace GdscSharingPlatform.Domain.Memberships;

public static class SystemClubRoles
{
    public const string Lead = "LEAD";
    public const string SubLead = "SUBLEAD";
    public const string CoreTeam = "CORETEAM";
    public const string Alumni = "ALUMNI";

    public static readonly IReadOnlyCollection<(
        string Code,
        string Name,
        int SortOrder)> All =
    [
        (Lead, "Lead", 10),
        (SubLead, "Sub Lead", 20),
        (CoreTeam, "Core Team", 30),  
        (Alumni, "Alumni", 40),  
    ];
}
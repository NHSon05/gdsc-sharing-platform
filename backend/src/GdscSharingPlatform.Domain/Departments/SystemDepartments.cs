namespace GdscSharingPlatform.Domain.Departments;

public static class SystemDepartments
{
    public const string Software = "Software";
    public const string AI = "AI";
    public const string Marketing = "Marketing";
    public const string Media = "Media";
    public const string Community = "Community";
    public const string Business = "Business";

    public static readonly IReadOnlyCollection<string> All =
    [
        Software,
        AI,
        Marketing,
        Media,
        Community,
        Business,
    ];
}

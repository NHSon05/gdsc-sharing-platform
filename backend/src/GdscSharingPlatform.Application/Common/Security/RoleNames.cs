namespace GdscSharingPlatform.Application.Common.Security;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Member = "Member";

    public static readonly IReadOnlyCollection<string> All =
    [
        Admin,
        Member
    ];
}

public static class PolicyNames
{
    public const string AdminOnly = "AdminOnly";
    public const string MemberOnly = "MemberOnly";
    public const string RequireActiveUser = "RequireActiveUser";
}

public static class AuthClaimNames
{
    public const string DepartmentId = "department_id";
    public const string Status = "status";
}

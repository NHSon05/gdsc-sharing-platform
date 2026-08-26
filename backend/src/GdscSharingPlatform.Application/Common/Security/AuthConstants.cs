namespace GdscSharingPlatform.Application.Common.Security;

public static class RoleNames
{
    public const string Admin = nameof(Admin);
    public const string Member = nameof(Member);

    public static readonly IReadOnlyCollection<string> All =
    [
        Admin,
        Member
    ];
}

public static class AuthPolicies
{
    public const string AdminOnly = nameof(AdminOnly);
    public const string MemberOnly = nameof(MemberOnly);
    public const string RequireActiveUser = nameof(RequireActiveUser);
}

public static class AuthClaimTypes
{
    public const string DepartmentId = "department_id";
    public const string Status = "status";
    public const string TokenVersion = "token_version";

}
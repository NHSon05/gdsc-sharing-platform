using GdscSharingPlatform.Application.Features.Auth.Models;

namespace GdscSharingPlatform.Infrastructure.Identity.Services;

internal static class CurrentUserMapper
{
    public static CurrentUserDto Map(
        ApplicationUser user,
        IEnumerable<string> roles
    )
    {
        DepartmentDto? department = user.Department is null
            ? null
            : new DepartmentDto(
                Id: user.Department.Id,
                Name: user.Department.Name
            );
        return new CurrentUserDto(
            Id: user.Id,
            Email: user.Email ?? string.Empty,
            DisplayName: user.DisplayName ?? user.FullName,
            StudentCode: user.StudentCode,
            Generation: user.Generation,
            AvatarUrl: user.AvatarUrl,
            Status: user.Status.ToString(),
            Department: department,
            Roles: roles.Distinct(StringComparer.Ordinal).ToArray()
        );
    }
    
}

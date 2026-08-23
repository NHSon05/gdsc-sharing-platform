using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Domain.Entities;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.Infrastructure.Identity.Seeding;

public sealed class DatabaseSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AdminSeedOptions _adminOptions;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder
    (
        ApplicationDbContext dbContext,
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager,
        IOptions<AdminSeedOptions> adminOptions,
        ILogger<DatabaseSeeder> logger
    )
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
        _userManager = userManager;
        _adminOptions = adminOptions.Value;
        _logger = logger;
    }

    // SeedAsync
    public async Task SeedAsync
    (
        CancellationToken cancellationToken = default
    )
    {
        await SeedRolesAsync();

        await SeedDepartmentAsync(cancellationToken);

        if (_adminOptions.Enabled)
        {
            await SeedAdminAsync(cancellationToken);
        }
    }

    // SeedRolesAsync
    public async Task SeedRolesAsync()
    {
        foreach (var roleName in RoleNames.All)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);

            if (roleExists)
            {
                continue;
            }

            var role = new IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = roleName
            };
            var result = await _roleManager.CreateAsync(role);

            EnsureSuccess(
                result,
                $"Failed to create Identity role '{roleName}'.");

            _logger.LogInformation(
                "Created Identity role {RoleName}.", roleName
            );
        }
    }
    // SeedDepartmentsAsync
    private async Task SeedDepartmentAsync
    (
        CancellationToken cancellationToken
    )
    {
        var departments = new[]
        {
            CreateDepartment(
                "MANAGEMENT",
                "Management",
                "Club management department",
                1),

            CreateDepartment(
                "SOFTWARE",
                "Software",
                "Software development department",
                2),

            CreateDepartment(
                "R&D",
                "R&D",
                "Research and development department",
                3),

            CreateDepartment(
                "MARKETING",
                "Marketing",
                "Marketing department",
                4)
        };

        var hasChanges = false;
        foreach (var department in departments)
        {
            var exists = await _dbContext.Departments
            .IgnoreQueryFilters()
            .AnyAsync(
                item => item.Code == department.Code,
                cancellationToken
            );

            if (exists)
            {
                continue;
            }

            _dbContext.Departments.Add(department);
            hasChanges = true;

            _logger.LogInformation(
                "Preparing Department {DepartmentCode}", department.Code
            );
        }
        if (hasChanges)
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken
            );
        }
    }

    // SeedAdminAsync
    private async Task SeedAdminAsync(
        CancellationToken cancellationToken)
    {
        var email = _adminOptions.Email.Trim();

        var admin =
            await _userManager.FindByEmailAsync(email);

        if (admin is null)
        {
            admin = await CreateAdminAsync(
                email,
                cancellationToken);
        }

        await AddRoleIfMissingAsync(
            admin,
            RoleNames.Admin);

        await AddRoleIfMissingAsync(
            admin,
            RoleNames.Member);
    }
    // CreateAdminAsync
    private async Task<ApplicationUser> CreateAdminAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var departmentCode =
            _adminOptions.DepartmentCode
                .Trim()
                .ToUpperInvariant();

        var department =
            await _dbContext.Departments
                .SingleOrDefaultAsync(
                    item => item.Code == departmentCode,
                    cancellationToken)
            ?? throw new InvalidOperationException(
                $"Department '{departmentCode}' was not found.");

        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,

            FullName = _adminOptions.FullName.Trim(),
            DisplayName = _adminOptions.FullName.Trim(),

            DepartmentId = department.Id,
            Status = UserStatus.Active,

            JoinedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,

            TimeZone = "Asia/Ho_Chi_Minh",
            Locale = "vi-VN",

            IsDeleted = false
        };

        var result = await _userManager.CreateAsync(
            admin,
            _adminOptions.Password);

        EnsureSuccess(
            result,
            "Failed to create the bootstrap Admin account.");

        _logger.LogInformation(
            "Created bootstrap Admin account with ID {UserId}.",
            admin.Id);

        return admin;
    }

    // AddRoleIfMissingAsync
    private async Task AddRoleIfMissingAsync(
        ApplicationUser user,
        string roleName)
    {
        var alreadyInRole =
            await _userManager.IsInRoleAsync(
                user,
                roleName);

        if (alreadyInRole)
        {
            return;
        }

        var result =
            await _userManager.AddToRoleAsync(
                user,
                roleName);

        EnsureSuccess(
            result,
            $"Failed to assign role '{roleName}'.");

        _logger.LogInformation(
            "Assigned role {RoleName} to user {UserId}.",
            roleName,
            user.Id);
    }

    // CreateDepartment
    private static Department CreateDepartment
    (
        string code,
        string name,
        string description,
        int displayOrder
    )
    {
        return new Department
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Description = description,
            DisplayOrder = displayOrder,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
    // Ensure Success
    private static void EnsureSuccess(
        IdentityResult result,
        string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(
            ";",
            result.Errors.Select(
                error => $"{error.Code}: {error.Description}"));
        throw new InvalidOperationException(
            $"{message} {errors}"
        );
    }
}
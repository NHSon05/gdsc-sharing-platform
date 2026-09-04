using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Domain.Departments;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Memberships;
using GdscSharingPlatform.Infrastructure.Persistence;
using GdscSharingPlatform.Infrastructure.Persistence.Backfill;
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
    private readonly MemberSeedOptions _memberOptions;
    private readonly LegacyProfileBackfillService? _backfillService;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ApplicationDbContext dbContext,
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager,
        IOptions<AdminSeedOptions> adminOptions,
        IOptions<MemberSeedOptions> memberOptions,
        ILogger<DatabaseSeeder> logger,
        LegacyProfileBackfillService? backfillService = null)
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
        _userManager = userManager;
        _adminOptions = adminOptions.Value;
        _memberOptions = memberOptions.Value;
        _logger = logger;
        _backfillService = backfillService;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync();

        await SeedClubRolesAsync(cancellationToken);

        await SeedDepartmentAsync(cancellationToken);

        await CleanupOldSeedDataAsync(cancellationToken);

        if (_adminOptions.Enabled)
        {
            await SeedAdminAsync(cancellationToken);
        }

        if (_memberOptions.Enabled)
        {
            await SeedMemberAsync(cancellationToken);
        }

        if (_backfillService is not null)
        {
            await _backfillService.BackfillAsync(cancellationToken);
        }
    }

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

    public async Task SeedClubRolesAsync(CancellationToken cancellationToken = default)
    {
        var hasChanges = false;
        foreach (var (code, name, sortOrder) in SystemClubRoles.All)
        {
            var exists = await _dbContext.ClubRoles
                .AnyAsync(r => r.Code == code, cancellationToken);

            if (exists)
            {
                continue;
            }

            var clubRole = new ClubRole(
                code: code,
                name: name,
                level: sortOrder,
                isActive: true);

            _dbContext.ClubRoles.Add(clubRole);
            hasChanges = true;

            _logger.LogInformation(
                "Created Club Role {ClubRoleCode} - {ClubRoleName}.", code, name);
        }

        if (hasChanges)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedDepartmentAsync(CancellationToken cancellationToken)
    {
        var departments = new[]
        {
            CreateDepartment(
                "MANAGEMENT",
                "Management",
                "management",
                "Club management department",
                1,
                "#64748B",
                "briefcase"),

            CreateDepartment(
                "SOFTWARE",
                SystemDepartments.Software,
                "software",
                "Software development department",
                10,
                "#3B82F6",
                "code"),

            CreateDepartment(
                "AI",
                SystemDepartments.AI,
                "ai",
                "Artificial intelligence and data science department",
                20,
                "#8B5CF6",
                "cpu"),

            CreateDepartment(
                "R&D",
                "R&D",
                "rd",
                "Research and development department",
                3,
                "#06B6D4",
                "flask"),

            CreateDepartment(
                "MARKETING",
                SystemDepartments.Marketing,
                "marketing",
                "Marketing department",
                30,
                "#EC4899",
                "megaphone"),

            CreateDepartment(
                "MEDIA",
                SystemDepartments.Media,
                "media",
                "Media and design department",
                40,
                "#F59E0B",
                "camera"),

            CreateDepartment(
                "COMMUNITY",
                SystemDepartments.Community,
                "community",
                "Community engagement department",
                50,
                "#10B981",
                "users")
        };

        var hasChanges = false;
        foreach (var department in departments)
        {
            var existing = await _dbContext.Departments
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    item => item.Code == department.Code,
                    cancellationToken
                );

            if (existing is null)
            {
                _dbContext.Departments.Add(department);
                hasChanges = true;

                _logger.LogInformation(
                    "Preparing Department {DepartmentCode}", department.Code
                );
            }
            else
            {
                if (string.IsNullOrWhiteSpace(existing.Slug))
                {
                    existing.Slug = department.Slug;
                    hasChanges = true;
                }

                if (string.IsNullOrWhiteSpace(existing.Color) && !string.IsNullOrWhiteSpace(department.Color))
                {
                    existing.Color = department.Color;
                    hasChanges = true;
                }

                if (string.IsNullOrWhiteSpace(existing.Icon) && !string.IsNullOrWhiteSpace(department.Icon))
                {
                    existing.Icon = department.Icon;
                    hasChanges = true;
                }
            }
        }

        if (hasChanges)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task CleanupOldSeedDataAsync(CancellationToken cancellationToken)
    {
        var candidateStaleEmails = new[] { "member@gdsc.dev", "admin@gdsc.com" };
        foreach (var staleEmail in candidateStaleEmails)
        {
            if (string.Equals(_memberOptions.Email, staleEmail, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_adminOptions.Email, staleEmail, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var staleUser = await _userManager.FindByEmailAsync(staleEmail);
            if (staleUser is not null)
            {
                var clubMemberships = await _dbContext.ClubMemberships
                    .Where(cm => cm.UserId == staleUser.Id)
                    .ToListAsync(cancellationToken);

                var clubMembershipIds = clubMemberships.Select(cm => cm.Id).ToList();

                var deptMemberships = await _dbContext.DepartmentMemberships
                    .Where(dm => clubMembershipIds.Contains(dm.ClubMembershipId))
                    .ToListAsync(cancellationToken);

                var deptMembershipIds = deptMemberships.Select(dm => dm.Id).ToList();

                var roleAssignments = await _dbContext.RoleAssignments
                    .Where(ra => deptMembershipIds.Contains(ra.DepartmentMembershipId) || ra.AssignedByUserId == staleUser.Id)
                    .ToListAsync(cancellationToken);

                _dbContext.RoleAssignments.RemoveRange(roleAssignments);
                _dbContext.DepartmentMemberships.RemoveRange(deptMemberships);
                _dbContext.ClubMemberships.RemoveRange(clubMemberships);
                await _dbContext.SaveChangesAsync(cancellationToken);

                await _userManager.DeleteAsync(staleUser);
                _logger.LogInformation("Cleaned up old seed account: {Email}", staleEmail);
            }
        }
    }

    private async Task SeedAdminAsync(CancellationToken cancellationToken)
    {
        var email = _adminOptions.Email.Trim();
        var admin = await _userManager.FindByEmailAsync(email);

        var department = await _dbContext.Departments
            .SingleOrDefaultAsync(
                item => item.Code == _adminOptions.DepartmentCode.Trim().ToUpperInvariant(),
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"Department '{_adminOptions.DepartmentCode}' was not found.");

        var normalizedStudentCode = string.IsNullOrWhiteSpace(_adminOptions.StudentCode)
            ? null
            : _adminOptions.StudentCode.Trim().ToUpperInvariant();

        if (admin is null && normalizedStudentCode is not null)
        {
            admin = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.StudentCode == normalizedStudentCode, cancellationToken);

            if (admin is not null)
            {
                _logger.LogInformation("Found Admin by StudentCode {StudentCode}. Syncing email to {Email}.", normalizedStudentCode, email);
                admin.Email = email;
                admin.NormalizedEmail = email.ToUpperInvariant();
                admin.UserName = email;
                admin.NormalizedUserName = email.ToUpperInvariant();
                await _userManager.UpdateAsync(admin);
            }
        }

        if (admin is null)
        {
            if (normalizedStudentCode is not null)
            {
                var conflict = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.StudentCode == normalizedStudentCode, cancellationToken);
                if (conflict is not null)
                {
                    _logger.LogWarning("Clearing conflicting StudentCode {StudentCode} from user {Email}.", normalizedStudentCode, conflict.Email);
                    conflict.StudentCode = null;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }

            admin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = _adminOptions.FullName.Trim(),
                DisplayName = string.IsNullOrWhiteSpace(_adminOptions.DisplayName) ? _adminOptions.FullName.Trim() : _adminOptions.DisplayName.Trim(),
                PhoneNumber = _adminOptions.PhoneNumber,
                StudentCode = normalizedStudentCode,
                GitHubUrl = _adminOptions.GithubUrl,
                Bio = _adminOptions.Bio,
                AvatarUrl = _adminOptions.AvatarUrl,
                DepartmentId = department.Id,
                Generation = $"Gen {_adminOptions.GenerationNumber}",
                Status = UserStatus.Active,
                JoinedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                TimeZone = "Asia/Ho_Chi_Minh",
                Locale = "vi-VN",
                IsDeleted = false
            };

            var result = await _userManager.CreateAsync(admin, _adminOptions.Password);
            EnsureSuccess(result, $"Failed to create account with email '{email}'.");
            _logger.LogInformation("Created Admin account with ID {UserId} and email {Email}.", admin.Id, email);
        }
        else
        {
            if (normalizedStudentCode is not null)
            {
                var conflict = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Id != admin.Id && u.StudentCode == normalizedStudentCode, cancellationToken);
                if (conflict is not null)
                {
                    _logger.LogWarning("Clearing conflicting StudentCode {StudentCode} from user {Email}.", normalizedStudentCode, conflict.Email);
                    conflict.StudentCode = null;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }

            admin.FullName = _adminOptions.FullName.Trim();
            admin.DisplayName = string.IsNullOrWhiteSpace(_adminOptions.DisplayName) ? _adminOptions.FullName.Trim() : _adminOptions.DisplayName.Trim();
            admin.PhoneNumber = _adminOptions.PhoneNumber;
            admin.StudentCode = normalizedStudentCode;
            admin.GitHubUrl = _adminOptions.GithubUrl;
            admin.Bio = _adminOptions.Bio;
            admin.AvatarUrl = _adminOptions.AvatarUrl;
            admin.DepartmentId = department.Id;
            admin.Generation = $"Gen {_adminOptions.GenerationNumber}";
            admin.UpdatedAt = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(admin);
        }

        await AddRoleIfMissingAsync(admin, RoleNames.Admin);

        if (await _userManager.IsInRoleAsync(admin, RoleNames.Member))
        {
            await _userManager.RemoveFromRoleAsync(admin, RoleNames.Member);
            _logger.LogInformation("Removed Member role from Admin user {UserId}.", admin.Id);
        }

        await EnsureUserMembershipAsync(
            admin,
            _adminOptions.GenerationNumber,
            _adminOptions.DepartmentCode.Trim().ToUpperInvariant(),
            _adminOptions.RoleCode.Trim().ToUpperInvariant(),
            cancellationToken);
    }

    private async Task SeedMemberAsync(CancellationToken cancellationToken)
    {
        var email = _memberOptions.Email.Trim();
        var member = await _userManager.FindByEmailAsync(email);

        var department = await _dbContext.Departments
            .SingleOrDefaultAsync(
                item => item.Code == _memberOptions.DepartmentCode.Trim().ToUpperInvariant(),
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"Department '{_memberOptions.DepartmentCode}' was not found.");

        var normalizedStudentCode = string.IsNullOrWhiteSpace(_memberOptions.StudentCode)
            ? null
            : _memberOptions.StudentCode.Trim().ToUpperInvariant();

        if (member is null && normalizedStudentCode is not null)
        {
            member = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.StudentCode == normalizedStudentCode, cancellationToken);

            if (member is not null)
            {
                _logger.LogInformation("Found Member by StudentCode {StudentCode}. Syncing email to {Email}.", normalizedStudentCode, email);
                member.Email = email;
                member.NormalizedEmail = email.ToUpperInvariant();
                member.UserName = email;
                member.NormalizedUserName = email.ToUpperInvariant();
                await _userManager.UpdateAsync(member);
            }
        }

        if (member is null)
        {
            if (normalizedStudentCode is not null)
            {
                var conflict = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.StudentCode == normalizedStudentCode, cancellationToken);
                if (conflict is not null)
                {
                    _logger.LogWarning("Clearing conflicting StudentCode {StudentCode} from user {Email}.", normalizedStudentCode, conflict.Email);
                    conflict.StudentCode = null;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }

            member = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = _memberOptions.FullName.Trim(),
                DisplayName = string.IsNullOrWhiteSpace(_memberOptions.DisplayName) ? _memberOptions.FullName.Trim() : _memberOptions.DisplayName.Trim(),
                PhoneNumber = _memberOptions.PhoneNumber,
                StudentCode = normalizedStudentCode,
                GitHubUrl = _memberOptions.GithubUrl,
                Bio = _memberOptions.Bio,
                AvatarUrl = _memberOptions.AvatarUrl,
                DepartmentId = department.Id,
                Generation = $"Gen {_memberOptions.GenerationNumber}",
                Status = UserStatus.Active,
                JoinedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                TimeZone = "Asia/Ho_Chi_Minh",
                Locale = "vi-VN",
                IsDeleted = false
            };

            var result = await _userManager.CreateAsync(member, _memberOptions.Password);
            EnsureSuccess(result, $"Failed to create account with email '{email}'.");
            _logger.LogInformation("Created Member account with ID {UserId} and email {Email}.", member.Id, email);
        }
        else
        {
            if (normalizedStudentCode is not null)
            {
                var conflict = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Id != member.Id && u.StudentCode == normalizedStudentCode, cancellationToken);
                if (conflict is not null)
                {
                    _logger.LogWarning("Clearing conflicting StudentCode {StudentCode} from user {Email}.", normalizedStudentCode, conflict.Email);
                    conflict.StudentCode = null;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }

            member.FullName = _memberOptions.FullName.Trim();
            member.DisplayName = string.IsNullOrWhiteSpace(_memberOptions.DisplayName) ? _memberOptions.FullName.Trim() : _memberOptions.DisplayName.Trim();
            member.PhoneNumber = _memberOptions.PhoneNumber;
            member.StudentCode = normalizedStudentCode;
            member.GitHubUrl = _memberOptions.GithubUrl;
            member.Bio = _memberOptions.Bio;
            member.AvatarUrl = _memberOptions.AvatarUrl;
            member.DepartmentId = department.Id;
            member.Generation = $"Gen {_memberOptions.GenerationNumber}";
            member.UpdatedAt = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(member);
        }

        await AddRoleIfMissingAsync(member, RoleNames.Member);

        if (await _userManager.IsInRoleAsync(member, RoleNames.Admin))
        {
            await _userManager.RemoveFromRoleAsync(member, RoleNames.Admin);
            _logger.LogInformation("Removed Admin role from Member user {UserId}.", member.Id);
        }

        await EnsureUserMembershipAsync(
            member,
            _memberOptions.GenerationNumber,
            _memberOptions.DepartmentCode.Trim().ToUpperInvariant(),
            _memberOptions.RoleCode.Trim().ToUpperInvariant(),
            cancellationToken);
    }

    private async Task<ClubGeneration> GetOrCreateGenerationAsync(int generationNumber, CancellationToken cancellationToken)
    {
        var generation = await _dbContext.ClubGenerations
            .SingleOrDefaultAsync(g => g.Number == generationNumber, cancellationToken);

        if (generation is null)
        {
            generation = new ClubGeneration(
                number: generationNumber,
                startDate: new DateOnly(2025, 9, 1),
                endDate: new DateOnly(2026, 8, 31));

            _dbContext.ClubGenerations.Add(generation);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created Club Generation {GenNumber} - {GenName}", generation.Number, generation.Name);
        }

        return generation;
    }

    private async Task EnsureUserMembershipAsync(
        ApplicationUser user,
        int generationNumber,
        string departmentCode,
        string roleCode,
        CancellationToken cancellationToken)
    {
        var generation = await GetOrCreateGenerationAsync(generationNumber, cancellationToken);

        var department = await _dbContext.Departments
            .SingleOrDefaultAsync(d => d.Code == departmentCode, cancellationToken)
            ?? throw new InvalidOperationException($"Department '{departmentCode}' was not found.");

        var clubRole = await _dbContext.ClubRoles
            .SingleOrDefaultAsync(r => r.Code == roleCode, cancellationToken)
            ?? throw new InvalidOperationException($"Club Role '{roleCode}' was not found.");

        // 1. ClubMembership
        var clubMembership = await _dbContext.ClubMemberships
            .Include(cm => cm.DepartmentMemberships)
                .ThenInclude(dm => dm.RoleAssignments)
            .FirstOrDefaultAsync(cm => cm.UserId == user.Id && cm.GenerationId == generation.Id, cancellationToken);

        if (clubMembership is null)
        {
            clubMembership = new ClubMembership(user.Id, generation.Id, joinedAt: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.ClubMemberships.Add(clubMembership);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // 2. DepartmentMembership
        var deptMembership = clubMembership.DepartmentMemberships
            .FirstOrDefault(dm => dm.DepartmentId == department.Id);

        if (deptMembership is null)
        {
            deptMembership = new DepartmentMembership(
                clubMembership.Id,
                department.Id,
                isPrimary: true,
                joinedAt: DateOnly.FromDateTime(DateTime.UtcNow));
            _dbContext.DepartmentMemberships.Add(deptMembership);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (!deptMembership.IsPrimary)
        {
            deptMembership.SetPrimary(true);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // 3. RoleAssignment
        var roleAssignment = deptMembership.RoleAssignments
            .FirstOrDefault(ra => ra.ClubRoleId == clubRole.Id);

        if (roleAssignment is null)
        {
            roleAssignment = new RoleAssignment(deptMembership.Id, clubRole.Id, user.Id);
            _dbContext.RoleAssignments.Add(roleAssignment);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (!roleAssignment.IsActive)
        {
            roleAssignment.Reactivate();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task AddRoleIfMissingAsync(
        ApplicationUser user,
        string roleName)
    {
        var alreadyInRole = await _userManager.IsInRoleAsync(user, roleName);

        if (alreadyInRole)
        {
            return;
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);

        EnsureSuccess(
            result,
            $"Failed to assign role '{roleName}'.");

        _logger.LogInformation(
            "Assigned role {RoleName} to user {UserId}.",
            roleName,
            user.Id);
    }

    private static Department CreateDepartment(
        string code,
        string name,
        string slug,
        string description,
        int sortOrder,
        string color,
        string icon)
    {
        return new Department
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Slug = slug,
            Description = description,
            SortOrder = sortOrder,
            Color = color,
            Icon = icon,
            IsActive = true
        };
    }

    private static void EnsureSuccess(
        IdentityResult result,
        string errorMessage)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(
            ", ",
            result.Errors.Select(error => error.Description)
        );

        throw new InvalidOperationException(
            $"{errorMessage} Errors: {errors}"
        );
    }
}
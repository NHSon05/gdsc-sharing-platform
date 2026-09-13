using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Memberships;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Identity.Seeding;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.UnitTests.Infrastructure.Identity.Seeding;

public class DatabaseSeederTests
{
    private (ServiceProvider Provider, ApplicationDbContext DbContext, DatabaseSeeder Seeder) CreateSeederEnvironment(
        AdminSeedOptions? adminOptions = null,
        MemberSeedOptions? memberOptions = null)
    {
        var services = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();

        services.AddLogging(builder => builder.AddDebug());

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase(dbName);
        });

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        var adminModel = adminOptions ?? new AdminSeedOptions
        {
            Enabled = true,
            Email = "admin@gdsc.test",
            Password = "Password123!",
            FullName = "Platform Administrator",
            DepartmentCode = "MANAGEMENT"
        };

        var memberModel = memberOptions ?? new MemberSeedOptions
        {
            Enabled = false,
            Email = "member@gdsc.test",
            Password = "Password123!",
            FullName = "Platform Member",
            DepartmentCode = "SOFTWARE"
        };

        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(adminModel));
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(memberModel));
        services.AddScoped<DatabaseSeeder>();

        var provider = services.BuildServiceProvider();
        var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

        return (provider, dbContext, seeder);
    }

    [Fact]
    public async Task SeedRolesAsync_ShouldCreateAllRolesDefinedInRoleNames()
    {
        // Arrange
        var (_, dbContext, seeder) = CreateSeederEnvironment();

        // Act
        await seeder.SeedRolesAsync();

        // Assert
        var roles = await dbContext.Roles.Select(r => r.Name).ToListAsync();
        foreach (var expectedRole in RoleNames.All)
        {
            Assert.Contains(expectedRole, roles);
        }
        Assert.Equal(RoleNames.All.Count, roles.Count);
    }

    [Fact]
    public async Task SeedRolesAsync_WhenRunMultipleTimes_ShouldBeIdempotent()
    {
        // Arrange
        var (_, dbContext, seeder) = CreateSeederEnvironment();

        // Act
        await seeder.SeedRolesAsync();
        await seeder.SeedRolesAsync(); // Second run

        // Assert
        var roles = await dbContext.Roles.ToListAsync();
        Assert.Equal(RoleNames.All.Count, roles.Count);
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateAllDefaultDepartments()
    {
        // Arrange
        var (_, dbContext, seeder) = CreateSeederEnvironment();

        // Act
        await seeder.SeedAsync();

        // Assert
        var departments = await dbContext.Departments.ToListAsync();
        Assert.Equal(7, departments.Count);

        var codes = departments.Select(d => d.Code).ToList();
        Assert.Contains("MANAGEMENT", codes);
        Assert.Contains("SOFTWARE", codes);
        Assert.Contains("R&D", codes);
        Assert.Contains("MARKETING", codes);
        Assert.Contains("AI", codes);
        Assert.Contains("MEDIA", codes);
        Assert.Contains("COMMUNITY", codes);
    }

    [Fact]
    public async Task SeedAsync_WhenAdminEnabled_ShouldCreateAdminUserAndAssignAdminRoleOnly()
    {
        // Arrange
        var adminOptions = new AdminSeedOptions
        {
            Enabled = true,
            Email = "admin@gdsc.club",
            Password = "StrongPassword123!",
            FullName = "Club Admin",
            DepartmentCode = "MANAGEMENT"
        };
        var (provider, dbContext, seeder) = CreateSeederEnvironment(adminOptions);
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Act
        await seeder.SeedAsync();

        // Assert
        var user = await userManager.FindByEmailAsync("admin@gdsc.club");
        Assert.NotNull(user);
        Assert.Equal("Club Admin", user.FullName);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.True(user.EmailConfirmed);

        var mgmtDept = await dbContext.Departments.SingleAsync(d => d.Code == "MANAGEMENT");
        Assert.Equal(mgmtDept.Id, user.DepartmentId);

        var isInAdminRole = await userManager.IsInRoleAsync(user, RoleNames.Admin);
        var isInMemberRole = await userManager.IsInRoleAsync(user, RoleNames.Member);
        Assert.True(isInAdminRole);
        Assert.False(isInMemberRole); // Admin chỉ có role Admin
    }

    [Fact]
    public async Task SeedAsync_WhenMemberEnabled_ShouldCreateMemberUserAndAssignMemberRoleOnly()
    {
        // Arrange
        var adminOptions = new AdminSeedOptions { Enabled = false };
        var memberOptions = new MemberSeedOptions
        {
            Enabled = true,
            Email = "member@gdsc.club",
            Password = "StrongPassword123!",
            FullName = "Club Member",
            DepartmentCode = "SOFTWARE"
        };
        var (provider, dbContext, seeder) = CreateSeederEnvironment(adminOptions, memberOptions);
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Act
        await seeder.SeedAsync();

        // Assert
        var user = await userManager.FindByEmailAsync("member@gdsc.club");
        Assert.NotNull(user);
        Assert.Equal("Club Member", user.FullName);
        Assert.Equal(UserStatus.Active, user.Status);

        var softDept = await dbContext.Departments.SingleAsync(d => d.Code == "SOFTWARE");
        Assert.Equal(softDept.Id, user.DepartmentId);

        var isInAdminRole = await userManager.IsInRoleAsync(user, RoleNames.Admin);
        var isInMemberRole = await userManager.IsInRoleAsync(user, RoleNames.Member);
        Assert.False(isInAdminRole);
        Assert.True(isInMemberRole); // Member chỉ có role Member
    }

    [Fact]
    public async Task SeedAsync_WhenUsersDisabled_ShouldNotCreateUsers()
    {
        // Arrange
        var adminOptions = new AdminSeedOptions { Enabled = false };
        var memberOptions = new MemberSeedOptions { Enabled = false };
        var (provider, dbContext, seeder) = CreateSeederEnvironment(adminOptions, memberOptions);
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Act
        await seeder.SeedAsync();

        // Assert
        var users = await dbContext.Users.ToListAsync();
        Assert.Empty(users);

        // Departments & Roles should still be seeded
        var roles = await dbContext.Roles.ToListAsync();
        Assert.NotEmpty(roles);
        var departments = await dbContext.Departments.ToListAsync();
        Assert.NotEmpty(departments);
    }

    [Fact]
    public async Task SeedAsync_WhenRunTwice_ShouldNotThrowAndKeepDataConsistent()
    {
        // Arrange
        var adminOptions = new AdminSeedOptions
        {
            Enabled = true,
            Email = "repeat@gdsc.club",
            Password = "Password123!",
            FullName = "Repeat Admin",
            DepartmentCode = "SOFTWARE"
        };
        var memberOptions = new MemberSeedOptions
        {
            Enabled = true,
            Email = "repeat.member@gdsc.club",
            Password = "Password123!",
            FullName = "Repeat Member",
            DepartmentCode = "R&D"
        };
        var (_, dbContext, seeder) = CreateSeederEnvironment(adminOptions, memberOptions);

        // Act
        await seeder.SeedAsync();
        var exception = await Record.ExceptionAsync(() => seeder.SeedAsync());

        // Assert
        Assert.Null(exception);
        var users = await dbContext.Users.ToListAsync();
        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task SeedClubRolesAsync_ShouldCreateAllRolesDefinedInSystemClubRoles()
    {
        // Arrange
        var (_, dbContext, seeder) = CreateSeederEnvironment();

        // Act
        await seeder.SeedClubRolesAsync();

        // Assert
        var clubRoles = await dbContext.ClubRoles.ToListAsync();
        Assert.Equal(SystemClubRoles.All.Count, clubRoles.Count);

        foreach (var (code, name, sortOrder) in SystemClubRoles.All)
        {
            var role = clubRoles.SingleOrDefault(r => r.Code == code);
            Assert.NotNull(role);
            Assert.Equal(name, role.Name);
            Assert.Equal(sortOrder, role.Level);
            Assert.True(role.IsActive);
        }
    }

    [Fact]
    public async Task SeedClubRolesAsync_WhenRunMultipleTimes_ShouldBeIdempotent()
    {
        // Arrange
        var (_, dbContext, seeder) = CreateSeederEnvironment();

        // Act
        await seeder.SeedClubRolesAsync();
        await seeder.SeedClubRolesAsync();

        // Assert
        var clubRoles = await dbContext.ClubRoles.ToListAsync();
        Assert.Equal(SystemClubRoles.All.Count, clubRoles.Count);
    }
    [Fact]
    public async Task RoadmapCategories_SeedIsIdempotentAndPreservesAdminChanges()
    {
        var (_, dbContext, seeder) = CreateSeederEnvironment(
            new AdminSeedOptions { Enabled = false }, new MemberSeedOptions { Enabled = false });
        await seeder.SeedAsync();
        var category = await dbContext.RoadmapCategories.SingleAsync(x => x.Slug == "frontend");
        var originalId = category.Id;
        category.Update("Custom Frontend", "frontend", 99);
        category.SetActive(false);
        category.Description = "Admin description";
        await dbContext.SaveChangesAsync();

        await seeder.SeedAsync();
        var categories = await dbContext.RoadmapCategories.ToListAsync();
        Assert.Equal(6, categories.Count);
        Assert.Equal(6, categories.Select(x => x.Slug).Distinct().Count());
        Assert.Contains(categories, x => x.Name == "Artificial Intelligence");
        var preserved = categories.Single(x => x.Id == originalId);
        Assert.Equal("Custom Frontend", preserved.Name);
        Assert.Equal("Admin description", preserved.Description);
        Assert.Equal(99, preserved.SortOrder);
        Assert.False(preserved.IsActive);
    }


    [Fact]
    public async Task Roadmaps_SeedPublishedGraphsAndPreserveAdminEditsIncludingSlug()
    {
        var (_, db, seeder) = CreateSeederEnvironment();
        await seeder.SeedAsync();
        var roadmaps = await db.Roadmaps.Include(x => x.Nodes).Include(x => x.Edges).ToListAsync();
        Assert.Equal(6, roadmaps.Count);
        Assert.All(roadmaps, roadmap =>
        {
            Assert.Equal(RoadmapStatus.Published, roadmap.Status);
            Assert.NotNull(roadmap.PublishedAtUtc);
            Assert.Equal(5, roadmap.Nodes.Count);
            Assert.Equal(4, roadmap.Edges.Count);
            Assert.All(roadmap.Edges, edge =>
            {
                Assert.Contains(roadmap.Nodes, node => node.Id == edge.SourceNodeId);
                Assert.Contains(roadmap.Nodes, node => node.Id == edge.TargetNodeId);
                Assert.Equal(RoadmapRelationType.Required, edge.RelationType);
            });
        });
        var frontend = roadmaps.Single(x => x.Slug == "frontend");
        frontend.Update(frontend.CategoryId, "Admin curriculum", "admin-curriculum", "Edited summary",
            RoadmapLevel.Advanced, 99, frontend.CreatedByUserId);
        frontend.ChangeStatus(RoadmapStatus.Draft, true, frontend.CreatedByUserId);
        var node = frontend.Nodes.First();
        node.SetPosition(10, 20, 350);
        node.SetActive(false);
        frontend.Edges.First().SetActive(false);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await seeder.SeedAsync();
        Assert.Equal(6, await db.Roadmaps.CountAsync());
        Assert.Equal(30, await db.RoadmapNodes.CountAsync());
        Assert.Equal(24, await db.RoadmapEdges.CountAsync());
        var preserved = await db.Roadmaps.FindAsync(frontend.Id);
        Assert.Equal("Admin curriculum", preserved!.Title);
        Assert.Equal("admin-curriculum", preserved.Slug);
        Assert.Equal(RoadmapStatus.Draft, preserved.Status);
        Assert.Equal(99, preserved.SortOrder);
        var preservedNode = await db.RoadmapNodes.FindAsync(node.Id);
        Assert.False(preservedNode!.IsActive);
        Assert.Equal(10, preservedNode.PositionX);
        Assert.Equal(350, preservedNode.Width);
        Assert.Equal(1, await db.RoadmapEdges.CountAsync(x => !x.IsActive));
    }

    [Fact]
    public async Task Roadmaps_WithoutAdminDeferSeedUntilAnAdminExists()
    {
        var (_, db, seeder) = CreateSeederEnvironment(new AdminSeedOptions { Enabled = false });
        await seeder.SeedAsync();
        Assert.Empty(await db.Roadmaps.ToListAsync());
        Assert.Empty(await db.Users.ToListAsync());

        var admin = new ApplicationUser { UserName = "existing-admin", Status = UserStatus.Active };
        db.Users.Add(admin);
        var role = await db.Roles.SingleAsync(x => x.Name == RoleNames.Admin);
        db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = admin.Id, RoleId = role.Id });
        await db.SaveChangesAsync();
        await seeder.SeedRoadmapsAsync();
        Assert.Equal(6, await db.Roadmaps.CountAsync());
        Assert.All(await db.Roadmaps.ToListAsync(), x => Assert.Equal(admin.Id, x.CreatedByUserId));
    }

    [Fact]
    public async Task Roadmaps_ExistingSlugIsNotOverwrittenOrGivenSeedNodes()
    {
        var (_, db, seeder) = CreateSeederEnvironment();
        await seeder.SeedAsync();
        var frontend = await db.Roadmaps.SingleAsync(x => x.Slug == "frontend");
        db.RoadmapEdges.RemoveRange(await db.RoadmapEdges.Where(x => x.RoadmapId == frontend.Id).ToListAsync());
        db.RoadmapNodes.RemoveRange(await db.RoadmapNodes.Where(x => x.RoadmapId == frontend.Id).ToListAsync());
        db.Roadmaps.Remove(frontend);
        await db.SaveChangesAsync();
        var custom = new GdscSharingPlatform.Domain.Roadmaps.Roadmap(frontend.CategoryId,
            "Existing curriculum", "frontend", "Keep my content", RoadmapLevel.Advanced, frontend.CreatedByUserId);
        db.Roadmaps.Add(custom);
        await db.SaveChangesAsync();

        await seeder.SeedRoadmapsAsync();
        Assert.Equal(6, await db.Roadmaps.CountAsync());
        Assert.Equal(custom.Id, (await db.Roadmaps.SingleAsync(x => x.Slug == "frontend")).Id);
        Assert.False(await db.RoadmapNodes.AnyAsync(x => x.RoadmapId == custom.Id));
    }

}

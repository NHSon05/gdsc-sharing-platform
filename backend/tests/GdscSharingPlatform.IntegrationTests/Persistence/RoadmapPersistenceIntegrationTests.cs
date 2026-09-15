using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Identity.Seeding;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace GdscSharingPlatform.IntegrationTests.Persistence;

// Each test creates and drops its own database. No application database is modified.
public sealed partial class RoadmapPersistenceIntegrationTests : IAsyncLifetime
{
    private string? _connectionString;
    private string? _databaseName;
    private const string ConnectionVariable = "ROADMAP_TEST_POSTGRES";
    private const string Sprint2Migration = "20260903105331_AddMultiMembershipProfileModel";

    public async Task InitializeAsync()
    {
        var configured = Environment.GetEnvironmentVariable(ConnectionVariable);
        if (string.IsNullOrWhiteSpace(configured)) return;

        _databaseName = $"roadmap_tests_{Guid.NewGuid():N}";
        var builder = new NpgsqlConnectionStringBuilder(configured) { Database = "postgres", Pooling = false };
        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE DATABASE \"{_databaseName}\"", connection);
        await command.ExecuteNonQueryAsync();
        builder.Database = _databaseName;
        _connectionString = builder.ConnectionString;
    }

    public async Task DisposeAsync()
    {
        if (_databaseName is null) return;
        var builder = new NpgsqlConnectionStringBuilder(_connectionString) { Database = "postgres" };
        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE \"{_databaseName}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }

    private ApplicationDbContext CreateContext() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(_connectionString).Options);

    [PostgresTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Migration_WorksOnCleanDatabaseAndPreservesSprint2Data(bool upgrade)
    {
        await using var context = CreateContext();
        var userId = Guid.NewGuid();
        if (upgrade)
        {
            await context.GetService<IMigrator>().MigrateAsync(Sprint2Migration);
            context.Users.Add(new ApplicationUser { Id = userId, FullName = "Existing Sprint 2 member" });
            await context.SaveChangesAsync();
        }

        await context.Database.MigrateAsync();
        Assert.False(context.Database.HasPendingModelChanges());
        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
        Assert.Empty(await context.Roadmaps.ToListAsync());
        Assert.Empty(await context.RoadmapCategories.ToListAsync());
        Assert.Empty(await context.RoadmapNodes.ToListAsync());
        Assert.Empty(await context.RoadmapEdges.ToListAsync());
        Assert.Empty(await context.LearningResources.ToListAsync());
        if (upgrade) Assert.True(await context.Users.AnyAsync(x => x.Id == userId));

        // The feature migration is reversible without removing Sprint 2 tables/data.
        await context.GetService<IMigrator>().MigrateAsync(Sprint2Migration);
        if (upgrade) Assert.True(await context.Users.AnyAsync(x => x.Id == userId));
        await context.Database.MigrateAsync();
        Assert.Empty(await context.Roadmaps.ToListAsync());
    }

    [PostgresFact]
    public async Task Seeder_RunsTwiceAndPreservesExistingCategories()
    {
        await using (var context = CreateContext()) await context.Database.MigrateAsync();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(_connectionString));
        services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        services.Configure<AdminSeedOptions>(options => options.Enabled = false);
        services.Configure<MemberSeedOptions>(options => options.Enabled = false);
        services.AddScoped<DatabaseSeeder>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await seeder.SeedAsync();
        var category = await db.RoadmapCategories.SingleAsync(x => x.Slug == "frontend");
        category.Update("Edited Frontend", "frontend", 42);
        category.SetActive(false);
        await db.SaveChangesAsync();
        await seeder.SeedAsync();
        db.ChangeTracker.Clear();
        Assert.Equal(6, await db.RoadmapCategories.CountAsync());
        category = await db.RoadmapCategories.SingleAsync(x => x.Slug == "frontend");
        Assert.False(category.IsActive);
        Assert.Equal("Edited Frontend", category.Name);
        Assert.Equal(42, category.SortOrder);
    }

    private async Task<(Roadmap Roadmap, RoadmapNode Source, RoadmapNode Target, ApplicationUser User)> SeedGraphAsync(ApplicationDbContext db)
    {
        await db.Database.MigrateAsync();
        var user = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Roadmap author" };
        var category = new RoadmapCategory("Frontend", "frontend");
        var roadmap = new Roadmap(category.Id, "Frontend", "frontend", "Learn frontend", RoadmapLevel.Beginner, user.Id);
        var source = new RoadmapNode(roadmap.Id, "HTML", "html", positionX: -120.1234m, positionY: 320.5678m, width: 200.25m);
        var target = new RoadmapNode(roadmap.Id, "CSS", "css");
        db.AddRange(user, category, roadmap, source, target,
            new RoadmapEdge(roadmap.Id, source.Id, target.Id),
            LearningResource.CreateLink(source.Id, "Guide", "https://example.com", user.Id),
            LearningResource.CreateFile(source.Id, "PDF", "guide.pdf", "stored.pdf", "resources/stored.pdf", 100, "application/pdf", user.Id));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return (roadmap, source, target, user);
    }

    [PostgresFact]
    public async Task Graph_RoundTripsCoordinatesAndKeepsInactiveContent()
    {
        await using var db = CreateContext();
        var (roadmap, source, _, _) = await SeedGraphAsync(db);
        var node = await db.RoadmapNodes.Include(x => x.Resources).Include(x => x.OutgoingEdges).SingleAsync(x => x.Id == source.Id);
        Assert.Equal(-120.1234m, node.PositionX);
        Assert.Equal(320.5678m, node.PositionY);
        Assert.Equal(200.25m, node.Width);
        Assert.Equal(2, node.Resources.Count);
        Assert.Single(node.OutgoingEdges);
        node.SetActive(false);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.False((await db.RoadmapNodes.SingleAsync(x => x.Id == source.Id)).IsActive);
        Assert.Equal(2, await db.LearningResources.CountAsync());
        Assert.Single(await db.RoadmapEdges.ToListAsync());
        var loaded = await db.Roadmaps.SingleAsync(x => x.Id == roadmap.Id);
        Assert.Equal(RoadmapStatus.Published, loaded.Status);
        Assert.NotNull(loaded.PublishedAtUtc);

        // Draft (enum zero) must be saved explicitly despite the Published DB default.
        var draft = new Roadmap(roadmap.CategoryId, "Draft", "draft", "Summary", RoadmapLevel.AllLevels, roadmap.CreatedByUserId);
        db.Add(draft);
        db.Entry(draft).Property(x => x.Status).CurrentValue = RoadmapStatus.Draft;
        db.Entry(draft).Property(x => x.PublishedAtUtc).CurrentValue = null;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.Equal(RoadmapStatus.Draft, (await db.Roadmaps.SingleAsync(x => x.Id == draft.Id)).Status);
    }

    [PostgresFact]
    public async Task UniqueIndexes_RejectDuplicatesIncludingInactiveRecords()
    {
        await using var db = CreateContext();
        var (roadmap, source, target, user) = await SeedGraphAsync(db);
        object[] duplicates =
        [
            new RoadmapCategory("Duplicate", "frontend"),
            new Roadmap(roadmap.CategoryId, "Duplicate", "frontend", "Summary", RoadmapLevel.Beginner, user.Id),
            new RoadmapNode(roadmap.Id, "Duplicate", "html"),
            new RoadmapEdge(roadmap.Id, source.Id, target.Id)
        ];
        foreach (var duplicate in duplicates)
        {
            db.Add(duplicate);
            var error = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(error.InnerException).SqlState);
            db.ChangeTracker.Clear();
        }

        var inactive = await db.RoadmapNodes.SingleAsync(x => x.Id == source.Id);
        inactive.SetActive(false);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        db.Add(new RoadmapNode(roadmap.Id, "Duplicate inactive", "html"));
        var inactiveError = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(inactiveError.InnerException).SqlState);
        db.ChangeTracker.Clear();

        var other = new Roadmap(roadmap.CategoryId, "Other", "other", "Summary", RoadmapLevel.Beginner, user.Id);
        db.AddRange(other, new RoadmapNode(other.Id, "HTML", "html"));
        await db.SaveChangesAsync(); // Node slugs are unique within a roadmap only.
    }

    [PostgresFact]
    public async Task Constraints_RejectCrossRoadmapEdgesSelfLoopsAndInvalidResourceMetadata()
    {
        await using var db = CreateContext();
        var (roadmap, source, target, user) = await SeedGraphAsync(db);
        var other = new Roadmap(roadmap.CategoryId, "Other", "other", "Summary", RoadmapLevel.Beginner, user.Id);
        var foreignNode = new RoadmapNode(other.Id, "Other node", "other-node");
        db.AddRange(other, foreignNode);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        db.Add(new RoadmapEdge(roadmap.Id, source.Id, foreignNode.Id));
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, Assert.IsType<PostgresException>(error.InnerException).SqlState);
        db.ChangeTracker.Clear();

        await AssertCheckViolation(() => db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE gdsc."RoadmapEdges" SET "TargetNodeId" = {source.Id} WHERE "SourceNodeId" = {source.Id}
            """));
        await AssertCheckViolation(() => db.Database.ExecuteSqlRawAsync("""
            UPDATE gdsc."LearningResources" SET "ExternalUrl" = 'https://example.com' WHERE "ResourceType" = 1
            """));
        await AssertCheckViolation(() => db.Database.ExecuteSqlRawAsync("""
            UPDATE gdsc."LearningResources" SET "ContentType" = NULL WHERE "ResourceType" = 1
            """));
        await AssertCheckViolation(() => db.Database.ExecuteSqlRawAsync("""
            UPDATE gdsc."LearningResources" SET "StoredFileName" = 'bad.pdf' WHERE "ResourceType" = 0
            """));
        await AssertCheckViolation(() => db.Database.ExecuteSqlRawAsync("""
            UPDATE gdsc."RoadmapNodes" SET "Width" = -1
            """));
        await AssertCheckViolation(() => db.Database.ExecuteSqlRawAsync("""
            UPDATE gdsc."Roadmaps" SET "SortOrder" = -1
            """));
    }

    [PostgresFact]
    public async Task Restrict_PreventsHardDeletingRelatedContentOrAuthor()
    {
        await using var db = CreateContext();
        await SeedGraphAsync(db);
        string[] deletes =
        [
            "DELETE FROM gdsc.\"RoadmapCategories\"",
            "DELETE FROM gdsc.\"Roadmaps\"",
            "DELETE FROM gdsc.\"RoadmapNodes\"",
            "DELETE FROM gdsc.\"Users\""
        ];
        foreach (var sql in deletes)
        {
            var error = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlRawAsync(sql));
            Assert.Contains(error.SqlState, new[] { PostgresErrorCodes.ForeignKeyViolation, PostgresErrorCodes.RestrictViolation });
        }
        Assert.Equal(2, await db.LearningResources.CountAsync());
        Assert.Single(await db.RoadmapEdges.ToListAsync());
    }

    private static async Task AssertCheckViolation(Func<Task<int>> action)
    {
        var error = await Assert.ThrowsAsync<PostgresException>(action);
        Assert.Equal(PostgresErrorCodes.CheckViolation, error.SqlState);
    }

    private sealed class PostgresFactAttribute : FactAttribute
    {
        public PostgresFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConnectionVariable)))
                Skip = $"Set {ConnectionVariable} to a disposable PostgreSQL server to run database tests.";
        }
    }

    private sealed class PostgresTheoryAttribute : TheoryAttribute
    {
        public PostgresTheoryAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConnectionVariable)))
                Skip = $"Set {ConnectionVariable} to a disposable PostgreSQL server to run database tests.";
        }
    }
}

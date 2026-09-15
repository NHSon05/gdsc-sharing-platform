using System.Data.Common;
using GdscSharingPlatform.Application;
using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Sharing;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Sharing;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Persistence;
using GdscSharingPlatform.Infrastructure.Services.Sharing;
using GdscSharingPlatform.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace GdscSharingPlatform.IntegrationTests.Sharing;

public sealed class SharingPostgresTests : IAsyncLifetime
{
    private readonly PostgresTestDatabase? _postgres = PostgresTestDatabase.FromEnvironment();
    private readonly Guid _uid = Guid.NewGuid();
    public async Task InitializeAsync()
    {
        if (_postgres is null) return;
        await _postgres.CreateAsync();
        await using var db = Context();
        await db.Database.MigrateAsync();
        db.Add(new ApplicationUser { Id = _uid, FullName = "Sharing admin" });
        await db.SaveChangesAsync();
    }
    public async Task DisposeAsync() { if (_postgres is not null) await _postgres.DropAsync(); }
    private ApplicationDbContext Context() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(_postgres!.ConnectionString).Options);
    private ServiceProvider Services(IInterceptor? interceptor = null, IFileStorage? storage = null)
    {
        var services = new ServiceCollection();
        services.AddLogging(); services.AddApplication(); services.AddHttpContextAccessor();
        services.AddSingleton<ICurrentUserService>(new Admin(_uid));
        services.AddDbContext<ApplicationDbContext>(o =>
        { o.UseNpgsql(_postgres!.ConnectionString); if (interceptor is not null) o.AddInterceptors(interceptor); });
        services.AddScoped<SharingOperations>(); services.AddScoped<SharingScheduleService>();
        services.AddScoped<SharingContentService>(); services.AddScoped<SharingResourceService>();
        services.AddSingleton<IFileStorage>(storage ?? new MemoryStorage());
        return services.BuildServiceProvider();
    }
    [PostgresFact]
    public async Task ConcurrentPublishOfOverlappingDrafts_OnlyOneCanCommit()
    {
        await using var db = Context();
        var start = new DateTimeOffset(2026, 10, 1, 7, 0, 0, TimeSpan.Zero);
        SharingSchedule NewSchedule()
        {
            var s = new SharingSchedule("Talk", SharingType.TechTalk, DeliveryMode.Online, start, start.AddHours(1), "UTC", AudienceScope.AllMembers, _uid, meetingUrl: "https://meet.example.com");
            s.Presenters.Add(new(s.Id, _uid, PresenterRole.Speaker, 0)); return s;
        }
        var first = NewSchedule(); var second = NewSchedule(); db.AddRange(first, second); await db.SaveChangesAsync();
        await using var services = Services(new OverlapBarrier());
        async Task<Exception?> Publish(Guid id)
        {
            await using var scope = services.CreateAsyncScope();
            return await Record.ExceptionAsync(() => scope.ServiceProvider.GetRequiredService<SharingScheduleService>()
                .TransitionAsync(id, ScheduleAction.Publish, 0, null, default));
        }
        var results = await Task.WhenAll(Publish(first.Id), Publish(second.Id));
        Assert.Single(results, x => x is null);
        Assert.IsType<ConflictException>(Assert.Single(results, x => x is not null));
        Assert.Equal(1, await db.SharingSchedules.CountAsync(x => x.Status == SharingScheduleStatus.Scheduled));
        Assert.Equal(1, await db.Set<SharingAuditEntry>().CountAsync(x => x.Action == "Publish"));
    }
    [PostgresFact]
    public async Task ConcurrentContentEdits_Return412AndPreserveOneVersion()
    {
        await using var db = Context();
        var content = new SharingContent("Title", "title", "Summary", "Body", _uid); db.Add(content); await db.SaveChangesAsync();
        await using var services = Services(new ContentReadBarrier());
        async Task<Exception?> Update(string title)
        {
            await using var scope = services.CreateAsyncScope();
            return await Record.ExceptionAsync(() => scope.ServiceProvider.GetRequiredService<SharingContentService>()
                .UpdateAsync(content.Id, new(title, "title", "Summary", "Body", [], []), 0, default));
        }
        var results = await Task.WhenAll(Update("First"), Update("Second"));
        Assert.Single(results, x => x is null);
        Assert.IsType<PreconditionFailedException>(Assert.Single(results, x => x is not null));
        Assert.Equal(1, (await db.SharingContents.AsNoTracking().SingleAsync()).Version);
        Assert.Equal(1, await db.Set<SharingAuditEntry>().CountAsync(x => x.Action == "Update"));
    }
    [PostgresFact]
    public async Task ConcurrentScheduleEdits_Return412AndCommitOneAudit()
    {
        await using var db = Context();
        var start = new DateTimeOffset(2026, 10, 1, 7, 0, 0, TimeSpan.Zero);
        var schedule = new SharingSchedule("Talk", SharingType.TechTalk, DeliveryMode.Online, start,
            start.AddHours(1), "UTC", AudienceScope.AllMembers, _uid, meetingUrl: "https://meet.example.com");
        schedule.Presenters.Add(new(schedule.Id, _uid, PresenterRole.Speaker, 0));
        db.Add(schedule); await db.SaveChangesAsync();
        await using var services = Services(new OverlapBarrier());
        async Task<Exception?> Update(string title)
        {
            await using var scope = services.CreateAsyncScope();
            var request = new ScheduleRequest(title, SharingType.TechTalk, DeliveryMode.Online,
                DateTime.SpecifyKind(start.UtcDateTime, DateTimeKind.Unspecified),
                DateTime.SpecifyKind(start.AddHours(1).UtcDateTime, DateTimeKind.Unspecified),
                "UTC", AudienceScope.AllMembers, [new(_uid, PresenterRole.Speaker)], [], [], [],
                MeetingUrl: "https://meet.example.com");
            return await Record.ExceptionAsync(() => scope.ServiceProvider.GetRequiredService<SharingScheduleService>()
                .UpdateAsync(schedule.Id, request, 0, default));
        }
        var results = await Task.WhenAll(Update("First"), Update("Second"));
        Assert.Single(results, x => x is null);
        Assert.IsType<PreconditionFailedException>(Assert.Single(results, x => x is not null));
        Assert.Equal(1, (await db.SharingSchedules.AsNoTracking().SingleAsync()).Version);
        Assert.Equal(1, await db.Set<SharingAuditEntry>().CountAsync(x => x.Action == "Update"));
    }

    [PostgresFact]
    public async Task SharingMigrationsPreserveExistingSprint3Graph_AndCanRunTwice()
    {
        var database = PostgresTestDatabase.FromEnvironment()!;
        await database.CreateAsync();
        try
        {
            await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(database.ConnectionString).Options);
            await db.GetService<IMigrator>().MigrateAsync("20260909190532_AddRoadmapManagement");
            var user = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Existing member" };
            var category = new GdscSharingPlatform.Domain.Roadmaps.RoadmapCategory("Backend", "backend");
            var roadmap = new GdscSharingPlatform.Domain.Roadmaps.Roadmap(category.Id, "Roadmap", "roadmap", "Summary", RoadmapLevel.Beginner, user.Id);
            var first = new GdscSharingPlatform.Domain.Roadmaps.RoadmapNode(roadmap.Id, "First", "first", positionX: 123m);
            var second = new GdscSharingPlatform.Domain.Roadmaps.RoadmapNode(roadmap.Id, "Second", "second");
            var edge = new GdscSharingPlatform.Domain.Roadmaps.RoadmapEdge(roadmap.Id, first.Id, second.Id);
            db.AddRange(user, category, roadmap, first, second, edge);
            await db.SaveChangesAsync();
            await db.Database.MigrateAsync();
            await db.Database.MigrateAsync();
            db.ChangeTracker.Clear();
            Assert.Equal(user.Id, (await db.Users.SingleAsync()).Id);
            Assert.Equal(roadmap.Id, (await db.Roadmaps.SingleAsync()).Id);
            Assert.Equal(2, await db.RoadmapNodes.CountAsync());
            Assert.Equal(123m, (await db.RoadmapNodes.SingleAsync(x => x.Id == first.Id)).PositionX);
            Assert.Equal(edge.Id, (await db.RoadmapEdges.SingleAsync()).Id);
            Assert.Empty(await db.SharingContents.ToListAsync());
            Assert.Empty(await db.SharingSchedules.ToListAsync());
            Assert.Empty(await db.Set<SharingAuditEntry>().ToListAsync());
            Assert.Empty(await db.Database.GetPendingMigrationsAsync());
            Assert.False(db.Database.HasPendingModelChanges());
        }
        finally { await database.DropAsync(); }
    }

    [PostgresFact]
    public async Task DatabaseRejectsSecondOwner()
    {
        await using var db = Context();
        var content = new SharingContent("Title", "title", "Summary", "Body", _uid);
        var other = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Other" };
        db.AddRange(content, other); await db.SaveChangesAsync();
        db.Add(new SharingContentAuthor(content.Id, other.Id, SharingAuthorRole.Owner, 1));
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(Npgsql.PostgresErrorCodes.UniqueViolation, Assert.IsType<Npgsql.PostgresException>(error.InnerException).SqlState);
        Assert.Equal(1, await db.Set<SharingContentAuthor>().AsNoTracking().CountAsync(x => x.SharingContentId == content.Id));
    }

    [PostgresFact]
    public async Task FailedFileWrites_RollBackMetadataVersionAndAudit_AndCleanNewBytes()
    {
        await using var db = Context();
        var content = new SharingContent("Title", "title", "Summary", "Body", _uid);
        var resource = new SharingResource(content.Id, "Old", ResourceType.File, 0, _uid);
        resource.SetFile("old.pdf", "old.pdf", 1, "application/pdf");
        db.Add(content); db.Add(resource); await db.SaveChangesAsync();
        await db.Database.ExecuteSqlRawAsync("""
            CREATE FUNCTION gdsc.reject_sharing_file() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN RAISE EXCEPTION 'Injected file metadata failure'; END $$;
            CREATE TRIGGER reject_sharing_file BEFORE INSERT OR UPDATE ON gdsc."SharingResources"
                FOR EACH ROW EXECUTE FUNCTION gdsc.reject_sharing_file();
            """);
        var storage = new MemoryStorage(); storage.Keys.Add("old.pdf");
        await using var services = Services(storage: storage);
        foreach (var replace in new[] { false, true })
        {
            await using var scope = services.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<SharingResourceService>();
            var file = new FileUpload(new MemoryStream([1]), "new.pdf", "application/pdf", 1);
            await Assert.ThrowsAsync<DbUpdateException>(() => replace ? service.ReplaceAsync(resource.Id, file, 0, default)
                : service.CreateAsync(content.Id, new("New"), file, 0, default));
            Assert.Equal("old.pdf", Assert.Single(storage.Keys));
            Assert.Equal(0, (await db.SharingContents.AsNoTracking().SingleAsync()).Version);
            Assert.Equal("old.pdf", (await db.SharingResources.AsNoTracking().SingleAsync()).StorageKey);
            Assert.Empty(await db.Set<SharingAuditEntry>().ToListAsync());
        }
    }
    private sealed class PostgresFactAttribute : FactAttribute
    { public PostgresFactAttribute() { if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ROADMAP_TEST_POSTGRES"))) Skip = "Requires a disposable PostgreSQL server via ROADMAP_TEST_POSTGRES."; } }
    private sealed class Admin(Guid id) : ICurrentUserService
    {
        public Guid? UserId => id; public string? Email => null;
        public IReadOnlyCollection<string> Roles => [RoleNames.Admin]; public bool IsAuthenticated => true;
    }
    private sealed class MemoryStorage : IFileStorage
    {
        public HashSet<string> Keys { get; } = [];
        public Task<StoredFile> SaveAsync(FileUpload file, CancellationToken ct = default)
        { var key = Guid.NewGuid().ToString(); Keys.Add(key); return Task.FromResult(new StoredFile("new.pdf", key, key, 1, "application/pdf")); }
        public Task<Stream> OpenReadAsync(string key, CancellationToken ct = default) => Task.FromResult<Stream>(new MemoryStream([1]));
        public Task DeleteAsync(string key, CancellationToken ct = default) { Keys.Remove(key); return Task.CompletedTask; }
    }
    private abstract class ReadBarrier : DbCommandInterceptor
    {
        private int _readers;
        private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
        protected abstract bool Matches(string sql);
        public override async ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData,
            DbDataReader result, CancellationToken ct = default)
        {
            if (Matches(command.CommandText))
            {
                var n = Interlocked.Increment(ref _readers);
                if (n == 2) _ready.TrySetResult();
                if (n <= 2) await _ready.Task.WaitAsync(TimeSpan.FromSeconds(15), ct);
            }
            return result;
        }
    }
    private sealed class OverlapBarrier : ReadBarrier
    { protected override bool Matches(string sql) => sql.Contains("FROM gdsc.\"SharingSchedules\"") && sql.Contains("\"EndsAtUtc\" >") && sql.Contains("LIMIT 1"); }
    private sealed class ContentReadBarrier : ReadBarrier
    { protected override bool Matches(string sql) => sql.Contains("FROM gdsc.\"SharingContents\"") && sql.Contains("\"SharingContentAuthors\"") && sql.Contains("\"BodyMarkdown\""); }
}

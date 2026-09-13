using System.Data.Common;
using GdscSharingPlatform.Application;
using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;
using GdscSharingPlatform.Infrastructure.Persistence;
using GdscSharingPlatform.Infrastructure.Services.Roadmaps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace GdscSharingPlatform.IntegrationTests.Persistence;

public sealed partial class RoadmapPersistenceIntegrationTests
{
    private ServiceProvider Services(Guid author, IInterceptor? interceptor = null, IFileStorage? storage = null)
    {
        var services = new ServiceCollection();
        services.AddLogging(); services.AddApplication();
        services.AddSingleton<ICurrentUserService>(new AdminUser(author));
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(_connectionString);
            if (interceptor is not null) options.AddInterceptors(interceptor);
        });
        services.AddSingleton<IFileStorage>(storage ?? new FakeStorage());
        services.AddScoped<RoadmapOperations>(); services.AddScoped<RoadmapEdgeService>();
        services.AddScoped<RoadmapNodeService>(); services.AddScoped<LearningResourceService>();
        return services.BuildServiceProvider();
    }

    [PostgresFact]
    public async Task ConcurrentRequiredEdges_CannotCommitACycle()
    {
        await using var db = CreateContext();
        var (roadmap, source, target, user) = await SeedGraphAsync(db);
        await db.RoadmapEdges.ExecuteDeleteAsync();
        var barrier = new EdgeReadBarrier();
        await using var services = Services(user.Id, barrier);
        async Task<Exception?> AddAsync(Guid from, Guid to)
        {
            await using var scope = services.CreateAsyncScope();
            return await Record.ExceptionAsync(() => scope.ServiceProvider.GetRequiredService<RoadmapEdgeService>()
                .CreateAsync(roadmap.Id, new EdgeRequest(from, to)));
        }
        var results = await Task.WhenAll(AddAsync(source.Id, target.Id), AddAsync(target.Id, source.Id));
        Assert.Single(results, x => x is null);
        var failure = Assert.Single(results, x => x is not null);
        Assert.True(failure is ConflictException, failure?.ToString());
        Assert.Single(await db.RoadmapEdges.ToListAsync());
    }

    [PostgresFact]
    public async Task DatabaseUniqueViolation_IsTranslatedToConflict()
    {
        await using var db = CreateContext();
        var (roadmap, _, _, user) = await SeedGraphAsync(db);
        await using var services = Services(user.Id);
        await using var scope = services.CreateAsyncScope();
        var op = scope.ServiceProvider.GetRequiredService<RoadmapOperations>();
        await Assert.ThrowsAsync<ConflictException>(() => op.WriteAsync(() =>
        {
            op.Db.Add(new Roadmap(roadmap.CategoryId, "Duplicate", roadmap.Slug, "Summary", RoadmapLevel.Beginner, user.Id));
            return Task.FromResult(true);
        }, CancellationToken.None));
        Assert.Single(await db.Roadmaps.ToListAsync());
    }

    [PostgresFact]
    public async Task FailedFileInsertAndReplacement_CleanNewFilesAndKeepOriginal()
    {
        await using var db = CreateContext();
        var (_, source, _, user) = await SeedGraphAsync(db);
        var original = await db.LearningResources.SingleAsync(x => x.ResourceType == ResourceType.File);
        var storage = new FakeStorage();
        storage.Keys.Add(original.StorageKey!);
        await using var services = Services(user.Id, new FailResourceSave(), storage);
        await using (var scope = services.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<LearningResourceService>();
            await Assert.ThrowsAsync<DbUpdateException>(() => service.CreateFileAsync(source.Id, new("PDF"),
                new(new MemoryStream([1]), "guide.pdf", "application/pdf", 1)));
        }
        Assert.Single(storage.Keys); Assert.Contains(original.StorageKey!, storage.Keys);
        await using (var scope = services.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<LearningResourceService>();
            await Assert.ThrowsAsync<DbUpdateException>(() => service.ReplaceFileAsync(original.Id,
                new(new MemoryStream([1]), "guide.pdf", "application/pdf", 1)));
        }
        Assert.Single(storage.Keys); Assert.Contains(original.StorageKey!, storage.Keys);
        db.ChangeTracker.Clear();
        Assert.Equal(original.StorageKey, (await db.LearningResources.SingleAsync(x => x.Id == original.Id)).StorageKey);
        Assert.Equal(2, await db.LearningResources.CountAsync());
    }

    [PostgresFact]
    public async Task FailedBatchPositionWrite_RollsBackAllNodeChanges()
    {
        await using var db = CreateContext();
        var (roadmap, source, target, user) = await SeedGraphAsync(db);
        // Force a PostgreSQL failure on the second update in the same SaveChanges transaction.
        await db.Database.ExecuteSqlRawAsync("""
            CREATE FUNCTION gdsc.reject_test_position() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN IF NEW."PositionX" = 777 THEN RAISE EXCEPTION 'Injected second-node failure'; END IF; RETURN NEW; END $$;
            CREATE TRIGGER reject_test_position BEFORE UPDATE ON gdsc."RoadmapNodes"
                FOR EACH ROW EXECUTE FUNCTION gdsc.reject_test_position();
            """);
        await using var services = Services(user.Id);
        await using var scope = services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<RoadmapNodeService>();
        await Assert.ThrowsAsync<DbUpdateException>(() => service.SavePositionsAsync(roadmap.Id,
            new([new(source.Id, 666, 100), new(target.Id, 777, 100)])));
        db.ChangeTracker.Clear();
        Assert.Equal(source.PositionX, (await db.RoadmapNodes.SingleAsync(x => x.Id == source.Id)).PositionX);
        Assert.Equal(target.PositionX, (await db.RoadmapNodes.SingleAsync(x => x.Id == target.Id)).PositionX);
    }

    private sealed class AdminUser(Guid id) : ICurrentUserService
    {
        public Guid? UserId => id;
        public string? Email => "admin@test.local";
        public IReadOnlyCollection<string> Roles => [RoleNames.Admin];
        public bool IsAuthenticated => true;
    }
    private sealed class FakeStorage : IFileStorage
    {
        public HashSet<string> Keys { get; } = [];
        public Task<StoredFile> SaveAsync(FileUpload upload, CancellationToken cancellationToken = default)
        {
            var key = $"{Guid.NewGuid():N}.pdf"; Keys.Add(key);
            return Task.FromResult(new StoredFile("guide.pdf", key, key, upload.Length, "application/pdf"));
        }
        public Task DeleteAsync(string key, CancellationToken cancellationToken = default) { Keys.Remove(key); return Task.CompletedTask; }
        public Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult<Stream>(new MemoryStream([1]));
    }
    private sealed class FailResourceSave : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<LearningResource>().Any(x => x.State is EntityState.Added or EntityState.Modified))
                throw new DbUpdateException("Injected database save failure.");
            return ValueTask.FromResult(result);
        }
    }
    private sealed class EdgeReadBarrier : DbCommandInterceptor
    {
        private int _readers;
        private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData,
            DbDataReader result, CancellationToken cancellationToken = default)
        {
            if (command.CommandText.Contains("FROM gdsc.\"RoadmapEdges\"", StringComparison.Ordinal))
            {
                if (Interlocked.Increment(ref _readers) == 2) _ready.TrySetResult();
                await _ready.Task.WaitAsync(TimeSpan.FromSeconds(15), cancellationToken);
            }
            return result;
        }
    }
}

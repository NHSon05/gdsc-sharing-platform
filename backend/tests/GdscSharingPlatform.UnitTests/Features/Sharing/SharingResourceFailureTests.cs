using GdscSharingPlatform.Application;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Sharing;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Sharing;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Persistence;
using GdscSharingPlatform.Infrastructure.Services.Sharing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace GdscSharingPlatform.UnitTests.Features.Sharing;

public sealed class SharingResourceFailureTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DatabaseFailureCleansNewFileAndPreservesOriginalAndVersion(bool replace)
    {
        var uid = Guid.NewGuid();
        await using var db = new FailingContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var content = new SharingContent("Title", "title", "Summary", "Body", uid);
        var resource = new SharingResource(content.Id, "Guide", ResourceType.File, 0, uid);
        resource.SetFile("old.pdf", "old.pdf", 1, "application/pdf");
        db.Add(new ApplicationUser { Id = uid, FullName = "Owner" }); db.Add(content); db.Add(resource);
        await db.SaveChangesAsync(); db.ChangeTracker.Clear(); db.Fail = true;
        using var provider = new ServiceCollection().AddApplication().BuildServiceProvider();
        var op = new SharingOperations(db, new Member(uid), provider, new HttpContextAccessor(), NullLogger<SharingOperations>.Instance);
        var storage = new MemoryStorage(); storage.Keys.Add("old.pdf");
        var service = new SharingResourceService(op, storage, NullLogger<SharingResourceService>.Instance);
        var upload = new FileUpload(new MemoryStream([1]), "new.pdf", "application/pdf", 1);
        await Assert.ThrowsAsync<DbUpdateException>(() => replace
            ? service.ReplaceAsync(resource.Id, upload, 0, default)
            : service.CreateAsync(content.Id, new ResourceRequest("New"), upload, 0, default));
        Assert.Equal("old.pdf", Assert.Single(storage.Keys));
        Assert.Equal(0, (await db.SharingContents.SingleAsync()).Version);
        Assert.Equal("old.pdf", (await db.SharingResources.SingleAsync()).StorageKey);
        Assert.Empty(await db.Set<SharingAuditEntry>().ToListAsync());
    }
    private sealed class FailingContext(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
    {
        public bool Fail { get; set; }
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) =>
            Fail ? throw new DbUpdateException("Injected failure") : base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    private sealed class Member(Guid id) : ICurrentUserService
    {
        public Guid? UserId => id;
        public string? Email => null;
        public IReadOnlyCollection<string> Roles => [RoleNames.Member];
        public bool IsAuthenticated => true;
    }
    private sealed class MemoryStorage : IFileStorage
    {
        public HashSet<string> Keys { get; } = [];
        public Task<StoredFile> SaveAsync(FileUpload file, CancellationToken ct = default)
        { var key = Guid.NewGuid().ToString(); Keys.Add(key); return Task.FromResult(new StoredFile("new.pdf", key, key, 1, "application/pdf")); }
        public Task<Stream> OpenReadAsync(string key, CancellationToken ct = default) => Task.FromResult<Stream>(new MemoryStream([1]));
        public Task DeleteAsync(string key, CancellationToken ct = default) { Keys.Remove(key); return Task.CompletedTask; }
    }
}

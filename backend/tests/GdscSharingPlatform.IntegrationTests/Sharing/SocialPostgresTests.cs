using System.Net;
using GdscSharingPlatform.Domain.Sharing;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Services.Sharing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.IntegrationTests.Sharing;

public sealed partial class SharingPostgresTests
{
    private async Task<Guid> SeedSocialContent()
    {
        await using var db = Context();
        var author = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Author" };
        var content = new SharingContent("Title", "social", "Summary", "Body", author.Id);
        content.Submit(author.Id); content.Approve(_uid);
        db.AddRange(author, content); await db.SaveChangesAsync();
        return content.Id;
    }
    [PostgresFact]
    public async Task SocialConcurrentLikesAreIdempotentAndDoNotDuplicateNotifications()
    {
        var id = await SeedSocialContent();
        await using var services = Services(new ContentReadBarrier());
        async Task Like()
        {
            await using var scope = services.CreateAsyncScope();
            var result = await scope.ServiceProvider.GetRequiredService<ContentInteractionService>().LikeAsync(id, default);
            Assert.Equal(1, result.LikeCount);
        }
        await Task.WhenAll(Like(), Like());
        await using var db = Context();
        Assert.Equal(1, await db.ContentLikes.CountAsync());
        Assert.Equal(1, await db.Notifications.CountAsync());
        Assert.Equal(2, await db.OutboxMessages.CountAsync());
    }
    [PostgresFact]
    public async Task SocialOutboxFailureRollsBackLikeAndNotification()
    {
        var id = await SeedSocialContent();
        await using var db = Context();
        await db.Database.ExecuteSqlRawAsync("""
            CREATE FUNCTION gdsc.reject_social_outbox() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN RAISE EXCEPTION 'Injected outbox failure'; END $$;
            CREATE TRIGGER reject_social_outbox BEFORE INSERT ON gdsc."OutboxMessages"
              FOR EACH ROW EXECUTE FUNCTION gdsc.reject_social_outbox();
            """);
        await using var services = Services();
        await using var scope = services.CreateAsyncScope();
        await Assert.ThrowsAsync<DbUpdateException>(() => scope.ServiceProvider.GetRequiredService<ContentInteractionService>().LikeAsync(id, default));
        Assert.Empty(await db.ContentLikes.ToListAsync());
        Assert.Empty(await db.Notifications.ToListAsync());
        Assert.Empty(await db.OutboxMessages.ToListAsync());
    }
    [PostgresFact]
    public async Task SocialWorkerSkipsRowsClaimedByAnotherWorker()
    {
        await using var db = Context();
        db.OutboxMessages.Add(new(Guid.NewGuid(), "notification.created", "{}", DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();
        var handler = new BlockingGateway();
        var config = Options.Create(new SocialOutboxOptions { BatchSize = 1, GatewayUrl = "https://gateway.test/events", ServiceToken = "test" });
        await using var firstDb = Context(); await using var secondDb = Context();
        var first = new OutboxDispatcher(firstDb, handler, config, TimeProvider.System).DispatchAsync(default);
        await handler.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        try { Assert.Equal(0, await new OutboxDispatcher(secondDb, handler, config, TimeProvider.System).DispatchAsync(default)); }
        finally { handler.Release.TrySetResult(); }
        Assert.Equal(1, await first);
        Assert.Equal(1, handler.Calls);
        Assert.NotNull((await db.OutboxMessages.AsNoTracking().SingleAsync()).ProcessedAtUtc);
    }
    private sealed class BlockingGateway : HttpMessageHandler, IHttpClientFactory
    {
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int Calls;
        public HttpClient CreateClient(string name) => new(this, false);
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        { Interlocked.Increment(ref Calls); Entered.TrySetResult(); await Release.Task.WaitAsync(cancellationToken); return new(HttpStatusCode.NoContent); }
    }
}

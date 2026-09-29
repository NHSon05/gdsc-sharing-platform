using GdscSharingPlatform.Application.Features.Sharing;
using GdscSharingPlatform.Domain.Sharing;
using GdscSharingPlatform.Infrastructure.Persistence;
using GdscSharingPlatform.Infrastructure.Services.Sharing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.UnitTests.Features.Sharing;

public sealed class OutboxDispatcherTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeliveryPersistsSuccessOrRetriesWithBackoffAndDeadLetter(bool success)
    {
        using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var clock = new TestClock();
        var message = new OutboxMessage(Guid.NewGuid(), "notification.created", "{}", clock.GetUtcNow());
        db.Add(message); await db.SaveChangesAsync();
        var handler = new Handler(success);
        var dispatcher = new OutboxDispatcher(db, handler, Options.Create(new SocialOutboxOptions
        { MaxAttempts = 2, BaseDelaySeconds = 2 }), clock);
        Assert.Equal(1, await dispatcher.DispatchAsync(default));
        db.ChangeTracker.Clear();
        var result = await db.OutboxMessages.SingleAsync();
        Assert.Equal(message.Id.ToString(), Assert.Single(handler.Keys));
        if (success)
        {
            Assert.NotNull(result.ProcessedAtUtc); Assert.Null(result.LastError);
            Assert.Equal(0, await dispatcher.DispatchAsync(default));
        }
        else
        {
            Assert.Null(result.ProcessedAtUtc); Assert.Equal(1, result.RetryCount);
            Assert.Equal(clock.GetUtcNow().AddSeconds(2), result.NextAttemptAtUtc);
            Assert.Equal(0, await dispatcher.DispatchAsync(default));
            clock.Advance(TimeSpan.FromSeconds(2));
            Assert.Equal(1, await dispatcher.DispatchAsync(default));
            db.ChangeTracker.Clear(); result = await db.OutboxMessages.SingleAsync();
            Assert.NotNull(result.DeadLetteredAtUtc); Assert.Equal(2, result.RetryCount);
            Assert.Equal(0, await dispatcher.DispatchAsync(default));
            Assert.All(handler.Keys, key => Assert.Equal(message.Id.ToString(), key));
        }
    }
    private sealed class TestClock : TimeProvider
    {
        private DateTimeOffset _now = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan duration) => _now += duration;
    }
    private sealed class Handler(bool success) : IRealtimeEventPublisher
    {
        public List<string> Keys { get; } = [];
        public Task PublishAsync(Guid eventId, string eventName, string payloadJson, CancellationToken ct)
        {
            Keys.Add(eventId.ToString());
            return success ? Task.CompletedTask : Task.FromException(new IOException("Publish failed"));
        }
    }
}

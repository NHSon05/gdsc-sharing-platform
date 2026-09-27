using System.Net;
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
        var dispatcher = new OutboxDispatcher(db, new Clients(handler), Options.Create(new SocialOutboxOptions
        { GatewayUrl = "https://gateway.test/internal/events", ServiceToken = "test-token", MaxAttempts = 2, BaseDelaySeconds = 2 }), clock);
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
    private sealed class Clients(Handler handler) : IHttpClientFactory
    { public HttpClient CreateClient(string name) => new(handler, false); }
    private sealed class Handler(bool success) : HttpMessageHandler
    {
        public List<string> Keys { get; } = [];
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Keys.Add(request.Headers.GetValues("Idempotency-Key").Single());
            return Task.FromResult(new HttpResponseMessage(success ? HttpStatusCode.NoContent : HttpStatusCode.ServiceUnavailable));
        }
    }
}

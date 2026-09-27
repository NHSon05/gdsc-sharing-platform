using System.Diagnostics.Metrics;
using System.Net.Http.Headers;
using System.Text;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.Infrastructure.Services.Sharing;

public sealed class SocialOutboxOptions
{
    public bool Enabled { get; set; }
    public string GatewayUrl { get; set; } = "";
    public string ServiceToken { get; set; } = "";
    public int PollSeconds { get; set; } = 2;
    public int BatchSize { get; set; } = 20;
    public int MaxAttempts { get; set; } = 8;
    public int BaseDelaySeconds { get; set; } = 2;
    public int MaxDelaySeconds { get; set; } = 300;
    public int TimeoutSeconds { get; set; } = 10;
}

public sealed class OutboxDispatcher(ApplicationDbContext db, IHttpClientFactory clients, IOptions<SocialOutboxOptions> options, TimeProvider clock)
{
    private static readonly Meter Meter = new("Gdsc.Social.Outbox", "1.0");
    private static readonly Counter<long> Delivered = Meter.CreateCounter<long>("outbox.delivered");
    private static readonly Counter<long> Failed = Meter.CreateCounter<long>("outbox.failed");
    private static readonly Histogram<double> Latency = Meter.CreateHistogram<double>("outbox.commit_to_delivery", "s");
    private static readonly Histogram<long> Pending = Meter.CreateHistogram<long>("outbox.pending", "messages");
    private static readonly Histogram<long> DeadLetters = Meter.CreateHistogram<long>("outbox.dead_letter", "messages");

    public async Task<int> DispatchAsync(CancellationToken ct)
    {
        var config = options.Value;
        var handled = 0;
        // One row per transaction bounds lock duration to one HTTP timeout. Other workers skip locked rows.
        for (; handled < config.BatchSize; handled++)
        {
            var now = clock.GetUtcNow();
            await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
            var query = db.OutboxMessages.AsQueryable();
            if (db.Database.IsNpgsql()) query = db.OutboxMessages.FromSqlInterpolated($"""
                SELECT * FROM gdsc."OutboxMessages"
                WHERE "ProcessedAtUtc" IS NULL AND "DeadLetteredAtUtc" IS NULL
                  AND ("NextAttemptAtUtc" IS NULL OR "NextAttemptAtUtc" <= {now})
                ORDER BY "OccurredAtUtc", "Id" LIMIT 1 FOR UPDATE SKIP LOCKED
                """);
            else query = query.Where(x => x.ProcessedAtUtc == null && x.DeadLetteredAtUtc == null
                && (x.NextAttemptAtUtc == null || x.NextAttemptAtUtc <= now)).OrderBy(x => x.OccurredAtUtc).Take(1);
            var message = (await query.ToListAsync(ct)).FirstOrDefault();
            if (message is null) break;
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, config.GatewayUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ServiceToken);
                request.Headers.Add("Idempotency-Key", message.Id.ToString());
                request.Content = new StringContent(message.PayloadJson, Encoding.UTF8, "application/json");
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeout.CancelAfter(TimeSpan.FromSeconds(config.TimeoutSeconds));
                using var response = await clients.CreateClient("social-outbox").SendAsync(request, timeout.Token);
                response.EnsureSuccessStatusCode();
                message.MarkProcessed();
                Delivered.Add(1);
                Latency.Record((clock.GetUtcNow() - message.OccurredAtUtc).TotalSeconds);
            }
            catch (Exception e) when (!ct.IsCancellationRequested && e is HttpRequestException or OperationCanceledException)
            {
                // Never persist remote response bodies, tokens or URLs in error logs.
                var delay = Math.Min(config.MaxDelaySeconds, config.BaseDelaySeconds * Math.Pow(2, Math.Min(message.RetryCount, 20)));
                message.RecordFailure(clock.GetUtcNow().AddSeconds(delay), e is HttpRequestException http
                    ? $"Gateway request failed (HTTP {(int?)http.StatusCode})." : "Gateway request timed out.");
                if (message.RetryCount >= config.MaxAttempts) message.DeadLetter(clock.GetUtcNow());
                Failed.Add(1);
            }
            await db.SaveChangesAsync(ct);
            if (tx is not null) await tx.CommitAsync(ct);
            db.ChangeTracker.Clear();
        }
        Pending.Record(await db.OutboxMessages.LongCountAsync(x => x.ProcessedAtUtc == null && x.DeadLetteredAtUtc == null, ct));
        DeadLetters.Record(await db.OutboxMessages.LongCountAsync(x => x.DeadLetteredAtUtc != null, ct));
        return handled;
    }
}

public sealed class OutboxWorker(IServiceScopeFactory scopes, IOptions<SocialOutboxOptions> options, ILogger<OutboxWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled) return;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<OutboxDispatcher>().DispatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError("Outbox batch failed ({ErrorType}); retrying on next poll.", ex.GetType().Name); }
            try { await Task.Delay(TimeSpan.FromSeconds(options.Value.PollSeconds), stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }
}

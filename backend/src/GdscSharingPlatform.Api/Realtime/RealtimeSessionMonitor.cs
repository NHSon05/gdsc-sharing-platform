using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.Api.Realtime;

public sealed class RealtimeSessionMonitor(IServiceScopeFactory scopes, RealtimeConnections connections,
    ILogger<RealtimeSessionMonitor> logger, IOptions<RealtimeOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.Value.SessionRecheckSeconds));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                foreach (var (id, entry) in connections.Items)
                {
                    try
                    {
                        await using var scope = scopes.CreateAsyncScope();
                        var access = scope.ServiceProvider.GetRequiredService<RealtimeAccess>();
                        foreach (var group in entry.Groups.Keys)
                            if (!await access.CanReadAsync(entry.User, group, stoppingToken))
                            { connections.Items.TryRemove(id, out _); entry.Abort(); break; }
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { throw; }
                    catch (Exception)
                    {
                        connections.Items.TryRemove(id, out _); entry.Abort();
                        logger.LogWarning("Realtime session validation unavailable; connection closed.");
                    }
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        finally { foreach (var entry in connections.Items.Values) entry.Abort(); connections.Items.Clear(); }
    }
}

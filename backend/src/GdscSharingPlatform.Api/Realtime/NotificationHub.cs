using GdscSharingPlatform.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.Api.Realtime;

[Authorize(Policy = AuthPolicies.RequireActiveUser)]
public sealed class NotificationHub(RealtimeAccess access, RealtimeConnections connections, IOptions<RealtimeOptions> options) : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (!await access.CanReadAsync(Context.User!, null, Context.ConnectionAborted))
        { Context.Abort(); throw new HubException("AUTH_INVALID"); }
        var group = $"user:{RealtimeAccess.UserId(Context.User!)}";
        var entry = new RealtimeConnections.Entry(Context.User!, Context.Abort);
        entry.Groups.TryAdd(group, 0);
        connections.Items[Context.ConnectionId] = entry;
        try { await Groups.AddToGroupAsync(Context.ConnectionId, group, Context.ConnectionAborted); }
        catch { connections.Items.TryRemove(Context.ConnectionId, out _); throw; }
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    { connections.Items.TryRemove(Context.ConnectionId, out _); return base.OnDisconnectedAsync(exception); }

    public Task SubscribeContent(Guid contentId) => Change($"content:{contentId}", true);
    public Task UnsubscribeContent(Guid contentId) => Change($"content:{contentId}", false);
    public Task SubscribeSchedule(Guid scheduleId) => Change($"schedule:{scheduleId}:admins", true);
    public Task UnsubscribeSchedule(Guid scheduleId) => Change($"schedule:{scheduleId}:admins", false);

    private async Task Change(string group, bool join)
    {
        if (!connections.Items.TryGetValue(Context.ConnectionId, out var entry)) throw new HubException("AUTH_INVALID");
        // SignalR serializes invocations per connection (MaximumParallelInvocationsPerClient = 1).
        if (DateTimeOffset.UtcNow - entry.WindowStart >= TimeSpan.FromMinutes(1))
        { entry.WindowStart = DateTimeOffset.UtcNow; entry.Requests = 0; }
        if (++entry.Requests > options.Value.SubscribePerMinute) throw new HubException("RATE_LIMITED");
        if (!await access.CanReadAsync(Context.User!, null, Context.ConnectionAborted))
        { Context.Abort(); throw new HubException("AUTH_INVALID"); }
        if (!join)
        {
            entry.Groups.TryRemove(group, out _);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group, Context.ConnectionAborted);
            return;
        }
        if (entry.Groups.Count >= options.Value.MaxSubscriptions + 1 && !entry.Groups.ContainsKey(group)) throw new HubException("GROUP_LIMIT");
        if (!await access.CanReadAsync(Context.User!, group, Context.ConnectionAborted)) throw new HubException("FORBIDDEN");
        await Groups.AddToGroupAsync(Context.ConnectionId, group, Context.ConnectionAborted);
        entry.Groups.TryAdd(group, 0);
    }
}

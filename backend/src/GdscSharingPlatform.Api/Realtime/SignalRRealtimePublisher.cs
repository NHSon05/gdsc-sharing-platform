using GdscSharingPlatform.Application.Features.Sharing;
using Microsoft.AspNetCore.SignalR;

namespace GdscSharingPlatform.Api.Realtime;

public sealed class SignalRRealtimePublisher(IHubContext<NotificationHub> hub, RealtimeConnections connections,
    RealtimeAccess access) : IRealtimeEventPublisher
{
    public async Task PublishAsync(Guid eventId, string eventName, string payloadJson, CancellationToken ct)
    {
        var (group, envelope) = RealtimeEventContract.Parse(eventId, eventName, payloadJson);
        var recipients = new List<string>();
        foreach (var (id, entry) in connections.Items)
        {
            if (!entry.Groups.ContainsKey(group)) continue;
            if (await access.CanReadAsync(entry.User, group, ct)) recipients.Add(id);
            else { connections.Items.TryRemove(id, out _); entry.Abort(); }
        }
        // Snapshot authorized members, not an unchecked group broadcast. Offline delivery remains in DB.
        if (recipients.Count > 0) await hub.Clients.Clients(recipients).SendAsync(eventName, envelope, ct);
        // At-least-once: a crash between send and DB commit can replay this same eventId.
    }
}

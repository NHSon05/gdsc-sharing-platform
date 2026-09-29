using System.Collections.Concurrent;
using System.Security.Claims;

namespace GdscSharingPlatform.Api.Realtime;

// Local registry intentionally supports a single API instance. It allows delivery-time ACL checks.
public sealed class RealtimeConnections
{
    public sealed class Entry(ClaimsPrincipal user, Action abort)
    {
        public ClaimsPrincipal User { get; } = user;
        public Action Abort { get; } = abort;
        public ConcurrentDictionary<string, byte> Groups { get; } = new();
        public DateTimeOffset WindowStart { get; set; } = DateTimeOffset.UtcNow;
        public int Requests { get; set; }
    }
    public ConcurrentDictionary<string, Entry> Items { get; } = new();
}

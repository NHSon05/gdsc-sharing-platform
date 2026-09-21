using System.Collections.Concurrent;

namespace GdscSharingPlatform.Api.Authentication;

// Dành cho development chạy một API process.
public sealed class ExternalLoginAttemptStore
{
    private readonly ConcurrentDictionary<string, DateTimeOffset>
        _attempts = new();

    public string Create()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in _attempts)
        {
            if (entry.Value <= now)
            {
                _attempts.TryRemove(entry.Key, out _);
            }
        }

        var id = Guid.NewGuid().ToString("N");
        _attempts[id] = now.AddMinutes(5);

        return id;
    }

    public bool TryConsume(string? id)
    {
        return id is not null &&
               _attempts.TryRemove(id, out var expiresAt) &&
               expiresAt > DateTimeOffset.UtcNow;
    }
}
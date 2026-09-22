using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using GdscSharingPlatform.Application.Features.Auth.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace GdscSharingPlatform.Api.Authentication;

// Single API instance only. Multi-instance deployments require shared storage
// with atomic compare-and-delete and the same expiration guarantees.
public sealed class GoogleLoginHandoffStore(TimeProvider clock)
{
    private sealed record Entry(AuthResponse Session, string Challenge, DateTimeOffset ExpiresAt);
    private readonly Dictionary<string, Entry> entries = new();
    private readonly object gate = new();

    public static bool IsChallenge(string? value) => value is not null
        && Regex.IsMatch(value, "\\A[A-Za-z0-9_-]{43}\\z");

    public string Issue(AuthResponse session, string challenge)
    {
        if (!IsChallenge(challenge)) throw new ArgumentException("Invalid challenge.", nameof(challenge));
        lock (gate)
        {
            foreach (var key in entries.Where(pair => pair.Value.ExpiresAt <= clock.GetUtcNow())
                .Select(pair => pair.Key).ToArray()) entries.Remove(key);
            if (entries.Count >= 10000) throw new InvalidOperationException("Handoff capacity reached.");
            var code = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
            entries.Add(code, new Entry(session, challenge, clock.GetUtcNow().AddSeconds(60)));
            return code;
        }
    }

    public AuthResponse? Redeem(string? code, string? verifier)
    {
        if (!IsChallenge(code) || !IsChallenge(verifier)) return null;
        var challenge = WebEncoders.Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(verifier!)));
        lock (gate)
        {
            if (!entries.TryGetValue(code!, out var entry)) return null;
            if (entry.ExpiresAt <= clock.GetUtcNow()) { entries.Remove(code!); return null; }
            if (!CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(challenge), Encoding.ASCII.GetBytes(entry.Challenge))) return null;
            entries.Remove(code!);
            return entry.Session;
        }
    }
}

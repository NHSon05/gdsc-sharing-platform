using System.Text.Json;
using GdscSharingPlatform.Application.Common.Exceptions;

namespace GdscSharingPlatform.Application.Features.Sharing;

public sealed record SocialCursor(DateTimeOffset At, Guid Id, string Scope)
{
    public string Encode() => Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(this));
    public static SocialCursor? Decode(string? value, string scope)
    {
        if (value is null) return null;
        try
        {
            if (value.Length > 512) throw new FormatException();
            var cursor = JsonSerializer.Deserialize<SocialCursor>(Convert.FromBase64String(value));
            if (cursor is null || cursor.Id == Guid.Empty || cursor.Scope != scope || cursor.At.Offset != TimeSpan.Zero)
                throw new FormatException();
            return cursor;
        }
        catch (Exception e) when (e is FormatException or JsonException)
        { throw new ApplicationValidationException("cursor", "Invalid cursor for this query."); }
    }
}

using System.Text;
using System.Text.Json;
using GdscSharingPlatform.Domain.Enums;

namespace GdscSharingPlatform.Api.Realtime;

public static class RealtimeEventContract
{
    public static (string Group, JsonElement Envelope) Parse(Guid id, string name, string json)
    {
        try
        {
            Require(id != Guid.Empty && Encoding.UTF8.GetByteCount(json) <= 65536);
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 16 });
            var root = document.RootElement;
            Fields(root, "room", "envelope");
            var group = root.GetProperty("room").GetString() ?? throw new InvalidDataException();
            var e = root.GetProperty("envelope");
            Fields(e, "eventId", "eventName", "occurredAtUtc", "version", "data");
            Require(e.GetProperty("eventId").GetGuid() == id && e.GetProperty("eventName").GetString() == name
                && e.GetProperty("version").GetInt32() == 1);
            _ = e.GetProperty("occurredAtUtc").GetDateTimeOffset();
            var d = e.GetProperty("data");
            switch (name)
            {
                case "notification.created":
                    Fields(d, "notification", "unreadCount"); Count(d, "unreadCount");
                    Require(group.StartsWith("user:", StringComparison.Ordinal) && Guid.TryParseExact(group[5..], "D", out var user)
                        && user != Guid.Empty && group == $"user:{user}");
                    var n = d.GetProperty("notification");
                    Fields(n, "id", "type", "actorUserId", "entityType", "entityId", "title", "message", "route", "isRead", "createdAtUtc");
                    Id(n, "id"); Id(n, "entityId");
                    if (n.GetProperty("actorUserId").ValueKind != JsonValueKind.Null) Id(n, "actorUserId");
                    var type = n.GetProperty("type").GetString();
                    Require(Enum.GetNames<NotificationType>().Contains(type));
                    Require(n.GetProperty("entityType").GetString() is "SharingContent" or "ContentComment" or "SharingSchedule");
                    Require(n.GetProperty("title").GetString() is { Length: <= 200 }
                        && n.GetProperty("message").GetString() is { Length: <= 1000 });
                    var route = n.GetProperty("route").GetString();
                    Require(route is null || (route.Length <= 2048 && route.StartsWith('/') && !route.StartsWith("//")
                        && !route.Contains('\\') && !route.Any(char.IsControl)));
                    _ = n.GetProperty("isRead").GetBoolean(); _ = n.GetProperty("createdAtUtc").GetDateTimeOffset();
                    break;
                case "content.interaction.updated":
                    Fields(d, "contentId", "likeCount", "commentCount");
                    Require(group == $"content:{Id(d, "contentId")}"); Count(d, "likeCount"); Count(d, "commentCount");
                    break;
                case "comment.created":
                    Fields(d, "contentId", "commentId");
                    Require(group == $"content:{Id(d, "contentId")}"); Id(d, "commentId");
                    break;
                case "comment.updated": case "comment.deleted": case "comment.hidden":
                    Fields(d, "contentId", "commentId", "version");
                    Require(group == $"content:{Id(d, "contentId")}"); Id(d, "commentId"); Count(d, "version");
                    break;
                case "schedule.rsvp.updated":
                    Fields(d, "scheduleId", "going", "maybe", "notGoing");
                    Require(group == $"schedule:{Id(d, "scheduleId")}:admins");
                    Count(d, "going"); Count(d, "maybe"); Count(d, "notGoing");
                    break;
                default: throw new InvalidDataException();
            }
            return (group, e.Clone());
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException or KeyNotFoundException or ArgumentException or InvalidDataException)
        { throw new InvalidDataException("Invalid realtime event contract."); }
    }
    private static Guid Id(JsonElement e, string field)
    { var id = e.GetProperty(field).GetGuid(); Require(id != Guid.Empty); return id; }
    private static void Count(JsonElement e, string field) => Require(e.GetProperty(field).GetInt64() >= 0);
    private static void Require(bool valid) { if (!valid) throw new InvalidDataException(); }
    private static void Fields(JsonElement e, params string[] fields)
    {
        var actual = e.EnumerateObject().Select(p => p.Name).ToArray();
        Require(actual.Length == fields.Length && actual.Distinct().Count() == fields.Length && fields.All(actual.Contains));
    }
}

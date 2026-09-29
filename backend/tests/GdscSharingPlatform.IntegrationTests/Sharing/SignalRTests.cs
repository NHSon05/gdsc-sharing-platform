using System.Net;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using GdscSharingPlatform.Api.Realtime;
using GdscSharingPlatform.Application.Features.Sharing;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Infrastructure.Persistence;
using GdscSharingPlatform.Infrastructure.Services.Sharing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GdscSharingPlatform.IntegrationTests.Sharing;

public sealed partial class SharingEndpointsTests
{
    private async Task<HubSocket> ConnectHub(HttpClient client)
    {
        var wsClient = _factory.Server.CreateWebSocketClient();
        wsClient.ConfigureRequest = r => r.Headers["Origin"] = "http://localhost:3000";
        var token = client.DefaultRequestHeaders.Authorization!.Parameter;
        var ws = await wsClient.ConnectAsync(new Uri($"ws://localhost/hubs/notifications?access_token={token}"), default);
        var socket = new HubSocket(ws);
        await socket.Send(new { protocol = "json", version = 1 });
        var handshake = await socket.Read();
        Assert.Equal("{}", handshake.GetRawText());
        return socket;
    }

    [Fact]
    public async Task SignalR_LikeToOutboxToUser_MultipleTabsAndIsolation()
    {
        var id = await PublishedSocialContent();
        using var a = await ConnectHub(_owner);
        using var b = await ConnectHub(_owner);
        using var other = await ConnectHub(_other);
        // Successful invocation confirms OnConnected/group registration has completed.
        await a.Invoke("SubscribeContent", id); await b.Invoke("SubscribeContent", id);
        await other.Invoke("UnsubscribeContent", id);
        await _other.PutAsJsonAsync($"/api/sharing/contents/{id}/like", new { });
        using (var scope = _factory.Services.CreateScope())
            Assert.Equal(2, await scope.ServiceProvider.GetRequiredService<OutboxDispatcher>().DispatchAsync(default));
        var first = await a.Read(); var second = await b.Read();
        Assert.Equal("notification.created", first.GetProperty("target").GetString());
        Assert.Equal(first.GetProperty("arguments")[0].GetProperty("eventId").GetGuid(), second.GetProperty("arguments")[0].GetProperty("eventId").GetGuid());
        Assert.Equal(1, first.GetProperty("arguments")[0].GetProperty("data").GetProperty("unreadCount").GetInt32());
        Assert.Equal("content.interaction.updated", (await a.Read()).GetProperty("target").GetString());
        Assert.Equal("content.interaction.updated", (await b.Read()).GetProperty("target").GetString());
        // A completion as the next packet proves no notification leaked to the unrelated user.
        await other.Invoke("UnsubscribeContent", id);
    }

    [Fact]
    public async Task SignalR_SubscribeChecksVisibilityAndAdmin_NoArbitraryUserGroup()
    {
        var draft = await Read<ContentResponse>(await _owner.PostAsJsonAsync("/api/sharing/contents", Content() with { ContributorUserIds = [] }, Json), HttpStatusCode.Created);
        using var socket = await ConnectHub(_other);
        Assert.Contains("FORBIDDEN", await socket.Invoke("SubscribeContent", draft.Content.Id, false));
        Assert.Contains("FORBIDDEN", await socket.Invoke("SubscribeSchedule", Guid.NewGuid(), false));
        Assert.NotEmpty(await socket.Invoke("JoinGroup", _ownerId, false));
        using var owner = await ConnectHub(_owner);
        await owner.Invoke("SubscribeContent", draft.Content.Id);
    }

    [Fact]
    public async Task SignalR_RevokedSessionCannotReceiveQueuedNotification()
    {
        var id = await PublishedSocialContent();
        using var socket = await ConnectHub(_owner);
        await socket.Invoke("SubscribeContent", id);
        await _other.PutAsJsonAsync($"/api/sharing/contents/{id}/like", new { });
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Users.SingleAsync(x => x.Id == _ownerId)).TokenVersion++;
        await db.SaveChangesAsync();
        await scope.ServiceProvider.GetRequiredService<OutboxDispatcher>().DispatchAsync(default);
        var closed = await socket.Read();
        Assert.Equal(7, closed.GetProperty("type").GetInt32());
        Assert.Single(await db.Notifications.ToListAsync());
    }

    [Fact]
    public async Task SignalR_OriginAndAuthentication_AreScopedToHub()
    {
        using var anonymous = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsync("/hubs/notifications/negotiate?negotiateVersion=1", null)).StatusCode);
        var token = _owner.DefaultRequestHeaders.Authorization!.Parameter;
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync($"/api/notifications?access_token={token}")).StatusCode);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/hubs/notifications/negotiate?negotiateVersion=1");
        request.Headers.Add("Origin", "https://untrusted.example");
        Assert.Equal(HttpStatusCode.Forbidden, (await _owner.SendAsync(request)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _owner.PostAsync("/hubs/notifications/negotiate?negotiateVersion=1", null)).StatusCode);
    }

    [Fact]
    public async Task SignalR_ScheduleStats_AdminOnly_AndSubscriptionsResetAfterReconnect()
    {
        var request = Schedule([new(_ownerId, PresenterRole.Speaker)]) with
        {
            StartsAtLocal = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(10), DateTimeKind.Unspecified),
            EndsAtLocal = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(10).AddHours(1), DateTimeKind.Unspecified),
            TimeZoneId = "UTC"
        };
        var schedule = await Read<ScheduleResponse>(await _admin.PostAsJsonAsync("/api/admin/sharing/schedules", request, Json), HttpStatusCode.Created);
        await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/schedules/{schedule.Id}/publish", 0);
        using var presenter = await ConnectHub(_owner);
        Assert.Contains("FORBIDDEN", await presenter.Invoke("SubscribeSchedule", schedule.Id, false));
        using (var admin = await ConnectHub(_admin)) await admin.Invoke("SubscribeSchedule", schedule.Id);
        // New connection has only its user group; it must explicitly subscribe again.
        using var reconnected = await ConnectHub(_admin);
        await reconnected.Invoke("UnsubscribeContent", Guid.NewGuid());
        await _other.PutAsJsonAsync($"/api/sharing/schedules/{schedule.Id}/rsvp", new RsvpRequest(ScheduleRsvpStatus.Going), Json);
        using (var scope = _factory.Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<OutboxDispatcher>().DispatchAsync(default);
        await reconnected.Invoke("SubscribeSchedule", schedule.Id);
        var rsvp = await Read<RsvpResponse>(await _other.GetAsync($"/api/sharing/schedules/{schedule.Id}/rsvp/me"));
        await _other.PutAsJsonAsync($"/api/sharing/schedules/{schedule.Id}/rsvp", new RsvpRequest(ScheduleRsvpStatus.Maybe, rsvp.Version), Json);
        using (var scope = _factory.Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<OutboxDispatcher>().DispatchAsync(default);
        var packet = await reconnected.Read();
        Assert.Equal("schedule.rsvp.updated", packet.GetProperty("target").GetString());
        Assert.Equal(1, packet.GetProperty("arguments")[0].GetProperty("data").GetProperty("maybe").GetInt32());
    }

    [Fact]
    public async Task SignalR_RevokedHandshakeAndSubscribeRateLimit()
    {
        using (var socket = await ConnectHub(_other))
        {
            for (var i = 0; i < 60; i++) await socket.Invoke("UnsubscribeContent", Guid.NewGuid());
            Assert.Contains("RATE_LIMITED", await socket.Invoke("UnsubscribeContent", Guid.NewGuid(), false));
        }
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await db.Users.SingleAsync(x => x.Id == _ownerId)).TokenVersion++;
            await db.SaveChangesAsync();
        }
        using var revoked = await ConnectHub(_owner);
        Assert.Equal(7, (await revoked.Read()).GetProperty("type").GetInt32());
        using var invalid = _factory.CreateClient();
        invalid.DefaultRequestHeaders.Authorization = new("Bearer", "not-a-jwt");
        Assert.Equal(HttpStatusCode.Unauthorized, (await invalid.PostAsync("/hubs/notifications/negotiate?negotiateVersion=1", null)).StatusCode);
    }

    [Fact]
    public async Task SignalR_InvalidOutboxPayloadRetriesWithoutBroadcast()
    {
        var id = await PublishedSocialContent();
        await _other.PutAsJsonAsync($"/api/sharing/contents/{id}/like", new { });
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var message = await db.OutboxMessages.FirstAsync(x => x.Type == "notification.created");
        var publisher = scope.ServiceProvider.GetRequiredService<IRealtimeEventPublisher>();
        await Assert.ThrowsAsync<InvalidDataException>(() => publisher.PublishAsync(Guid.NewGuid(), message.Type, message.PayloadJson, default));
        await Assert.ThrowsAsync<InvalidDataException>(() => publisher.PublishAsync(message.Id, "unknown", message.PayloadJson, default));
        await Assert.ThrowsAsync<InvalidDataException>(() => publisher.PublishAsync(message.Id, message.Type, "{}", default));
        RealtimeEventContract.Parse(message.Id, message.Type, message.PayloadJson);
        var bad = new GdscSharingPlatform.Domain.Sharing.OutboxMessage(Guid.NewGuid(), "unknown", "{}", DateTimeOffset.UtcNow);
        db.Add(bad); await db.SaveChangesAsync();
        await scope.ServiceProvider.GetRequiredService<OutboxDispatcher>().DispatchAsync(default);
        var retried = await db.OutboxMessages.AsNoTracking().SingleAsync(x => x.Id == bad.Id);
        Assert.Equal(1, retried.RetryCount); Assert.Null(retried.ProcessedAtUtc);
    }

    private sealed class HubSocket(WebSocket socket) : IDisposable
    {
        private readonly Queue<JsonElement> _packets = new();
        private string _pending = "";
        public async Task Send(object value)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value) + "\u001e");
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, default);
        }
        public async Task<JsonElement> Read()
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            while (true)
            {
                if (_packets.TryDequeue(out var packet))
                {
                    if (packet.TryGetProperty("type", out var type) && type.GetInt32() == 6) continue;
                    return packet;
                }
                var bytes = new byte[65536];
                var result = await socket.ReceiveAsync(bytes, timeout.Token);
                if (result.MessageType == WebSocketMessageType.Close) return JsonSerializer.SerializeToElement(new { type = 7 });
                _pending += Encoding.UTF8.GetString(bytes, 0, result.Count);
                int index;
                while ((index = _pending.IndexOf('\u001e')) >= 0)
                {
                    using var document = JsonDocument.Parse(_pending[..index]);
                    _packets.Enqueue(document.RootElement.Clone()); _pending = _pending[(index + 1)..];
                }
            }
        }
        public async Task<string> Invoke(string method, Guid id, bool success = true)
        {
            await Send(new { type = 1, invocationId = "1", target = method, arguments = new[] { id } });
            var response = await Read();
            Assert.Equal(3, response.GetProperty("type").GetInt32());
            var error = response.TryGetProperty("error", out var e) ? e.GetString()! : "";
            if (success) Assert.Equal("", error); else Assert.NotEmpty(error);
            return error;
        }
        public void Dispose() { socket.Abort(); socket.Dispose(); }
    }
}

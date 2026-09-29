# Sprint 5 — Notification và ASP.NET Core SignalR

Tên file được giữ để không làm hỏng link cũ. Kế hoạch Phase 6 mới thay thế Socket.IO/Node gateway bằng SignalR trong backend.

## 1. Luồng triển khai

REST mutation → transaction PostgreSQL (business data + Notification + OutboxMessage) → Outbox Worker → IRealtimeEventPublisher → IHubContext<NotificationHub> → client handler.

Hub không ghi Like, Comment, Saved hoặc RSVP. Không có service Node.js, HTTP gateway endpoint, service credential hay cổng realtime riêng.

## 2. Endpoint và group

- Hub: `/hubs/notifications`.
- `user:{userId}`: server tự join theo JWT, không có method cho client chọn user group.
- `content:{contentId}`: quyền đọc giống REST content, bao gồm trạng thái Published và quyền tác giả/Admin.
- `schedule:{scheduleId}:admins`: Admin-only, đồng nhất REST RSVP stats Phase 5; chưa cấp stats cho Presenter.
- Methods: `SubscribeContent(contentId)`, `UnsubscribeContent(contentId)`, `SubscribeSchedule(scheduleId)`, `UnsubscribeSchedule(scheduleId)`; đối số UUID dạng chuỗi. Gọi bằng `connection.invoke` và chờ completion; lỗi trả HubException với mã AUTH_INVALID/FORBIDDEN/RATE_LIMITED/GROUP_LIMIT.
- Tối đa 20 subscriptions và 60 subscribe/unsubscribe mỗi phút/kết nối, cấu hình qua Realtime options.

## 3. Authentication và bảo mật

Dùng JWT hiện tại: signature, issuer, audience, expiry. Kiểm DB Active, IsDeleted, TokenVersion, lockout khi connect, khi gọi method và trước khi phát tới từng recipient đang online. Recheck định kỳ mặc định 30 giây; mất quyền sẽ đóng kết nối. Token hết hạn tự đóng qua `CloseOnAuthenticationExpiration`.

Client SignalR dùng `accessTokenFactory`; Authorization header được ưu tiên. Browser WebSocket/SSE dùng `access_token` query theo giao thức SignalR. Backend chỉ chấp nhận query credential trên đường dẫn chính xác `/hubs/notifications`, không chấp nhận trên REST hoặc negotiate. Không log query/token; proxy phải redact access_token. Dùng HTTPS production.

`Cors:AllowedOrigins` là allowlist chính xác; kiểm Origin riêng cho cả WebSocket upgrade, không dựa vào CORS đơn thuần. Client không có Origin vẫn cần JWT hợp lệ.

## 4. Event contract

Outbox JSON giữ nguyên để xử lý cả các record Phase 4–5 đã lưu:

```json
{
  "room": "user:<uuid>",
  "envelope": {
    "eventId": "<uuid>",
    "eventName": "notification.created",
    "occurredAtUtc": "2026-09-28T00:00:00Z",
    "version": 1,
    "data": {}
  }
}
```

Client nhận envelope, không nhận wrapper room. `eventId` phải khớp ID outbox, eventName khớp Type; version=1, payload tối đa 64 KiB. Validator kiểm shape, UUID, counts không âm, timestamp, route nội bộ, event allowlist và group tương ứng.

- `notification.created`: data `{notification, unreadCount}`. Notification có id, type, actorUserId (nullable), entityType, entityId, title, message, route (nullable), isRead, createdAtUtc.
- `content.interaction.updated`: contentId, likeCount, commentCount.
- `comment.created`: contentId, commentId.
- `comment.updated/deleted/hidden`: contentId, commentId, version.
- `schedule.rsvp.updated`: scheduleId, going, maybe, notGoing.

Notification event ID riêng từng recipient; room event có ID độc lập. Retry luôn giữ cùng ID. Re-like dưới 5 phút vẫn do DB xử lý; SignalR không thay đổi cooldown và không đặt timer gửi notification sau 5 phút.

## 5. Client Phase 7

```ts
const connection = new HubConnectionBuilder()
  .withUrl(API_URL + "/hubs/notifications", { accessTokenFactory })
  .withAutomaticReconnect()
  .build();

connection.on("notification.created", (event) => {
  if (seenEventIds.has(event.eventId)) return;
  seenEventIds.add(event.eventId);
  // Bounded, per-session dedupe cache; clear on logout/user switch.
  queryClient.invalidateQueries({ queryKey: notificationKeys.unreadCount() });
  queryClient.invalidateQueries({ queryKey: notificationKeys.list() });
});
await connection.start();
await connection.invoke("SubscribeContent", contentId);
```

Ví dụ minh họa, chưa tích hợp frontend. BFF cookie HttpOnly hiện tại cần cơ chế cấp credential hub phù hợp ở Phase 7; không public refresh token/JWT secret. Cleanup bằng connection.off/stop khi unmount hoặc logout.

## 6. Delivery, reconnect và shutdown

- Notification DB là nguồn chuẩn. SendAsync hoàn tất không chứng minh client đã nhận/đọc.
- Outbox chỉ MarkProcessed sau publisher thành công; lỗi validation hoặc send đều retry/backoff/dead-letter. Không có HTTP gateway ACK.
- At-least-once dispatch: crash sau send trước commit có thể phát lại cùng eventId. Không hứa exactly-once hoặc server dedupe bền vững. Client dedupe và refetch, không tăng badge +1.
- unreadCount là snapshot transaction, có thể đến trễ/sai thứ tự; refetch REST để lấy hiện tại, kể cả sau mark-read/read-all.
- Không bật stateful reconnect/replay. Mỗi reconnect xác thực lại, subscribe lại entity đang mở và refetch notification/count/entity. AutomaticReconnect không tự retry lần start đầu hoặc thay thế refresh token.
- Keep-alive 15 giây, client timeout 30 giây, handshake timeout 15 giây, hub receive limit 16 KiB, transport/application buffers 64 KiB.
- Host shutdown timeout 30 giây; cancellation dừng worker và đóng connections. Event chưa commit vẫn pending để xử lý sau restart.

## 7. Vận hành

Bật `SocialOutbox__Enabled=true` trong backend để worker phát; mặc định false nhằm bật chủ động. Không cần GatewayUrl/ServiceToken. Realtime__SubscribePerMinute, Realtime__MaxSubscriptions, Realtime__SessionRecheckSeconds cấu hình giới hạn. CORS dùng cấu hình hiện có.

Registry connections hiện local, chỉ hỗ trợ một API instance. Không chạy nhiều replica/worker độc lập: SKIP LOCKED có thể giao event cho instance không sở hữu connection. Scale-out cần shared connection routing + authorization-aware distribution; chỉ thêm Redis backplane chưa đủ với registry hiện tại.

Tham khảo chính thức: [authentication](https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-10.0), [configuration](https://learn.microsoft.com/en-us/aspnet/core/signalr/configuration?view=aspnetcore-10.0).

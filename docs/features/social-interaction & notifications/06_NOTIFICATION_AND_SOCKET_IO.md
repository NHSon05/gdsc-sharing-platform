# Sprint 5 — Notification và Socket.IO

## 1. Kiến trúc

ASP.NET Core API tiếp tục sở hữu nghiệp vụ và database. Bổ sung một Node.js Socket.IO Gateway độc lập:

```text
REST Mutation
→ PostgreSQL transaction
   ├── Business data
   ├── Notification
   └── OutboxMessage
→ Outbox Worker
→ Internal authenticated request
→ Socket.IO Gateway
→ emit đến room
→ Client on event
```

Socket.IO Gateway không tự ghi Like, Comment, Saved hoặc RSVP.

## 2. Namespace và room

Namespace:

```text
/realtime
```

Room:

```text
user:{userId}
content:{contentId}
schedule:{scheduleId}:admins
```

- Sau khi xác thực, server tự join `user:{userId}`.
- Client không được tự chọn user room.
- Client có thể request subscribe content room; gateway xác thực trước khi join.
- Schedule admin room chỉ dành cho Admin và Presenter được phép xem thống kê realtime.

## 3. Authentication handshake

Client gửi access token trong `auth`, không gửi token trong query string:

```ts
const socket = io(`${REALTIME_URL}/realtime`, {
  auth: { accessToken },
  transports: ["websocket", "polling"],
});
```

Gateway middleware kiểm tra:

- Chữ ký JWT.
- Issuer và audience.
- Expiration.
- TokenVersion hoặc session validity nếu kiến trúc hiện tại hỗ trợ kiểm tra.
- User còn hoạt động.

Token hết hạn:

```text
connect_error AUTH_EXPIRED
→ Client gọi refresh qua REST
→ Cập nhật auth.accessToken
→ socket.connect()
```

## 4. Event envelope

Mọi event dùng một envelope thống nhất:

```json
{
  "eventId": "uuid",
  "eventName": "notification.created",
  "occurredAtUtc": "2026-09-26T15:30:00Z",
  "version": 1,
  "data": {}
}
```

Client lưu ngắn hạn các `eventId` đã xử lý để tránh áp dụng trùng. `eventId` xác định một realtime message và không đổi khi outbox/gateway retry. Với notification, mỗi recipient có event ID riêng; content/schedule room event có ID độc lập.

## 5. Server emit và client on

### Notification mới

Server:

```ts
io.of("/realtime")
  .to(`user:${recipientUserId}`)
  .emit("notification.created", envelope);
```

Client:

```ts
socket.on("notification.created", (event) => {
  if (seenEventIds.has(event.eventId)) return;
  seenEventIds.add(event.eventId);
  // Count snapshots can arrive out of order or after a mark-read mutation.
  queryClient.invalidateQueries({ queryKey: notificationKeys.unreadCount() });
  queryClient.invalidateQueries({ queryKey: notificationKeys.list() });
});
```

`unreadCount` trong payload là snapshot được tính trong transaction ghi notification. REST count là trạng thái hiện tại; client refetch sau event để tránh snapshot cũ ghi đè trạng thái mới. Không tự tăng `+1`. Sau reconnect không recovery hoặc khi REST mutation mark-read/read-all hoàn tất, client refetch từ REST. `seenEventIds` là cache giới hạn dung lượng theo user session, xóa khi logout/đổi user.

### Tương tác bài viết

```text
content.interaction.updated
comment.created
comment.updated
comment.deleted
comment.hidden
```

### RSVP

```text
schedule.rsvp.updated
```

## 6. Payload đề xuất

`notification.created`:

```json
{
  "notification": {
    "id": "uuid",
    "type": "ContentLiked",
    "message": "Nguyễn Văn A đã thích bài viết của bạn.",
    "route": "/sharing/clean-architecture",
    "isRead": false,
    "createdAtUtc": "2026-09-26T15:30:00Z"
  },
  "unreadCount": 6
}
```

`content.interaction.updated`:

```json
{
  "contentId": "uuid",
  "likeCount": 25,
  "commentCount": 8
}
```

`schedule.rsvp.updated`:

```json
{
  "scheduleId": "uuid",
  "going": 33,
  "maybe": 7,
  "notGoing": 4
}
```

## 7. Subscribe và unsubscribe

Client emit:

```ts
socket.emit("content.subscribe", { contentId }, acknowledgement);
socket.emit("content.unsubscribe", { contentId });
```

Gateway on:

```ts
socket.on("content.subscribe", async ({ contentId }, ack) => {
  // validate access, then join content room
});
```

Mọi event client → gateway cần acknowledgement và validation. Không dùng socket event để thay thế REST mutation nghiệp vụ.

## 8. Reconnect và đồng bộ lại

- Bật connection state recovery trong khoảng ngắn.
- Đặt `skipMiddlewares: false` để xác thực lại khi cần.
- Khi `socket.recovered === false`, client refetch notification list, unread count và entity đang mở.
- REST API vẫn là cơ chế khôi phục đầy đủ sau mất kết nối dài hoặc restart server.

## 9. Outbox Worker

- Đọc message chưa xử lý theo batch.
- Gửi `eventId` đến gateway.
- Gateway xử lý idempotent theo `eventId` trong cửa sổ chống trùng.
- Thành công mới cập nhật `ProcessedAtUtc`.
- Thất bại tăng RetryCount và exponential backoff.
- Có dead-letter policy sau số lần thử tối đa.

## 10. Scale out

Giai đoạn đầu có thể chạy một Socket.IO Gateway. Khi chạy nhiều instance:

- Dùng Socket.IO adapter phù hợp để chia sẻ room/broadcast.
- Không dùng `socket.id` làm user ID.
- Giữ room theo `user:{stableUserId}`.

# Phase 6 — ASP.NET Core SignalR

Đã thay thiết kế Node.js/Socket.IO bằng SignalR trực tiếp trong ASP.NET Core theo kế hoạch mới. Không tạo service realtime riêng, không thêm package Node hay migration database.

## Thành phần

- `NotificationHub` tại `/hubs/notifications`, tự join user group từ JWT. Content subscribe theo quyền đọc hiện tại; schedule stats group Admin-only như REST.
- `RealtimeAccess` kiểm Active/IsDeleted/TokenVersion/lockout/expiry; kiểm quyền trước connect, method và delivery. Monitor recheck định kỳ; session không còn hợp lệ bị đóng. JWT expiry đóng kết nối tự động.
- `IRealtimeEventPublisher` tách worker khỏi transport. `SignalRRealtimePublisher` dùng IHubContext, gửi tới snapshot connection đã kiểm quyền của group.
- `RealtimeEventContract` kiểm event ID/name/version, room, shape/payload/route trước gửi. Outbox JSON cũ được giữ nguyên.
- Worker giữ retry/backoff/dead-letter, timeout và SKIP LOCKED; lỗi publisher không đánh dấu processed. Message đã processed không gửi lại, nhưng crash sau send trước commit vẫn có thể retry cùng ID. Client Phase 7 phải dedupe/refetch.
- CORS allowlist riêng cho hub, kiểm Origin cả WebSocket; token query chỉ tại hub, không dùng trên REST. Limit 16 KiB inbound, 64 KiB outbound contract/buffers, keepalive 15 giây, client timeout 30 giây, host shutdown 30 giây.

## Chạy

Backend vẫn chạy theo cách hiện có. Hub được map sẵn; bật worker bằng `SocialOutbox__Enabled=true`. Nếu chạy Docker Compose, đặt `SOCIAL_OUTBOX_ENABLED=true` trong `backend/.env`. Không còn `GatewayUrl` hoặc `ServiceToken`. Không thay đổi `.env` thật hay tự khởi động lại container trong lần triển khai này.

`Realtime:SubscribePerMinute=60`, `MaxSubscriptions=20`, `SessionRecheckSeconds=30` có validation lúc startup. `Cors:AllowedOrigins` cấu hình origin frontend. Production dùng HTTPS và redact access_token ở reverse proxy/access logs; mặc định logging ASP.NET Core Warning tránh request URL ở Information.

## Giới hạn

- Chỉ một API instance; registry local không hỗ trợ scale-out. Cần thiết kế routing/phân quyền phân tán trước khi thêm replica.
- Không replay/stateful reconnect; reconnect phải xác thực lại, subscribe lại và refetch. Notification DB là nguồn chuẩn khi offline/restart.
- BFF cookie HttpOnly cần chiến lược cấp credential hub phù hợp ở Phase 7; không expose refresh token/JWT secret.
- Chưa triển khai frontend provider/UI, chưa deploy hay bật worker trên môi trường thật.

## Kiểm chứng

- Build thành công; 380 unit tests pass.
- Regression integration: 110 pass, 20 PostgreSQL tests skip vì chưa cấu hình DB kiểm thử trong lần chạy này.
- 7 test SignalR dùng WebSocket trong TestServer: REST Like → Outbox → user event; nhiều tab/isolation; content ACL; RSVP Admin-only; reconnect mất subscription; session revoked; origin/JWT; subscribe rate limit; invalid payload/retry. Không dùng gateway/API giả lập.
- Test database mặc định là EF InMemory; chưa kiểm chứng full luồng mới trên PostgreSQL hoặc frontend trình duyệt.
- Compose config validation pass; vẫn có cảnh báo biến Google BFF callback chưa cấu hình từ cấu hình hiện có. Không build/run lại container.

Chi tiết contract và client example: [06_NOTIFICATION_AND_SOCKET_IO.md](./06_NOTIFICATION_AND_SOCKET_IO.md) (giữ tên file cũ để bảo toàn link, nội dung đã chuyển sang SignalR).

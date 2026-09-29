# Sprint 5 — Kế hoạch triển khai

## Phase 1 — Domain

- Tạo `ContentLike`.
- Tạo `SavedContent`.
- Tạo `ContentComment`.
- Tạo `ScheduleRsvp`.
- Tạo `Notification`.
- Tạo enum interaction, comment, RSVP và notification.
- Cài đặt state transition và business rule.

## Phase 2 — Persistence

- Cấu hình EF Core và quan hệ.
- Tạo unique index và query index.
- Tạo `OutboxMessage` JSONB.
- Cấu hình concurrency token.
- Tạo migration.
- Cấu hình restrict/cascade theo đặc tả.

## Phase 3 — Application

- Tạo DTO, validator và service interface.
- Tạo policy cho comment author, audience và notification owner.
- Tạo notification recipient resolver.
- Tạo policy chống spam `ContentLiked` 5 phút theo actor/content/recipient và sinh EventId riêng cho từng recipient.
- Tạo event envelope và event-name constants.
- Tạo query cursor pagination.
- Chốt RSVP dùng optimistic concurrency: create không gửi version; update/withdraw gửi version hiện tại và nhận `412` khi stale.

## Phase 4 — Infrastructure

- Cài đặt Like, Comment, Saved và RSVP service.
- Cài đặt Notification service.
- Ghi Notification và Outbox cùng transaction.
- Cài đặt Outbox Worker với retry/backoff.
- Cài đặt audit và metrics.

## Phase 5 — REST API

- Tạo interaction controller.
- Tạo comment moderation controller.
- Tạo RSVP controller.
- Tạo notification controller.
- Áp dụng authentication, authorization và rate limiting.
- Bổ sung Problem Details và Swagger.

## Phase 6 — ASP.NET Core SignalR

- Cài đặt SignalR Hub trong backend ASP.NET Core, ví dụ `NotificationHub`.
- Cấu hình endpoint hub, ví dụ `/hubs/notifications`.
- Cấu hình JWT authentication cho SignalR handshake, hỗ trợ lấy access token từ query string khi client kết nối hub.
- Tạo user group theo user hiện tại; client không được tự chọn user group.
- Tạo content/schedule group khi cần subscribe theo quyền đọc hiện tại.
- Tạo service phát realtime event từ Outbox Worker qua `IHubContext<NotificationHub>`.
- Kiểm tra session, TokenVersion, trạng thái user và quyền đọc trước khi cho kết nối hoặc join group.
- Kiểm tra EventId, event name và payload trước khi gửi event.
- Cấu hình CORS, giới hạn payload, keep-alive/client-timeout và graceful shutdown trong backend.
- Giữ Notification trong database là nguồn dữ liệu chuẩn; SignalR chỉ dùng để báo client refetch hoặc cập nhật cache an toàn.

## Phase 7 — Frontend realtime

- Cài `@microsoft/signalr`.
- Tạo authenticated SignalR provider duy nhất.
- Tạo event contract TypeScript tương ứng với backend envelope.
- Đăng ký handler bằng `connection.on` và cleanup bằng `connection.off`.
- Đồng bộ TanStack Query cache.
- Dedupe `eventId`; `unreadCount` trong payload là snapshot, refetch REST để tránh giá trị cũ ghi đè và không tăng badge cục bộ.
- Refetch khi reconnect hoặc khi connection bị gián đoạn.
- Tạo Notification bell và Notification page.

## Phase 8 — Frontend interaction

- Tạo Like, Comment và Saved UI.
- Tạo Saved Content page.
- Tạo RSVP control.
- Tạo Admin RSVP summary.
- Tạo Admin comment moderation.
- Bổ sung accessibility và empty/error state.

## Phase 9 — Testing

- Unit Test domain và validator.
- Integration Test REST và transaction.
- SignalR hub/realtime integration test.
- Outbox reliability test.
- Frontend hook/component test.
- End-to-end test cho like → notification realtime.
- Regression Test Sprint 1–4.

## Thứ tự triển khai

```text
Domain
→ Persistence + Outbox
→ REST Application/Infrastructure/API
→ Backend tests
→ SignalR Hub
→ Realtime integration tests
→ Frontend SignalR provider
→ Interaction + Notification UI
→ End-to-end tests
→ Documentation
```

## Definition of Done

- Like, Comment, Saved và RSVP hoạt động đúng quyền và idempotency.
- Content author nhận notification khi có người khác like.
- Notification tồn tại trong database trước khi gửi realtime event.
- SignalR xác thực JWT và gửi event đúng user group.
- Client đăng ký handler realtime và cập nhật Query Cache không trùng.
- Mất kết nối không làm mất notification vì client refetch từ REST khi reconnect.
- Outbox retry và dead-letter hoạt động.
- Comment được xóa mềm và moderation có audit.
- RSVP tuân thủ audience và thời gian Schedule.
- Rate limiting và XSS protection hoạt động.
- Unit, integration, SignalR hub, frontend và end-to-end test đều pass.
- Swagger và event contract được cập nhật.
- Authentication, Profile, Roadmap và Sharing của sprint trước không bị ảnh hưởng.

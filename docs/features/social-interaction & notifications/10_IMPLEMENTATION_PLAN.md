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

## Phase 6 — Socket.IO Gateway

- Tạo service Node.js TypeScript riêng tại `realtime/`.
- Cấu hình namespace `/realtime`.
- Cấu hình JWT middleware.
- Tạo user/content/schedule room.
- Tạo internal endpoint nhận event từ Outbox Worker.
- Kiểm tra internal service credential và EventId.
- Cấu hình connection state recovery.
- Cấu hình CORS, payload limit và graceful shutdown.

## Phase 7 — Frontend realtime

- Cài `socket.io-client`.
- Tạo authenticated Socket provider duy nhất.
- Tạo event contract TypeScript.
- Đăng ký `on` và cleanup bằng `off`.
- Đồng bộ TanStack Query cache.
- Dedupe `eventId` và dùng `unreadCount` trong payload làm giá trị chuẩn cho notification badge.
- Refetch khi reconnect không recover.
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
- Socket gateway test.
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
→ Socket.IO Gateway
→ Gateway tests
→ Frontend Socket provider
→ Interaction + Notification UI
→ End-to-end tests
→ Documentation
```

## Definition of Done

- Like, Comment, Saved và RSVP hoạt động đúng quyền và idempotency.
- Content author nhận notification khi có người khác like.
- Notification tồn tại trong database trước khi emit.
- Socket.IO xác thực JWT và emit đúng user room.
- Client `on` event và cập nhật Query Cache không trùng.
- Mất kết nối không làm mất notification.
- Outbox retry và dead-letter hoạt động.
- Comment được xóa mềm và moderation có audit.
- RSVP tuân thủ audience và thời gian Schedule.
- Rate limiting và XSS protection hoạt động.
- Unit, integration, gateway, frontend và end-to-end test đều pass.
- Swagger và event contract được cập nhật.
- Authentication, Profile, Roadmap và Sharing của sprint trước không bị ảnh hưởng.

# Sprint 5 — Social Interaction và Notification

## Mục tiêu

Sprint 5 bổ sung khả năng tương tác thời gian thực cho GDSC Sharing Platform:

- Like bài Sharing Content.
- Comment và reply comment.
- Lưu bài viết để xem lại.
- Xác nhận tham gia buổi Sharing Schedule.
- Nhận thông báo trong ứng dụng khi bài viết được like hoặc có tương tác liên quan.
- Sử dụng SignalR để server `SendAsync` và client `connection.on` các sự kiện realtime.

## Nguyên tắc kiến trúc

```text
Client
├── REST API: ghi dữ liệu nghiệp vụ
└── SignalR: nhận thay đổi realtime

ASP.NET Core API
├── PostgreSQL
├── Notification
├── OutboxMessage
└── Outbox Worker
    └── SignalR Hub (IHubContext)
        └── SendAsync đến user/content/schedule group đã kiểm quyền
```

REST API và database là nguồn dữ liệu chính. SignalR chỉ giúp giao diện cập nhật ngay; người dùng offline vẫn đọc được notification đã lưu khi đăng nhập lại.

## Danh sách tài liệu

1. [01_REQUIREMENTS.md](./01_REQUIREMENTS.md): Phạm vi, vai trò và quy tắc nghiệp vụ.
2. [02_DATA_MODEL.md](./02_DATA_MODEL.md): Entity, quan hệ và index.
3. [03_API_CONTRACT.md](./03_API_CONTRACT.md): REST API cho Like, Comment, Saved, RSVP và Notification.
4. [04_SOCIAL_INTERACTION_WORKFLOW.md](./04_SOCIAL_INTERACTION_WORKFLOW.md): Luồng Like, Comment và Saved.
5. [05_SCHEDULE_RSVP_WORKFLOW.md](./05_SCHEDULE_RSVP_WORKFLOW.md): Luồng xác nhận tham gia.
6. [06_NOTIFICATION_AND_SOCKET_IO.md](./06_NOTIFICATION_AND_SOCKET_IO.md): Event contract và kiến trúc realtime.
7. [07_UI_UX.md](./07_UI_UX.md): Hành vi giao diện.
8. [08_SECURITY_AND_RELIABILITY.md](./08_SECURITY_AND_RELIABILITY.md): Phân quyền, chống spam và độ tin cậy.
9. [09_TEST_PLAN.md](./09_TEST_PLAN.md): Acceptance Criteria và kiểm thử.
10. [10_IMPLEMENTATION_PLAN.md](./10_IMPLEMENTATION_PLAN.md): Phase triển khai và Definition of Done.
11. [11_PHASE_4_5_NOTES.md](./11_PHASE_4_5_NOTES.md): Kết quả Phase 4–5, contract thực tế và cấu hình Outbox Worker.
12. [12_PHASE_6_NOTES.md](./12_PHASE_6_NOTES.md): Kết quả Phase 6 SignalR, cấu hình và giới hạn vận hành.

## Phạm vi Sprint 5

Bao gồm:

- Like/unlike idempotent.
- Comment, reply một cấp, sửa và xóa mềm.
- Saved Content riêng tư.
- RSVP Going, Maybe hoặc NotGoing.
- Notification trong ứng dụng và số lượng chưa đọc.
- Notification realtime khi có like, comment, reply hoặc thay đổi lịch quan trọng.
- SignalR authentication, user room, content room và schedule room.
- Outbox để phát event sau khi transaction database thành công.

Chưa bao gồm:

- Email, SMS hoặc mobile push notification.
- Emoji reaction ngoài Like.
- Reply comment nhiều cấp.
- Mention `@user`.
- Báo cáo nội dung xấu.
- Điểm danh thực tế.
- Giới hạn chỗ ngồi hoặc waitlist.
- Chat trực tiếp.

## Tài liệu tham chiếu

- SignalR delivery guarantees: https://learn.microsoft.com/aspnet/core/signalr/hubs
- SignalR rooms: https://learn.microsoft.com/aspnet/core/signalr/groups
- SignalR middleware: https://learn.microsoft.com/aspnet/core/signalr/authn-and-authz
- SignalR connection state recovery: https://learn.microsoft.com/aspnet/core/signalr/javascript-client

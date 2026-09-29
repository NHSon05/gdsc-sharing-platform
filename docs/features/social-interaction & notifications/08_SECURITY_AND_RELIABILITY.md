# Sprint 5 — Security và Reliability

## 1. Authorization

- Like, comment, save và RSVP yêu cầu access token hợp lệ.
- Backend kiểm tra Content Published và Schedule audience.
- User chỉ sửa/xóa comment của mình.
- Admin moderation dùng policy riêng.
- User chỉ đọc và mark-read notification của mình.
- SignalR Hub không tin userId hoặc group name do client gửi.

## 2. Socket authentication

- Client dùng `accessTokenFactory`; header được ưu tiên, browser WebSocket/SSE có thể dùng `access_token` query chỉ trên `/hubs/notifications`.
- Không log token/query; cấu hình reverse proxy redact access_token và dùng HTTPS.
- JWT middleware xác thực issuer, audience, signature và expiry; Hub kiểm DB session/TokenVersion/Active/lockout.
- `connectionId` chỉ là ID kết nối tạm thời.
- Mỗi kết nối được ánh xạ đến stable user ID từ JWT.
- Khi token hết hạn, client refresh bằng REST rồi reconnect.

## 3. Rate limiting

Baseline đề xuất:

| Hành động | Giới hạn |
|---|---:|
| Like/unlike | 60 lần/phút/user |
| Save/unsave | 60 lần/phút/user |
| Tạo comment/reply | 10 lần/phút/user |
| Sửa comment | 20 lần/phút/user |
| RSVP | 20 lần/phút/user |
| Socket subscribe | 60 lần/phút/connection |

Các giá trị phải cấu hình được, không hard-code.

## 4. Nội dung và XSS

- Comment lưu Markdown giới hạn.
- Không cho raw HTML.
- Render bằng sanitizer được phê duyệt.
- Chặn URL nguy hiểm trong Markdown.
- Không nhúng nội dung comment trực tiếp vào HTML notification.

## 5. Độ tin cậy event

SignalR không thay thế persistent storage. Vì vậy:

- Notification ghi database trước.
- Outbox ghi cùng transaction với nghiệp vụ.
- Chỉ emit sau commit.
- Event có `eventId` để dedupe.
- Client refetch và subscribe lại sau mọi reconnect; Phase 6 không bật replay/stateful reconnect.
- Event bị phát trùng không làm tăng count hai lần.

## 6. Privacy

- Saved Content chỉ chủ sở hữu được xem.
- Notification chỉ gửi đến user room tương ứng.
- Danh sách RSVP chỉ Admin xem trong Sprint 5.
- Không broadcast Meeting URL hoặc dữ liệu profile nhạy cảm.
- Payload realtime chỉ chứa dữ liệu tối thiểu cần render.

## 7. Audit và observability

Ghi audit cho:

- Admin hide/restore comment.
- Admin xem/export RSVP nếu chức năng export được bổ sung.
- Thay đổi notification system template.

Theo dõi metrics:

- Socket connections hiện tại.
- Connect error theo mã.
- Outbox pending và dead-letter count.
- Thời gian từ commit đến emit.
- Số event phát thất bại.
- Tỷ lệ reconnect và recovery.

## 8. CORS và origin

- Hub chỉ cho phép origin frontend đã cấu hình, kiểm Origin riêng cho WebSocket.
- Không dùng wildcard origin khi gửi credentials.
- Giới hạn payload và timeout handshake.
- Tắt event không sử dụng.

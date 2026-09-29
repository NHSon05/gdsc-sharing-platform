# Sprint 5 — Test Plan

## 1. Acceptance Criteria

| ID     | Tiêu chí nghiệm thu                                                                                                 |
| ------ | ------------------------------------------------------------------------------------------------------------------- |
| AC-01  | User like một Content Published thành công.                                                                         |
| AC-02  | Like lặp lại không tạo bản ghi trùng.                                                                               |
| AC-03  | Unlike lặp lại không gây lỗi hoặc count âm.                                                                         |
| AC-04  | Tác giả nhận notification khi người khác like.                                                                      |
| AC-05  | User tự like bài mình không nhận notification.                                                                      |
| AC-05a | Unlike rồi like lại trong 5 phút vẫn like thành công nhưng không tạo notification; sau 5 phút tạo notification mới. |
| AC-06  | User comment và reply một cấp thành công.                                                                           |
| AC-07  | Không thể reply vào reply.                                                                                          |
| AC-08  | User chỉ sửa/xóa comment của mình.                                                                                  |
| AC-09  | Comment bị xóa mềm vẫn giữ reply.                                                                                   |
| AC-10  | Admin hide/restore comment thành công.                                                                              |
| AC-11  | Save/unsave idempotent và không tạo notification.                                                                   |
| AC-12  | Saved Content chỉ chủ sở hữu xem được.                                                                              |
| AC-13  | User RSVP lịch thuộc audience trước giờ bắt đầu.                                                                    |
| AC-14  | User không RSVP được lịch ngoài audience.                                                                           |
| AC-15  | RSVP bị khóa khi lịch bắt đầu, hoàn tất hoặc hủy.                                                                   |
| AC-16  | Admin xem được danh sách và summary RSVP.                                                                           |
| AC-17  | Notification được lưu trước khi emit.                                                                               |
| AC-18  | User chỉ xem/mark-read notification của mình.                                                                       |
| AC-19  | Unread count chính xác sau read và read-all.                                                                        |
| AC-20  | Socket handshake từ chối token sai/hết hạn.                                                                         |
| AC-21  | Kết nối tự join đúng user room.                                                                                     |
| AC-22  | Client nhận `notification.created` realtime.                                                                        |
| AC-23  | Mất socket không làm mất notification đã lưu.                                                                       |
| AC-24  | Reconnect không recover được sẽ refetch REST.                                                                       |
| AC-25  | Event trùng `eventId` không cập nhật UI hai lần.                                                                    |
| AC-26  | Outbox retry khi SignalR publisher tạm thời lỗi.                                                                   |
| AC-27  | Member gọi moderation hoặc RSVP admin API nhận 403.                                                                 |
| AC-28  | Rate limit trả 429 đúng trường hợp.                                                                                 |

## 2. Unit Test

### Like và Saved

- Unique constraint.
- Idempotent create/delete.
- Không notification self-like.
- Recipient loại trùng khi có nhiều author role.
- Cửa sổ 5 phút của re-like được kiểm tra độc lập theo actor, content và recipient; retry cùng EventId không tạo record mới.

### Comment

- Trim và giới hạn độ dài.
- Parent cùng Content.
- Chặn nested reply.
- Transition Active → Deleted/Hidden.
- Optimistic concurrency.

### RSVP

- Status hợp lệ.
- Audience authorization.
- Thời điểm trước StartsAtUtc.
- Upsert và withdraw.

### Notification

- Mark read idempotent.
- Unread count.
- Unique EventId cho recipient.
- Route và entity mapping.

## 3. Integration Test REST

- Like tạo Notification và Outbox trong cùng transaction.
- Rollback không để lại Like, Notification hoặc Outbox rời rạc.
- Comment tạo đúng recipient.
- Saved không lộ giữa hai user.
- RSVP lọc đúng audience Gen/Department.
- Schedule cancellation tạo notification đúng đối tượng.
- Cursor pagination không bỏ sót hoặc lặp item.

## 4. SignalR Test

- Kết nối token hợp lệ.
- Từ chối token sai, hết hạn hoặc user inactive.
- Nhiều tab cùng user đều nhận user-room event.
- User khác không nhận notification.
- Subscribe content room được kiểm tra quyền.
- Listener `on/off` không bị đăng ký trùng.
- Reconnect xác thực lại, subscribe lại và refetch REST; không replay cache cũ.
- Retry giữ nguyên EventId; client dedupe, không tăng count hai lần. Publisher không hứa exactly-once qua crash.
- Client nhận EventId trùng không tăng unread count; EventId mới dùng `unreadCount` từ server, không tự cộng cục bộ.

## 5. Outbox Test

- Worker chỉ lấy message chưa xử lý.
- Thành công đặt ProcessedAtUtc.
- Thất bại tăng RetryCount và đặt NextAttemptAtUtc.
- Worker restart không làm mất message.
- Hai worker không xử lý cùng message đồng thời.
- Dead-letter sau ngưỡng cấu hình.

## 6. Regression Test

- Authentication và refresh rotation.
- Profile và membership.
- Roadmap node–edge.
- Sharing Content workflow.
- Sharing Schedule và audience.
- Migration Sprint 5 chạy sau Sprint 1–4.

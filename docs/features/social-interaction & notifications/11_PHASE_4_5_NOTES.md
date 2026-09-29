# Sprint 5 — Phase 4 và 5

## Đã triển khai

- Service Like/Unlike, Saved, Comment/Reply/Edit/Delete/Moderation, RSVP và Notification.
- REST controllers, active-user authorization, Admin-only moderation/RSVP reports, rate limits cấu hình được, Problem Details và Swagger.
- Like/Notification/Outbox ghi cùng transaction PostgreSQL Serializable. Retry có giới hạn khi serialization/unique conflict; request idempotent đọc lại trạng thái sau retry.
- Re-like cooldown theo actor/content/recipient: dưới 5 phút không tạo notification mới; đúng 5 phút được tạo. Mốc thời gian là notification gần nhất, không phải lần unlike hoặc lần re-like bị chặn. Không có delayed notification.
- Reply ưu tiên `CommentReplied` nếu người nhận cũng là tác giả bài; mỗi recipient chỉ nhận một notification cho lần reply đó. Mỗi notification có event ID riêng và giữ nguyên qua retry.
- Sửa lịch Scheduled đổi start/end/time zone, location, delivery mode hoặc Meeting URL sẽ thông báo Going/Maybe và Presenter; hủy lịch giữ RSVP và thông báo cùng nhóm. Người gây ra thay đổi không tự nhận thông báo.
- Link lịch dùng route hiện có `/schedule?id={id}`. Nội dung thông báo không chứa Meeting URL hoặc body comment.
- Outbox worker dùng `FOR UPDATE SKIP LOCKED`, một message/transaction, HTTP timeout, exponential backoff và dead-letter. Hai worker không giữ cùng row đồng thời; gateway phải dedupe khi worker gửi lại sau mất ACK/crash.
- Audit hide/restore và xem danh sách RSVP. Meter `Gdsc.Social.Outbox`: delivered, failed, commit_to_delivery (giây), pending và dead_letter (snapshot số message qua histogram).
- Migration EF mới gồm Designer và ModelSnapshot; thay migration viết tay chưa được apply của Phase 2.

## Quy ước API

- Comment list là flat cursor page `(CreatedAtUtc, Id)`, có `ParentCommentId`; client ghép theo ID. Active comment và reply được tính vào commentCount. Deleted/Hidden không lộ body hoặc moderation reason cho Member.
- Edit/delete chỉ cho chính tác giả. Admin moderation bằng hide/restore. Markdown bị từ chối nếu có raw HTML hoặc executable URL scheme; UI vẫn cần sanitizer khi render trong Phase 8.
- RSVP `status` bắt buộc; đổi trạng thái cần body `version`, rút cần `If-Match`. Retry PUT cùng trạng thái là no-op. GET RSVP chưa có trả 204.
- NoResponse tính theo active users trong audience hiện tại chưa có RSVP; không lấy tổng audience trừ tổng RSVP vì Presenter/Admin có thể nằm ngoài audience.
- Saved list trả `SharingPage<SavedContentItem>`, sắp theo ngày lưu mới nhất; Member chỉ thấy Published. Unsave được phép kể cả bài đã archive.
- Trạng thái interaction nằm trong `ContentSummary`, giữ wrapper detail của Sprint 4.
- PATCH read/read-all trả unread count mới từ database. Payload realtime có count snapshot; khi event đến trễ hoặc sai thứ tự, client phải refetch count, không tự cộng trừ hoặc coi snapshot cũ là trạng thái mới nhất.

## Cấu hình worker cho Phase 6

Phase 6 mới đã thay HTTP gateway bằng SignalR publisher trong backend. `SocialOutbox:Enabled` vẫn mặc định false để bật chủ động; REST luôn lưu Notification/Outbox bền vững:

```text
SocialOutbox__Enabled=true
```

Worker gọi `IRealtimeEventPublisher`, implementation dùng `IHubContext<NotificationHub>`. JSON `{ room, envelope }` cũ vẫn dùng được. Không còn GatewayUrl/ServiceToken. `ProcessedAtUtc` ghi sau publish thành công, không phải browser receipt. Crash sau send trước commit có thể gửi trùng cùng EventId; client dedupe/refetch. Phần “Đã triển khai” phía trên mô tả lịch sử Phase 4–5 trước thay đổi transport này.

Mặc định: poll 2 giây, batch 20, timeout 10 giây, tối đa 8 lần thử; backoff từ 2 đến 300 giây. Message đã dead-letter được giữ để điều tra; chưa có REST endpoint replay/xóa. Không tự xóa notification hoặc outbox trong giai đoạn này.

RateLimits có key `social-like`, `social-save`, `social-comment`, `social-edit`, `social-rsvp`, `social-read`; giá trị là số request/phút/user. Limiter hiện ở từng API instance, cần shared limiter nếu triển khai nhiều API instance.

## Kiểm chứng

- Kết quả: 380 unit tests pass; 124 integration tests pass với PostgreSQL. Sau chỉnh validation cuối, chạy lại 9 tests Social đều pass. EF `has-pending-model-changes` xác nhận model khớp migration.
- Migration đã được chạy trên database test riêng; chưa apply vào database ứng dụng đang chạy.
- API tests: cooldown, nhiều tác giả/self-like, private notification/saved, reply/moderation/cursor, stale RSVP/audience/withdraw/cancel, Swagger và 429.
- PostgreSQL tests trên database tạm: hai like đồng thời, rollback khi outbox insert lỗi, hai worker tranh cùng message, migration và regression Sharing.
- Unit tests: retry/backoff/dead-letter và Markdown validation.
- Chưa có test delivery Socket.IO end-to-end: gateway và frontend thuộc Phase 6–8.

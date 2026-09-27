# Sprint 5 — Quy trình Social Interaction

## 1. Like

```text
User bấm Like
→ PUT REST API
→ Validate quyền và Content Published
→ Insert ContentLike nếu chưa có
→ Nếu đã like: trả trạng thái hiện tại, không tạo event/notification mới
→ Với từng tác giả hợp lệ, kiểm tra notification ContentLiked cùng actor/content/recipient trong 5 phút
→ Chỉ khi ngoài cửa sổ 5 phút: tạo Notification có EventId riêng và OutboxMessage tương ứng
→ Ghi OutboxMessage content.interaction.updated khi trạng thái Like thực sự thay đổi
→ Commit toàn bộ Like, Notification và OutboxMessage trong cùng transaction
→ Trả likeCount mới
→ Worker phát event qua Socket.IO
```

Unlike:

```text
User bấm Unlike
→ DELETE REST API
→ Xóa ContentLike nếu tồn tại
→ Ghi OutboxMessage content.interaction.updated trong cùng transaction nếu có thay đổi
→ Commit
→ Không tạo Notification
→ Phát content.interaction.updated
```

Không dùng endpoint toggle vì retry request có thể đảo trạng thái ngoài ý muốn.

Like lại trong cửa sổ 5 phút vẫn trả trạng thái Like thành công và `likeCount` mới, nhưng không tạo notification. Cửa sổ được tính độc lập cho từng recipient, từ `CreatedAtUtc` của notification `ContentLiked` gần nhất.

## 2. Comment

```text
User gửi comment
→ Validate Content Published
→ Validate độ dài và rate limit
→ Lưu ContentComment
→ Tạo Notification + Outbox
→ Commit
→ Phát comment.created và notification.created
```

## 3. Reply

- Parent phải là comment gốc.
- Parent phải thuộc cùng Content.
- Không reply vào comment Deleted hoặc Hidden.
- Tác giả comment gốc nhận `CommentReplied` nếu không phải chính người reply.
- Tác giả bài cũng có thể nhận `ContentCommented`; hệ thống loại trùng recipient trong cùng event nghiệp vụ.

## 4. Sửa comment

- Chỉ tác giả comment được sửa nội dung. Admin dùng hide/restore để moderation.
- Request gửi `Version` hiện tại.
- Sửa thành công cập nhật `UpdatedAtUtc`.
- Event `comment.updated` được phát đến `content:{contentId}`.
- Không tạo notification mới khi chỉ sửa nội dung.

## 5. Xóa và moderation

- User xóa comment của mình: Status thành Deleted.
- Admin hide: Status thành Hidden và bắt buộc lý do.
- Nội dung gốc không trả cho Member sau khi Deleted/Hidden.
- Reply vẫn giữ cấu trúc hội thoại.
- Phát `comment.deleted` hoặc `comment.hidden` để client cập nhật.

## 6. Saved Content

```text
Save   → PUT /saved   → insert nếu chưa có
Unsave → DELETE      → delete nếu tồn tại
```

Saved không tạo notification và không cần broadcast cho người khác. Client cập nhật local state từ REST response.

## 7. Đồng bộ số lượng

- `likeCount` và `commentCount` lấy từ database hoặc projection đáng tin cậy.
- Client có thể cập nhật tức thời theo event nhưng phải refetch khi reconnect.
- Không cộng/trừ count nhiều lần nếu nhận trùng `eventId`.

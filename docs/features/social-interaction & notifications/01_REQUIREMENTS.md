# Sprint 5 — Yêu cầu nghiệp vụ

## 1. Vai trò

### Member

Member có thể:

- Like hoặc unlike bài Sharing Content đã Published.
- Xem số lượt like.
- Tạo comment và reply comment một cấp.
- Sửa hoặc xóa comment của mình.
- Lưu hoặc bỏ lưu bài viết.
- Xem danh sách bài đã lưu.
- Xác nhận Going, Maybe hoặc NotGoing cho lịch sharing được phép xem.
- Thay đổi hoặc rút xác nhận trước khi buổi sharing bắt đầu.
- Xem notification của mình.
- Đánh dấu một hoặc tất cả notification là đã đọc.
- Nhận notification realtime trên các tab hoặc thiết bị đang đăng nhập.

### Admin

Admin có toàn bộ quyền của Member và có thể:

- Ẩn hoặc khôi phục comment vi phạm.
- Xem danh sách RSVP của một lịch.
- Xem thống kê Going, Maybe và NotGoing.
- Gửi notification hệ thống trong phạm vi được phép nếu được triển khai sau.
- Không được đọc notification riêng của người dùng khác.

## 2. Quy tắc Like

- Chỉ content `Published` mới được like.
- Mỗi User chỉ có tối đa một Like trên một Content.
- Like và unlike phải idempotent.
- Người dùng không nhận notification khi tự like bài của mình.
- Khi một người like, tất cả tác giả đang hoạt động của bài nhận notification, trừ chính người like.
- Like lại sau khi đã unlike chỉ tạo notification nếu notification `ContentLiked` gần nhất của cùng actor, content và recipient đã cách ít nhất 5 phút. Trong cửa sổ 5 phút, Like vẫn thành công nhưng không tạo Notification hoặc OutboxMessage `notification.created` mới; event cập nhật số like vẫn được ghi. Không dời mốc 5 phút khi một re-like bị chặn notification, không gửi notification trì hoãn khi hết cửa sổ.
- Unlike không tạo notification và không xóa notification đã được phát trước đó.
- Like của bài bị Archived không bị xóa nhưng không còn tương tác được.

## 3. Quy tắc Comment

- Chỉ content `Published` mới nhận comment.
- Nội dung comment bắt buộc, sau trim từ 1 đến 2.000 ký tự.
- Hỗ trợ comment gốc và reply một cấp.
- Reply phải trỏ đến comment gốc thuộc cùng Content.
- Không cho phép reply vào reply.
- Tác giả comment được sửa khi comment còn Active.
- Xóa comment là xóa mềm; hệ thống giữ lịch sử và hiển thị “Bình luận đã bị xóa”.
- Khi xóa comment gốc, các reply vẫn được giữ.
- Admin có thể chuyển comment sang Hidden và ghi lý do kiểm duyệt.
- Tác giả bài nhận notification khi có comment mới, trừ người comment.
- Tác giả comment gốc nhận notification khi có reply, trừ người reply.

## 4. Quy tắc Saved Content

- Saved là dữ liệu riêng tư của từng User.
- Mỗi User chỉ lưu một Content một lần.
- Save và unsave phải idempotent.
- Save không tạo notification cho tác giả.
- Content Archived vẫn giữ bản ghi Saved nhưng không còn mở được đối với Member.
- Danh sách Saved chỉ trả về Content người dùng hiện có quyền xem.

## 5. Quy tắc RSVP

Các trạng thái:

```text
Going
Maybe
NotGoing
```

Quy tắc:

- Chỉ Schedule `Scheduled` và chưa bắt đầu mới nhận RSVP.
- User phải thuộc audience của lịch hoặc là Presenter/Admin.
- Mỗi User chỉ có một RSVP trên một Schedule.
- PUT RSVP tạo mới hoặc thay đổi trạng thái.
- User được rút RSVP trước giờ bắt đầu.
- Schedule Cancelled, Ongoing hoặc Completed khóa RSVP.
- Hủy lịch không xóa RSVP cũ.
- Sprint này không dùng RSVP làm điểm danh.

## 6. Quy tắc Notification

- Notification phải được lưu trong database trước khi phát realtime.
- Notification thuộc riêng một Recipient.
- Người dùng chỉ xem và cập nhật notification của chính mình.
- Notification hỗ trợ trạng thái unread/read.
- Notification có route đến entity liên quan.
- Xóa entity không được làm API notification lỗi; UI hiển thị nội dung không còn khả dụng.
- Số lượng chưa đọc lấy từ database, không chỉ dựa vào state trong trình duyệt.
- Một notification thuộc đúng một Recipient và có `EventId` UUID riêng. Cùng `EventId` phải được tái sử dụng cho mọi lần retry của notification và realtime event tương ứng; không dùng chung `EventId` cho nhiều recipient.

## 7. Loại Notification trong Sprint 5

```text
ContentLiked
ContentCommented
CommentReplied
ScheduleUpdated
ScheduleCancelled
```

`ContentLiked` là yêu cầu bắt buộc. Các loại còn lại dùng chung nền tảng notification trong cùng sprint.

## 8. Phân quyền tổng hợp

| Chức năng                   |  Member  |  Comment Author  |        Admin        |
| --------------------------- | :------: | :--------------: | :-----------------: |
| Like/Save content Published |    Có    |        Có        |         Có          |
| Tạo comment                 |    Có    |        Có        |         Có          |
| Sửa/xóa comment             |  Không   | Comment của mình | Có quyền moderation |
| RSVP lịch phù hợp audience  |    Có    |        Có        |         Có          |
| Xem notification            | Của mình |     Của mình     |      Của mình       |
| Xem danh sách RSVP          |  Không   |      Không       |         Có          |
| Ẩn/khôi phục comment        |  Không   |      Không       |         Có          |

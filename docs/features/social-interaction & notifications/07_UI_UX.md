# Sprint 5 — UI/UX

## 1. Tương tác bài viết

Trang Sharing Content hiển thị:

- Nút Like và tổng số lượt like.
- Nút Comment và tổng số comment.
- Nút Saved riêng cho current user.
- Trạng thái nút thay đổi ngay sau REST response.
- Khi nhận Socket.IO event, số lượng được đồng bộ mà không tải lại trang.

Không optimistic toggle trước khi server phản hồi nếu chưa có cơ chế rollback rõ ràng.

## 2. Khu vực Comment

- Form comment nằm dưới bài viết.
- Hiển thị comment theo cũ nhất hoặc mới nhất.
- Reply thụt vào một cấp.
- Hiện “đã chỉnh sửa” khi có UpdatedAtUtc.
- Comment Deleted hiển thị placeholder.
- Comment Hidden chỉ Admin thấy lý do moderation.
- Menu sửa/xóa chỉ hiện đúng quyền.
- Tải thêm bằng cursor, không tải toàn bộ comment một lần.

## 3. Saved Content

Trang “Bài viết đã lưu” hiển thị:

- Card bài viết.
- Ngày lưu.
- Nút bỏ lưu.
- Empty state khi chưa lưu bài nào.

## 4. RSVP trên chi tiết lịch

Hiển thị ba lựa chọn:

```text
Tham gia
Có thể tham gia
Không tham gia
```

- Nút đang chọn có trạng thái rõ ràng.
- Cho phép đổi lựa chọn trước khi lịch bắt đầu.
- Có hành động “Rút phản hồi”.
- Khóa form khi lịch đã bắt đầu, hoàn tất hoặc hủy.
- Hiển thị lỗi nếu user không còn thuộc audience.

## 5. Notification Center

### Notification bell

- Hiển thị badge unread count.
- Badge `3+` khi lớn hơn 3.
- Realtime event cập nhật badge ngay.
- Bấm mở popover các notification gần nhất.

### Trang notification

- Danh sách mới nhất trước.
- Phân biệt đã đọc/chưa đọc.
- Lọc All và Unread.
- Đánh dấu từng notification hoặc tất cả là đã đọc.
- Bấm notification điều hướng theo `Route` và đánh dấu đã đọc.

## 6. Realtime status

- Không cần hiển thị socket connected liên tục cho người dùng.
- Khi reconnect thất bại kéo dài, hiển thị trạng thái “Đang đồng bộ lại”.
- Sau reconnect, refetch dữ liệu chính.
- Không hiển thị toast cho từng like để tránh gây nhiễu; badge và notification center là mặc định.

## 7. Frontend state

- TanStack Query quản lý interaction, comment, RSVP và notification từ server.
- Zustand chỉ giữ UI state như notification panel đang mở.
- Socket listener invalidate hoặc cập nhật Query Cache.
- Mỗi listener phải được `off` khi provider unmount để tránh nhận trùng event.
- Chỉ tạo một Socket.IO connection dùng chung trong authenticated app shell.

## 8. Accessibility

- Nút Like, Saved và RSVP có `aria-pressed` hoặc trạng thái tương đương.
- Badge notification có nhãn đọc được.
- Comment form có label và thông báo lỗi.
- Notification mới không tự cướp focus.
- Màu chưa đọc luôn đi kèm kiểu chữ hoặc dấu hiệu khác.

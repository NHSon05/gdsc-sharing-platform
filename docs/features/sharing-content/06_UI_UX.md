# Sprint 4 — UI/UX

## 1. Trang Sharing Content

Hiển thị:

- Danh sách dạng card hoặc list.
- Tiêu đề, tóm tắt, ảnh bìa, tác giả, tag và ngày công khai.
- Tìm kiếm theo tiêu đề và nội dung tóm tắt.
- Lọc theo tag và tác giả.
- Phân trang hoặc tải thêm có kiểm soát.

## 2. Trang chi tiết Content

Hiển thị:

- Tiêu đề và tóm tắt.
- Owner và Contributor.
- Nội dung Markdown đã render an toàn.
- Danh sách tài liệu link và file.
- Các lịch sharing liên quan.
- Roadmap node liên quan nếu tích hợp Sprint 3.

## 3. Khu vực “Nội dung của tôi”

Các tab:

```text
Draft
Pending Review
Published
Rejected
Archived
```

Chức năng:

- Tạo bài mới.
- Lưu bản nháp.
- Thêm Contributor và Tag.
- Upload hoặc sắp xếp tài liệu.
- Gửi duyệt.
- Xem ReviewNote.
- Rút yêu cầu duyệt khi còn hợp lệ.

## 4. Trang duyệt nội dung của Admin

- Danh sách PendingReview ưu tiên theo thời gian gửi.
- Xem trước Markdown và tài liệu.
- Hiển thị lịch sử xử lý.
- Approve hoặc Reject.
- Reject mở hộp thoại bắt buộc nhập lý do.

## 5. Trang Sharing Schedule

Có hai chế độ:

- `List View`: Dễ đọc trên mobile, nhóm theo ngày hoặc tháng.
- `Calendar View`: Tháng hoặc tuần, bấm lịch để mở chi tiết.

Mỗi lịch hiển thị:

- Tiêu đề.
- Thời gian.
- Hình thức Online, Offline hoặc Hybrid.
- Địa điểm hoặc biểu tượng họp trực tuyến.
- Người trình bày.
- Trạng thái.

## 6. Chi tiết lịch

- Thời gian và múi giờ.
- Địa điểm hoặc link họp.
- Danh sách Presenter cùng vai trò.
- Nội dung liên quan.
- Đối tượng được mời ở mức mô tả, không công khai danh sách thành viên.
- Lý do hủy nếu lịch Cancelled.

## 7. Admin quản lý lịch

- Form tạo/sửa lịch cho Admin và Content Owner trong phạm vi được phép.
- Bộ chọn ngày, giờ và múi giờ.
- Chọn Delivery Mode và hiển thị field theo điều kiện.
- Tìm và chọn nhiều Presenter (Admin); Member tự tạo lịch sẽ mặc định là Presenter của lịch đó.
- Chọn Content đã có.
- Chọn AllMembers hoặc Gen/Department.
- Cảnh báo trùng lịch trước khi lưu.
- Hộp thoại xác nhận khi publish, complete hoặc cancel.

## 8. Trạng thái giao diện

- Loading skeleton cho danh sách và chi tiết.
- Empty state riêng cho content, lịch và kết quả tìm kiếm.
- Lỗi validation hiển thị cạnh field.
- Lỗi concurrency yêu cầu tải lại bản mới trước khi tiếp tục.
- Không tự retry mutation.

## 9. Khả năng truy cập

- Form có label rõ ràng.
- Calendar có chế độ list thay thế cho bàn phím và screen reader.
- Modal quản lý focus đúng.
- Màu trạng thái luôn đi kèm chữ hoặc icon.
- Link họp không hiển thị cho người không thuộc audience.

# Sprint 4 — Yêu cầu nghiệp vụ

## 1. Vai trò

### Member

Member có thể:

- Xem nội dung đã được công khai.
- Tìm kiếm và lọc nội dung theo tác giả, tag hoặc chủ đề.
- Tạo bài chia sẻ mới ở trạng thái Draft.
- Chỉnh sửa bài mà mình là Owner ở mọi trạng thái được phép, bao gồm Draft, Rejected và Published.
- Thêm đồng tác giả vào bài chia sẻ.
- Đính kèm liên kết hoặc tệp.
- Gửi bài cho Admin duyệt.
- Xem lý do từ chối và chỉnh sửa để gửi lại.
- Xem các lịch sharing mà mình thuộc đối tượng được phép xem.
- Xem vai trò của mình trong buổi sharing nếu được phân công.
- Tạo lịch sharing và mặc định được gán làm Presenter của chính lịch đó.
- Chỉnh sửa lịch sharing do mình tạo trong phạm vi được phép.

Member không được:

- Chỉ định người trình bày.
- Duyệt nội dung của chính mình hoặc của thành viên khác.

### Admin

Admin có thể:

- Xem và quản lý toàn bộ bài chia sẻ.
- Duyệt hoặc từ chối bài đang PendingReview.
- Công khai, ẩn hoặc lưu trữ nội dung.
- Quản lý tag và tác giả.
- Tạo, cập nhật, lên lịch, hoàn tất hoặc hủy lịch sharing.
- Chỉ định nhiều người trình bày và vai trò của từng người.
- Liên kết nhiều bài Sharing Content với một lịch.
- Chọn đối tượng xem lịch theo toàn câu lạc bộ, Gen hoặc Department.

## 2. Trạng thái Sharing Content

| Trạng thái      | Ý nghĩa                                              |
| --------------- | ---------------------------------------------------- |
| `Draft`         | Bản nháp, chỉ Owner và Admin được xem                |
| `PendingReview` | Đã gửi duyệt, tạm khóa chỉnh sửa                     |
| `Published`     | Đã được công khai cho thành viên                     |
| `Rejected`      | Bị từ chối, có lý do và được phép chỉnh sửa          |
| `Archived`      | Đã lưu trữ, không còn hiển thị trong danh sách chính |

## 3. Trạng thái Sharing Schedule

| Trạng thái  | Ý nghĩa                                      |
| ----------- | -------------------------------------------- |
| `Draft`     | Lịch nháp, người tạo, Presenter và Admin xem |
| `Scheduled` | Đã công bố lịch                              |
| `Ongoing`   | Buổi sharing đang diễn ra                    |
| `Completed` | Đã hoàn tất                                  |
| `Cancelled` | Đã hủy và phải có lý do                      |

## 4. Quy tắc nội dung

- Tiêu đề bắt buộc, tối đa 200 ký tự.
- Slug duy nhất toàn hệ thống.
- Nội dung chính lưu dưới dạng Markdown; không lưu HTML chưa được làm sạch.
- Mỗi bài có ít nhất một tác giả với vai trò `Owner`.
- Một bài chỉ có một Owner nhưng có thể có nhiều Contributor.
- Contributor được sửa nội dung và tài liệu khi Rejected nhưng không được quản lý danh sách tác giả, tag hay trạng thái. Draft chỉ Owner/Admin truy cập.
- Chỉ Owner được gửi duyệt hoặc rút bài khỏi hàng chờ duyệt.
- Bài PendingReview không được sửa nội dung, tác giả hoặc tài liệu.
- Owner được sửa toàn bộ bài Published. Các thay đổi phải kiểm tra optimistic concurrency và ghi audit log.
- Rejected phải có `ReviewNote`.
- Published phải lưu người duyệt và thời điểm công khai.
- Archived không bị xóa cứng.

## 5. Quy tắc lịch sharing

- Member và Admin có thể tạo lịch. Member tạo lịch sẽ tự động được gán làm Presenter của chính lịch đó.
- Content Owner có thể tạo và sửa lịch sharing trong phạm vi lịch do mình quản lý.
- Admin có thể tạo, sửa toàn bộ lịch và chọn Member để gán làm Presenter.
- `EndsAtUtc` phải lớn hơn `StartsAtUtc`.
- Lưu thời gian bằng UTC và lưu thêm `TimeZoneId` để hiển thị đúng múi giờ.
- Lịch Online bắt buộc có `MeetingUrl`.
- Lịch Offline bắt buộc có `Location`.
- Lịch Hybrid bắt buộc có cả `MeetingUrl` và `Location`.
- Lịch Scheduled phải có ít nhất một Presenter đang hoạt động.
- Một người không được có hai lịch Scheduled/Ongoing bị trùng thời gian.
- Một lịch có thể liên kết nhiều bài chia sẻ; Draft chỉ Owner/Admin xem, các trạng thái chưa Published khác chỉ Admin và tác giả được xem.
- Lịch Cancelled phải có lý do và không được chuyển sang Ongoing hoặc Completed.
- Lịch Completed không được chỉnh sửa thời gian hoặc người trình bày.

## 6. Đối tượng xem lịch

Mỗi lịch sử dụng một trong hai phạm vi:

- `AllMembers`: Tất cả thành viên đang hoạt động được xem.
- `SelectedAudience`: Membership đang hoạt động khớp ít nhất một Gen hoặc Department được chọn.

Quy tắc:

- Người tạo, Admin và Presenter luôn xem được lịch, kể cả khi không thuộc đối tượng.

## 7. Phân quyền tổng hợp

| Chức năng                 |    Member     | Content Owner | Admin |
| ------------------------- | :-----------: | :-----------: | :---: |
| Xem content Published     |      Có       |      Có       |  Có   |
| Xem content Draft         |     Không     | Bài của mình  |  Có   |
| Tạo content               |      Có       |      Có       |  Có   |
| Gửi duyệt                 |     Không     |      Có       |  Có   |
| Duyệt/từ chối             |     Không     |     Không     |  Có   |
| Xem lịch phù hợp audience |      Có       |      Có       |  Có   |
| Tạo/sửa lịch              | Lịch của mình |      Có       |  Có   |
| Phân công presenter       |     Không     |     Không     |  Có   |

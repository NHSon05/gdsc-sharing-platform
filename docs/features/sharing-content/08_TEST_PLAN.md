# Sprint 4 — Test Plan

## 1. Acceptance Criteria

| ID | Tiêu chí nghiệm thu |
|---|---|
| AC-01 | Member tạo được Sharing Content ở trạng thái Draft. |
| AC-02 | Người tạo tự động trở thành Owner. |
| AC-03 | Một content hỗ trợ nhiều Contributor nhưng chỉ một Owner. |
| AC-04 | Owner chỉnh sửa được Draft và Rejected. |
| AC-05 | PendingReview khóa việc chỉnh sửa và quản lý tệp. |
| AC-06 | Owner gửi duyệt và rút yêu cầu duyệt khi hợp lệ. |
| AC-07 | Admin approve content thành Published. |
| AC-08 | Admin reject phải nhập ReviewNote. |
| AC-09 | Draft chỉ Owner và Admin được xem; các trạng thái khác tuân thủ quyền xem tương ứng. |
| AC-10 | Link và file tuân thủ chính sách bảo mật. |
| AC-11 | Member và Admin tạo được lịch Draft; Member tự tạo được gán làm Presenter. |
| AC-12 | Scheduled bắt buộc có ít nhất một Presenter. |
| AC-13 | Delivery Mode kiểm tra đúng Location và MeetingUrl. |
| AC-14 | Presenter bị trùng thời gian nhận 409 Conflict. |
| AC-15 | Một lịch liên kết được nhiều content. |
| AC-16 | Member chỉ thấy content Published trong chi tiết lịch. |
| AC-17 | AllMembers hiển thị lịch cho mọi thành viên hoạt động. |
| AC-18 | SelectedAudience lọc đúng theo Gen hoặc Department. |
| AC-19 | Admin và Presenter luôn xem được lịch liên quan. |
| AC-20 | Cancel schedule bắt buộc có lý do. |
| AC-21 | Completed và Cancelled giữ nguyên lịch sử. |
| AC-22 | Member gọi Admin API nhận 403. |
| AC-23 | Version cũ khi cập nhật trả 412. |
| AC-24 | Migration và seeder chạy lặp lại an toàn. |

## 2. Unit Test

### Sharing Content

- Content mới là Draft.
- Owner tự động được tạo.
- Không thể có hai Owner.
- Chỉ cho phép transition hợp lệ.
- Reject thiếu lý do bị từ chối.
- Link và File loại trừ lẫn nhau.

### Sharing Schedule

- End phải sau Start.
- Kiểm tra field theo Delivery Mode.
- Scheduled yêu cầu Presenter.
- Overlap sử dụng đúng công thức thời gian.
- Cancelled không chuyển sang Completed.
- SelectedAudience yêu cầu Gen hoặc Department.

## 3. Integration Test

- Member CRUD Draft của chính mình.
- Member không sửa content của người khác.
- Submit, withdraw, approve và reject đúng trạng thái.
- Member không xem Draft của người khác.
- Member xem và cập nhật được lịch do mình tạo trong phạm vi được phép.
- Admin CRUD Tag.
- Upload và download theo đúng quyền.
- Admin CRUD Schedule.
- Admin gán được Member làm Presenter; Member không thể gán người khác.
- Lọc lịch theo khoảng thời gian và audience.
- Trùng lịch Presenter trả 409.
- Cập nhật với version cũ trả 412.
- Audit log được tạo cho hành động quan trọng.

## 4. File Test

- Upload tệp hợp lệ.
- Tệp quá lớn trả 413.
- Extension không hỗ trợ trả 415.
- MIME hoặc signature sai bị từ chối.
- Database lỗi sau upload phải dọn file mới.
- Replace lỗi phải giữ file cũ.

## 5. Regression Test

- Authentication và refresh rotation hoạt động.
- Profile nhiều Gen/Department/Role hoạt động.
- Roadmap node–edge hoạt động.
- Migration Sprint 4 chạy sau các migration trước.
- Không thay đổi contract API cũ ngoài phạm vi đã công bố.

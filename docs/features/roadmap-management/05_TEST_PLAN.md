# Sprint 3 — Test Plan Roadmap

## 1. Acceptance Criteria

| ID    | Tiêu chí nghiệm thu                                              |
| ----- | ---------------------------------------------------------------- |
| AC-01 | Member xem được danh sách roadmap Published.                     |
| AC-02 | Member không xem được roadmap Draft; Archived vẫn xem được.               |
| AC-03 | Member tìm kiếm được roadmap theo tên.                           |
| AC-04 | Member lọc được roadmap theo danh mục và mức độ.                 |
| AC-05 | Chi tiết roadmap trả đúng node, tọa độ và đường nối.             |
| AC-06 | Bấm vào node hiển thị đúng nội dung và tài liệu.                 |
| AC-07 | Member mở được tài liệu dạng liên kết hợp lệ.                    |
| AC-08 | Member tải được tài liệu dạng tệp khi có quyền.                  |
| AC-09 | Admin tạo và cập nhật được roadmap.                              |
| AC-10 | Admin tạo được nhiều node trong một roadmap.                     |
| AC-11 | Admin kéo thả và lưu được vị trí node.                           |
| AC-12 | Admin tạo được đường nối và thêm tài liệu vào node.              |
| AC-13 | Hệ thống hỗ trợ cả Link và File.                                 |
| AC-14 | URL không hợp lệ hoặc nguy hiểm bị từ chối.                      |
| AC-15 | Tệp vượt dung lượng trả về 413.                                  |
| AC-16 | Định dạng tệp không hỗ trợ trả về 415.                           |
| AC-17 | Member gọi API quản trị nhận 403.                                |
| AC-18 | Chuyển sang Published cần node active (ngoại lệ: tạo mới mặc định Published).             |
| AC-19 | Slug roadmap không được trùng toàn hệ thống.                     |
| AC-20 | Không tạo được self-loop, edge trùng hoặc Required cycle.        |
| AC-21 | Node bị vô hiệu hóa và edge liên quan không hiển thị cho Member. |
| AC-22 | Thay tệp thất bại không làm mất tệp cũ.                          |
| AC-23 | Database lỗi sau upload phải dọn tệp mới.                        |
| AC-24 | Roadmap Archived vẫn giữ node, edge và tài liệu trong database.  |

## 2. Unit Test

### Domain

- Tạo Roadmap với dữ liệu hợp lệ.
- Roadmap mới mặc định Published và có PublishedAtUtc.
- Không tạo được Roadmap với title rỗng.
- Không publish Roadmap không có node hoạt động.
- Archive Roadmap giữ nguyên dữ liệu.
- Node kiểm tra tọa độ hợp lệ.
- Edge từ chối self-loop và Required cycle.
- LearningResource bảo đảm Link và File loại trừ lẫn nhau.

### Validator

- Kiểm tra độ dài title và slug.
- Kiểm tra enum Level và Status.
- Kiểm tra URL HTTP/HTTPS.
- Kiểm tra `ExternalUrl` bắt buộc cho Link.
- Kiểm tra metadata tệp bắt buộc cho File.
- Kiểm tra request reorder không chứa ID trùng.

## 3. Integration Test

### Member API

- Danh sách chỉ trả về Published và Archived.
- Search và filter trả đúng dữ liệu.
- Chi tiết roadmap trả đủ node và edge đang hoạt động.
- Không truy cập được node không hoạt động.
- Không tải được tài liệu thuộc Draft; Archived cho tải nếu node và tài liệu active.

### Admin API

- Admin CRUD được Category.
- Admin tạo, sửa và đổi trạng thái Roadmap.
- Publish thất bại khi chưa có node hoạt động.
- Trùng slug trả 409.
- Lưu vị trí nhiều node trong một transaction.
- Edge không được nối các node thuộc hai roadmap khác nhau.
- Member gọi endpoint Admin trả 403.

### File API

- Upload tệp hợp lệ thành công.
- Tệp quá lớn trả 413.
- Extension không được phép trả 415.
- MIME type hoặc signature không khớp bị từ chối.
- Download trả đúng tên tệp và Content-Type.
- Database lỗi sau upload làm tệp mới được dọn.
- Replace lỗi giữ nguyên tệp cũ.

## 4. Regression Test

- Authentication vẫn hoạt động.
- Refresh-token rotation không bị ảnh hưởng.
- Profile và membership của Sprint 2 vẫn hoạt động.
- Migration mới chạy được sau migration Sprint 2.
- Seeder chạy nhiều lần không tạo dữ liệu trùng.

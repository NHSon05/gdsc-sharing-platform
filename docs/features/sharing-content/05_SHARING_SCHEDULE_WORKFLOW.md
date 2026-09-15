# Sprint 4 — Quy trình Sharing Schedule

## 1. Luồng chính

```text
Member hoặc Admin tạo lịch
→ Draft
→ Chọn thời gian, hình thức và audience
→ Member: tự gán làm Speaker; Admin: chọn Presenter
→ Liên kết Sharing Content
→ Publish
→ Scheduled
→ Start
→ Ongoing
→ Complete
→ Completed
```

## 2. Luồng hủy

```text
Draft hoặc Scheduled
→ Admin Cancel kèm lý do
→ Cancelled
```

Không xóa lịch đã công bố. Việc hủy phải giữ nguyên người trình bày, thời gian và nội dung để truy vết.

## 3. Vai trò người trình bày

```text
Host
Speaker
CoSpeaker
Facilitator
```

Một thành viên có thể giữ nhiều vai trò nếu cần, nhưng không được có cùng một vai trò lặp lại trong một lịch.

## 4. Kiểm tra trùng lịch

Khi thêm Presenter hoặc thay đổi thời gian, backend kiểm tra các lịch `Scheduled` và `Ongoing` của người đó.

Hai khoảng thời gian bị trùng khi:

```text
newStart < existingEnd AND newEnd > existingStart
```

Nếu trùng, trả `409 Conflict` cùng thông tin lịch gây xung đột. Lịch Cancelled không tham gia kiểm tra.

## 5. Audience

### AllMembers

- Tất cả thành viên đăng nhập và đang hoạt động được xem.
- Không cần lưu Gen hoặc Department đích.

### SelectedAudience

- Chọn một hoặc nhiều Gen.
- Chọn một hoặc nhiều Department.
- Thành viên được xem nếu có membership đang hoạt động khớp ít nhất một điều kiện.
- Creator, Admin và Presenter luôn được xem lịch; audience chỉ áp dụng cho lịch đã công bố.

## 6. Hiển thị thời gian

- Database lưu `StartsAtUtc` và `EndsAtUtc`.
- Request quản trị gửi thời gian địa phương cùng `TimeZoneId`.
- Backend chuyển đổi sang UTC và từ chối thời gian không tồn tại hoặc nhập nhằng do daylight saving.
- Client hiển thị theo múi giờ của lịch và có thể kèm múi giờ người dùng nếu khác nhau.

## 7. Liên kết nội dung

- Một lịch có thể có nhiều Sharing Content.
- Nội dung được sắp xếp bằng `SortOrder`.
- Member chỉ thấy liên kết đến content Published.
- Content Draft chỉ Owner/Admin xem; PendingReview và các trạng thái chưa Published khác chỉ Admin và tác giả xem.
- Lịch vẫn có thể tạo khi chưa có content, nhưng cần ít nhất một Presenter trước khi publish.

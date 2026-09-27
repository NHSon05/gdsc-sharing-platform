# Sprint 5 — Quy trình RSVP

## 1. Xác nhận tham gia

```text
User mở Schedule
→ Hệ thống kiểm tra audience
→ User chọn Going / Maybe / NotGoing
→ PUT RSVP
→ Upsert ScheduleRsvp
→ Commit
→ Trả trạng thái mới
→ Phát schedule.rsvp.updated đến room quản trị của lịch
```

## 2. Rút xác nhận

```text
DELETE /api/sharing/schedules/{scheduleId}/rsvp
```

- Chỉ chủ sở hữu RSVP được rút.
- Chỉ thực hiện trước `StartsAtUtc`.
- Xóa RSVP không đồng nghĩa NotGoing.
- Sau khi rút, user được tính là chưa phản hồi.

## 3. Thay đổi trạng thái

PUT là upsert idempotent:

- Chưa có RSVP: tạo mới.
- Đã có trạng thái khác: cập nhật.
- Đã có đúng trạng thái: trả kết quả hiện tại, không tạo dữ liệu trùng.

## 4. Quyền xem

- Member chỉ RSVP lịch mình được xem theo Sprint 4 audience policy.
- Presenter được RSVP nhưng không bắt buộc.
- Admin được xem danh sách và summary.
- Member không xem danh sách RSVP của người khác trong Sprint 5.

## 5. Khi lịch thay đổi

Nếu lịch Scheduled thay đổi thời gian, địa điểm hoặc Meeting URL:

- Lưu thay đổi lịch.
- Tạo notification `ScheduleUpdated` cho người Going/Maybe và Presenter.
- Phát notification qua user room.
- Không tự thay đổi trạng thái RSVP.

Nếu lịch bị hủy:

- Giữ nguyên RSVP để truy vết.
- Tạo `ScheduleCancelled` cho người Going/Maybe và Presenter.
- UI khóa toàn bộ thao tác RSVP.

## 6. Thống kê

Admin xem:

- Tổng Going.
- Tổng Maybe.
- Tổng NotGoing.
- Số thành viên trong audience chưa phản hồi.

`NoResponse` chỉ chính xác khi audience có thể xác định thành danh sách thành viên tại thời điểm truy vấn.


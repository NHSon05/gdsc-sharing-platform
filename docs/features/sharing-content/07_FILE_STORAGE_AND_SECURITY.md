# Sprint 4 — File Storage và Security

## 1. Nguyên tắc dùng chung

Sharing Content phải tái sử dụng abstraction `IFileStorage` và chính sách kiểm tra tệp đã có từ Sprint 3. Không tạo thêm cơ chế upload riêng thiếu kiểm soát.

## 2. Định dạng và dung lượng

Định dạng đề xuất:

```text
.pdf
.doc
.docx
.ppt
.pptx
.xls
.xlsx
.txt
.md
.zip
```

- Dung lượng tối đa mặc định: 20 MB mỗi tệp.
- Kiểm tra extension, MIME type và file signature khi phù hợp.
- Tên lưu trữ phải do hệ thống sinh.
- Không trả đường dẫn vật lý thật cho client.

## 3. Quyền quản lý tệp

- Owner quản lý tệp khi Content là Draft hoặc Rejected.
- Contributor chỉ quản lý tệp nếu nghiệp vụ cho phép chỉnh nội dung.
- PendingReview khóa upload, replace và delete.
- Owner được quản lý tệp của Published; thao tác phải kiểm tra quyền, optimistic concurrency và ghi audit log.
- Download tệp Draft chỉ dành cho Owner/Admin; PendingReview chỉ dành cho tác giả và Admin.
- Download tệp Published dành cho Member đã đăng nhập.

## 4. Transaction tệp

Upload mới:

```text
Validate
→ Save file
→ Save metadata
→ Database lỗi: xóa file mới
```

Replace:

```text
Validate file mới
→ Save file mới
→ Update database
→ Thành công: xóa file cũ
→ Thất bại: xóa file mới, giữ file cũ
```

## 5. Bảo mật URL

- Chỉ chấp nhận HTTP và HTTPS.
- Chặn `javascript:`, `data:`, `file:` và scheme không được phép.
- Link họp chỉ trả cho người thuộc audience, Presenter hoặc Admin.
- Không log meeting URL chứa token bí mật.

## 6. Markdown và XSS

- Lưu Markdown gốc.
- Render bằng thư viện an toàn.
- Không bật raw HTML nếu chưa có sanitizer được phê duyệt.
- Loại bỏ script, event handler và URL nguy hiểm.
- Giới hạn độ dài nội dung để tránh payload quá lớn.

## 7. Audit

Ghi audit log cho:

- Submit, withdraw, approve, reject và archive content.
- Thêm hoặc xóa tác giả.
- Upload, replace hoặc vô hiệu hóa tệp.
- Publish, start, complete hoặc cancel schedule.
- Thay đổi Presenter, audience và thời gian.

Audit gồm actor, action, entity, entity ID, timestamp, trace ID và metadata không nhạy cảm.

## 8. Các biện pháp bổ sung

- Rate limit endpoint upload và submit review.
- Kiểm tra quyền ở backend cho mọi request.
- Dùng optimistic concurrency cho Content và Schedule.
- Không mass-assign `Status`, `ReviewedByUserId` hoặc `CreatedByUserId` từ request.
- Chỉ lấy current user từ access token hợp lệ.

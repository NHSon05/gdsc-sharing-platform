# Sprint 4 — Quy trình Sharing Content

## 1. Luồng chính

```text
Member tạo bài
→ Draft
→ Thêm Contributor, Tag và tài liệu
→ Gửi duyệt
→ PendingReview
→ Admin Approve
→ Published
→ Admin Archive khi không còn sử dụng
```

## 2. Luồng bị từ chối

```text
PendingReview
→ Admin Reject kèm ReviewNote
→ Rejected
→ Owner chỉnh sửa
→ Gửi duyệt lại
→ PendingReview
```

## 3. Luồng rút yêu cầu duyệt

Owner được rút bài từ `PendingReview` về `Draft` nếu Admin chưa hoàn tất xử lý. Hành động phải kiểm tra concurrency để tránh ghi đè quyết định duyệt vừa xảy ra.

## 4. Quyền chỉnh sửa

| Trạng thái    |    Owner    | Contributor  |           Admin           |
| ------------- | :---------: | :----------: | :-----------------------: |
| Draft         | Sửa toàn bộ | Không xem/sửa |           Có             |
| PendingReview |    Không    |    Không     |     Chỉ duyệt/từ chối     |
| Rejected      | Sửa toàn bộ | Sửa nội dung |            Có             |
| Published     | Sửa toàn bộ |    Không     | Đưa về Draft hoặc Archive |
| Archived      |    Không    |    Không     | Có thể khôi phục về Draft |

Contributor không được:

- Xóa Owner.
- Thêm hoặc xóa tác giả khác.
- Gửi duyệt hoặc rút yêu cầu duyệt.
- Thay đổi trạng thái.

## 5. Quy tắc duyệt

- Chỉ Admin xử lý bài PendingReview.
- Admin không được approve bài thiếu Owner hoặc BodyMarkdown.
- Reject bắt buộc có lý do.
- Approve xóa `ReviewNote` cũ và lưu `ReviewedByUserId`, `ReviewedAtUtc`, `PublishedAtUtc`.
- Mỗi lần submit, approve, reject, return-to-draft và archive phải có audit log.

## 6. Liên kết Roadmap

Đề xuất mở rộng trong Sprint 4: một Sharing Content có thể liên kết với nhiều `RoadmapNode` của Sprint 3 để người học tìm thấy bài chia sẻ ngay trong lộ trình.

Nếu triển khai, sử dụng bảng nối `SharingContentRoadmapNode` và không tạo dependency ngược từ module Roadmap sang Sharing.

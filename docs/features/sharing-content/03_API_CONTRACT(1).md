# Sprint 4 — API Contract

Chi tiết response thực tế, collection bắt buộc, ETag cho tài liệu và giới hạn request: [Phase 3–5 notes](./10_PHASE_3_5_NOTES.md).

## 1. Sharing Content dành cho Member

```http
GET  /api/sharing/contents
GET  /api/sharing/contents/{slug}
GET  /api/sharing/contents/mine
GET  /api/sharing/contents/mine/{id}
POST /api/sharing/contents
PATCH  /api/sharing/contents/{id}
POST /api/sharing/contents/{id}/submit
POST /api/sharing/contents/{id}/withdraw
```

Query danh sách:

```text
search
tagId
authorId
page
pageSize
sort
```

Request tạo nội dung:

```json
{
  "title": "Clean Architecture trong ASP.NET Core",
  "slug": "clean-architecture-aspnet-core",
  "summary": "Giới thiệu cách tổ chức dự án theo các lớp độc lập.",
  "bodyMarkdown": "# Nội dung\n...",
  "tagIds": ["uuid"],
  "contributorUserIds": ["uuid"]
}
```

## 2. Tài liệu của Sharing Content

```http
GET    /api/sharing/contents/{contentId}/resources
POST   /api/sharing/contents/{contentId}/resources/links
POST   /api/sharing/contents/{contentId}/resources/files
PATCH    /api/sharing/resources/{resourceId}
POST   /api/sharing/resources/{resourceId}/replace-file
DELETE /api/sharing/resources/{resourceId}
PATCH    /api/sharing/contents/{contentId}/resources/reorder
GET    /api/sharing/resources/{resourceId}/download
```

Request tạo link:

```json
{
  "title": "Microsoft Architecture Guide",
  "description": "Tài liệu tham khảo",
  "externalUrl": "https://example.com/guide",
  "sortOrder": 1
}
```

Upload tệp sử dụng `multipart/form-data` gồm `title`, `description`, `sortOrder` và `file`.

## 3. Admin duyệt nội dung

```http
GET  /api/admin/sharing/contents
GET  /api/admin/sharing/contents/{id}
POST /api/admin/sharing/contents/{id}/approve
POST /api/admin/sharing/contents/{id}/reject
POST /api/admin/sharing/contents/{id}/return-to-draft
POST /api/admin/sharing/contents/{id}/archive
```

Request từ chối:

```json
{
  "reviewNote": "Vui lòng bổ sung ví dụ minh họa và nguồn tham khảo."
}
```

## 4. Quản lý Tag

```http
GET   /api/sharing/tags
GET   /api/admin/sharing/tags
POST  /api/admin/sharing/tags
PATCH   /api/admin/sharing/tags/{id}
PATCH /api/admin/sharing/tags/{id}/status
```

## 5. Sharing Schedule dành cho Member

Member có thể tạo và cập nhật lịch do mình tạo. Khi Member tạo lịch, backend tự động thêm Member đó vào danh sách Presenter.

```http
GET /api/sharing/schedules
GET /api/sharing/schedules/{id}
GET /api/sharing/schedules/mine
POST /api/sharing/schedules
PATCH /api/sharing/schedules/{id}
```

Query:

```text
from
to
status
deliveryMode
sharingType
presenterId
page
pageSize
```

Response chi tiết:

```json
{
  "id": "uuid",
  "title": "Frontend Sharing: React Performance",
  "description": "Chia sẻ kỹ thuật tối ưu React.",
  "sharingType": "TechTalk",
  "deliveryMode": "Hybrid",
  "startsAtUtc": "2026-09-20T07:00:00Z",
  "endsAtUtc": "2026-09-20T09:00:00Z",
  "timeZoneId": "Asia/Ho_Chi_Minh",
  "location": "Phòng Lab 01",
  "meetingUrl": "https://meet.example.com/session",
  "status": "Scheduled",
  "presenters": [
    {
      "userId": "uuid",
      "fullName": "Nguyễn Văn A",
      "role": "Speaker"
    }
  ],
  "contents": [
    {
      "id": "uuid",
      "title": "React Performance",
      "slug": "react-performance"
    }
  ]
}
```

## 6. Admin quản lý lịch

```http
GET    /api/admin/sharing/schedules
GET    /api/admin/sharing/schedules/{id}
POST   /api/admin/sharing/schedules
PATCH    /api/admin/sharing/schedules/{id}
POST   /api/admin/sharing/schedules/{id}/publish
POST   /api/admin/sharing/schedules/{id}/start
POST   /api/admin/sharing/schedules/{id}/complete
POST   /api/admin/sharing/schedules/{id}/cancel
PATCH    /api/admin/sharing/schedules/{id}/presenters
PATCH    /api/admin/sharing/schedules/{id}/contents
PATCH    /api/admin/sharing/schedules/{id}/audience
DELETE /api/admin/sharing/schedules/{id}
```

Admin có thể chọn Member làm Presenter khi tạo hoặc cập nhật lịch. Member không được tự thay đổi Presenter khác ngoài chính mình.

Request tạo lịch:

```json
{
  "title": "Frontend Sharing: React Performance",
  "description": "Chia sẻ kỹ thuật tối ưu React.",
  "sharingType": "TechTalk",
  "deliveryMode": "Hybrid",
  "startsAtLocal": "2026-09-20T14:00:00",
  "endsAtLocal": "2026-09-20T16:00:00",
  "timeZoneId": "Asia/Ho_Chi_Minh",
  "location": "Phòng Lab 01",
  "meetingUrl": "https://meet.example.com/session",
  "audienceScope": "SelectedAudience",
  "presenters": [{ "userId": "uuid", "role": "Speaker", "sortOrder": 1 }],
  "contentIds": ["uuid"],
  "generationIds": ["uuid"],
  "departmentIds": ["uuid"]
}
```

Request hủy:

```json
{
  "reason": "Người trình bày không thể tham gia theo lịch đã công bố."
}
```

## 7. Concurrency

Các request cập nhật Content và Schedule phải gửi version hiện tại:

```http
If-Match: "12"
```

Nếu dữ liệu đã bị người khác cập nhật, trả `412 Precondition Failed`.

## 8. Mã lỗi

| Status | Trường hợp                                               |
| -----: | -------------------------------------------------------- |
|  `400` | Dữ liệu hoặc chuyển trạng thái không hợp lệ              |
|  `401` | Chưa đăng nhập                                           |
|  `403` | Không phải Owner, Contributor phù hợp hoặc Admin         |
|  `404` | Không tìm thấy dữ liệu hoặc tệp                          |
|  `409` | Trùng slug, trùng lịch presenter hoặc xung đột nghiệp vụ |
|  `412` | Version không còn mới                                    |
|  `413` | Tệp vượt dung lượng                                      |
|  `415` | Định dạng tệp không hỗ trợ                               |
|  `500` | Lỗi hệ thống hoặc storage                                |

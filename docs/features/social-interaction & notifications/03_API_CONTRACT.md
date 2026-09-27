# Sprint 5 — API Contract

## 1. Like

```http
PUT    /api/sharing/contents/{contentId}/like
DELETE /api/sharing/contents/{contentId}/like
```

Response:

```json
{
  "contentId": "uuid",
  "isLikedByCurrentUser": true,
  "likeCount": 24
}
```

PUT khi đã like và DELETE khi chưa like vẫn trả trạng thái thành công tương ứng, không tạo bản ghi trùng.

## 2. Comment

```http
GET    /api/sharing/contents/{contentId}/comments
POST   /api/sharing/contents/{contentId}/comments
POST   /api/sharing/comments/{commentId}/replies
PUT    /api/sharing/comments/{commentId}
DELETE /api/sharing/comments/{commentId}
```

Request tạo hoặc reply:

```json
{
  "bodyMarkdown": "Nội dung này rất hữu ích."
}
```

Request cập nhật:

```json
{
  "bodyMarkdown": "Nội dung đã được chỉnh sửa.",
  "version": 3
}
```

Danh sách comment dùng cursor pagination:

```text
cursor
pageSize
sort=oldest|newest
```

Response là `{ items: CommentResponse[], nextCursor: string | null }`. Các item là danh sách phẳng có `parentCommentId`; UI ghép hội thoại qua ID (parent có thể nằm ở trang khác). Cursor dùng `(createdAtUtc, id)` và gắn với content/sort. `pageSize` từ 1–100, mặc định 20. Deleted/Hidden giữ placeholder; Member không nhận body hoặc lý do moderation. `commentCount` chỉ tính Active, bao gồm reply.

## 3. Comment moderation

```http
POST /api/admin/sharing/comments/{commentId}/hide
POST /api/admin/sharing/comments/{commentId}/restore
```

Request ẩn:

```json
{
  "reason": "Nội dung không phù hợp với quy định cộng đồng."
}
```

## 4. Saved Content

```http
PUT    /api/sharing/contents/{contentId}/saved
DELETE /api/sharing/contents/{contentId}/saved
GET    /api/sharing/contents/saved
```

Response PUT/DELETE:

```json
{
  "contentId": "uuid",
  "isSavedByCurrentUser": true
}
```

## 5. Trạng thái tương tác của bài viết

`GET /api/sharing/contents/{slug}` giữ wrapper `ContentResponse` của Sprint 4. Các field dưới đây được thêm trong `content` (và trong mỗi `ContentSummary` ở danh sách):

```json
{
  "id": "uuid",
  "title": "Clean Architecture",
  "likeCount": 24,
  "commentCount": 8,
  "isLikedByCurrentUser": true,
  "isSavedByCurrentUser": false
}
```

## 6. RSVP

```http
PUT    /api/sharing/schedules/{scheduleId}/rsvp
DELETE /api/sharing/schedules/{scheduleId}/rsvp
GET    /api/sharing/schedules/{scheduleId}/rsvp/me
GET    /api/admin/sharing/schedules/{scheduleId}/rsvps
GET    /api/admin/sharing/schedules/{scheduleId}/rsvp-summary
```

Request:

```json
{
  "status": "Going",
  "version": 3
}
```

`status` bắt buộc. `version` bắt buộc khi đổi trạng thái RSVP đã tồn tại và bỏ qua khi tạo lần đầu. Client lấy version mới từ response; version cũ nhận `412`. PUT đúng trạng thái hiện có là no-op idempotent, kể cả retry với version cũ. DELETE gửi `If-Match: "version"`; `GET .../rsvp/me` trả `204` nếu chưa có phản hồi.

Response:

```json
{
  "scheduleId": "uuid",
  "status": "Going",
  "updatedAtUtc": "2026-09-26T15:30:00Z",
  "version": 4
}
```

Summary dành cho Admin:

```json
{
  "scheduleId": "uuid",
  "going": 32,
  "maybe": 7,
  "notGoing": 4,
  "noResponse": 18
}
```

## 7. Notification

```http
GET   /api/notifications
GET   /api/notifications/unread-count
PATCH /api/notifications/{notificationId}/read
PATCH /api/notifications/read-all
```

Query danh sách:

```text
cursor
pageSize
isRead
type
```

Response:

```json
{
  "items": [
    {
      "id": "uuid",
      "type": "ContentLiked",
      "title": "Bài viết có lượt thích mới",
      "message": "Nguyễn Văn A đã thích bài viết Clean Architecture.",
      "route": "/sharing/clean-architecture",
      "isRead": false,
      "createdAtUtc": "2026-09-26T15:30:00Z"
    }
  ],
  "nextCursor": "opaque-cursor"
}
```

Unread count:

```json
{
  "count": 5
}
```

## 8. Mã lỗi

| Status | Trường hợp |
|---:|---|
| `400` | Nội dung hoặc trạng thái không hợp lệ |
| `401` | Chưa đăng nhập |
| `403` | Không có quyền hoặc không thuộc audience |
| `404` | Không tìm thấy hoặc entity không khả dụng |
| `409` | Duplicate/race conflict hoặc trạng thái Schedule không cho RSVP |
| `412` | Version comment/RSVP đã cũ |
| `429` | Vượt giới hạn tương tác |
| `500` | Lỗi hệ thống |

# Sprint 5 — Mô hình dữ liệu

## 1. ContentLike

| Trường | Kiểu | Bắt buộc | Mô tả |
|---|---|:---:|---|
| `Id` | UUID | Có | Khóa chính |
| `SharingContentId` | UUID | Có | Bài được like |
| `UserId` | UUID | Có | Người like |
| `CreatedAtUtc` | DateTime | Có | Thời điểm like |

Unique index: `(SharingContentId, UserId)`.

## 2. SavedContent

| Trường | Kiểu | Bắt buộc | Mô tả |
|---|---|:---:|---|
| `Id` | UUID | Có | Khóa chính |
| `SharingContentId` | UUID | Có | Bài được lưu |
| `UserId` | UUID | Có | Chủ sở hữu |
| `CreatedAtUtc` | DateTime | Có | Thời điểm lưu |

Unique index: `(SharingContentId, UserId)`.

## 3. ContentComment

| Trường | Kiểu | Bắt buộc | Mô tả |
|---|---|:---:|---|
| `Id` | UUID | Có | Khóa chính |
| `SharingContentId` | UUID | Có | Bài viết |
| `AuthorUserId` | UUID | Có | Người comment |
| `ParentCommentId` | UUID | Không | Comment gốc nếu là reply |
| `BodyMarkdown` | String | Có | Nội dung comment |
| `Status` | Enum | Có | Active, Deleted hoặc Hidden |
| `ModerationReason` | String | Không | Lý do ẩn |
| `ModeratedByUserId` | UUID | Không | Admin xử lý |
| `CreatedAtUtc` | DateTime | Có | Thời điểm tạo |
| `UpdatedAtUtc` | DateTime | Không | Thời điểm sửa |
| `DeletedAtUtc` | DateTime | Không | Thời điểm xóa mềm |
| `Version` | Long | Có | Optimistic concurrency |

Index:

```text
ContentComments(SharingContentId, Status, CreatedAtUtc)
ContentComments(ParentCommentId, CreatedAtUtc)
ContentComments(AuthorUserId, CreatedAtUtc)
```

## 4. ScheduleRsvp

| Trường | Kiểu | Bắt buộc | Mô tả |
|---|---|:---:|---|
| `Id` | UUID | Có | Khóa chính |
| `SharingScheduleId` | UUID | Có | Lịch sharing |
| `UserId` | UUID | Có | Người phản hồi |
| `Status` | Enum | Có | Going, Maybe hoặc NotGoing |
| `RespondedAtUtc` | DateTime | Có | Lần phản hồi đầu |
| `UpdatedAtUtc` | DateTime | Không | Lần thay đổi gần nhất |
| `Version` | Long | Có | Optimistic concurrency |

Unique index: `(SharingScheduleId, UserId)`.

## 5. Notification

| Trường | Kiểu | Bắt buộc | Mô tả |
|---|---|:---:|---|
| `Id` | UUID | Có | Khóa chính |
| `RecipientUserId` | UUID | Có | Người nhận |
| `ActorUserId` | UUID | Không | Người gây ra sự kiện |
| `Type` | Enum | Có | Loại notification |
| `EntityType` | String | Có | SharingContent, ContentComment hoặc SharingSchedule |
| `EntityId` | UUID | Có | Entity liên quan |
| `Title` | String | Có | Tiêu đề ngắn |
| `Message` | String | Có | Nội dung hiển thị |
| `Route` | String | Không | Route nội bộ đến entity |
| `EventId` | UUID | Có | ID riêng của một notification/recipient, tái sử dụng khi retry |
| `IsRead` | Boolean | Có | Đã đọc hay chưa |
| `ReadAtUtc` | DateTime | Không | Thời điểm đọc |
| `CreatedAtUtc` | DateTime | Có | Thời điểm tạo |

Unique index: `(RecipientUserId, EventId)`.

Index:

```text
Notifications(RecipientUserId, IsRead, CreatedAtUtc DESC)
Notifications(RecipientUserId, CreatedAtUtc DESC)
Notifications(Type, ActorUserId, EntityId, RecipientUserId, CreatedAtUtc DESC)
```

Index thứ ba phục vụ kiểm tra cửa sổ chống spam 5 phút của `ContentLiked`. Khi tạo, truy vấn notification mới nhất cùng `Type`, actor, content và recipient; chỉ tạo record mới khi không có record trong 5 phút gần nhất.

## 6. OutboxMessage

| Trường | Kiểu | Bắt buộc | Mô tả |
|---|---|:---:|---|
| `Id` | UUID | Có | Event ID |
| `Type` | String | Có | Loại realtime event |
| `PayloadJson` | JSONB | Có | Payload đã chuẩn hóa |
| `OccurredAtUtc` | DateTime | Có | Thời điểm nghiệp vụ |
| `ProcessedAtUtc` | DateTime | Không | Thời điểm gateway nhận thành công |
| `RetryCount` | Integer | Có | Số lần thử |
| `NextAttemptAtUtc` | DateTime | Không | Lần thử tiếp theo |
| `LastError` | String | Không | Lỗi gần nhất đã rút gọn |
| `DeadLetteredAtUtc` | DateTimeOffset | Không | Có giá trị khi đã dùng hết số lần retry; worker không tự lấy lại |

Index:

```text
OutboxMessages(ProcessedAtUtc, NextAttemptAtUtc, OccurredAtUtc)
```

## 7. Quan hệ

```text
User N ─── N SharingContent  qua ContentLike
User N ─── N SharingContent  qua SavedContent
SharingContent 1 ─── N ContentComment
ContentComment 1 ─── N ContentComment (reply một cấp)
User N ─── N SharingSchedule qua ScheduleRsvp
User 1 ─── N Notification
```

## 8. Quy tắc xóa

- Like, Saved và RSVP có thể xóa vật lý khi user chủ động bỏ tương tác.
- Comment luôn xóa mềm.
- Notification không cascade-delete theo entity nguồn.
- User bị vô hiệu hóa không làm mất lịch sử tương tác.
- Sharing Content hoặc Schedule bị archive/cancel vẫn giữ tương tác.

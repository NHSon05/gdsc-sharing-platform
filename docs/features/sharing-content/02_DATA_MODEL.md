# Sprint 4 — Mô hình dữ liệu

## 1. SharingContent

| Trường             | Kiểu     | Bắt buộc | Mô tả                   |
| ------------------ | -------- | :------: | ----------------------- |
| `Id`               | UUID     |    Có    | Khóa chính              |
| `Title`            | String   |    Có    | Tiêu đề                 |
| `Slug`             | String   |    Có    | Slug duy nhất           |
| `Summary`          | String   |    Có    | Mô tả ngắn              |
| `BodyMarkdown`     | Text     |    Có    | Nội dung chính          |
| `CoverImageUrl`    | String   |  Không   | Ảnh đại diện            |
| `Status`           | Enum     |    Có    | Trạng thái nội dung     |
| `ReviewNote`       | String   |  Không   | Phản hồi duyệt gần nhất |
| `SubmittedAtUtc`   | DateTime |  Không   | Thời điểm gửi duyệt     |
| `ReviewedAtUtc`    | DateTime |  Không   | Thời điểm duyệt/từ chối |
| `ReviewedByUserId` | UUID     |  Không   | Admin xử lý             |
| `PublishedAtUtc`   | DateTime |  Không   | Thời điểm công khai     |
| `CreatedByUserId`  | UUID     |    Có    | Người tạo               |
| `CreatedAtUtc`     | DateTime |    Có    | Thời điểm tạo           |
| `UpdatedAtUtc`     | DateTime |  Không   | Thời điểm cập nhật      |
| `Version`          | Long     |    Có    | Optimistic concurrency  |

## 2. SharingContentAuthor

| Trường             | Kiểu     | Bắt buộc | Mô tả                  |
| ------------------ | -------- | :------: | ---------------------- |
| `Id`               | UUID     |    Có    | Khóa chính             |
| `SharingContentId` | UUID     |    Có    | Bài chia sẻ            |
| `UserId`           | UUID     |    Có    | Tác giả                |
| `AuthorRole`       | Enum     |    Có    | Owner hoặc Contributor |
| `SortOrder`        | Integer  |    Có    | Thứ tự hiển thị        |
| `AddedAtUtc`       | DateTime |    Có    | Thời điểm thêm         |

Unique index: `(SharingContentId, UserId)` và filtered unique index bảo đảm mỗi bài chỉ có một Owner.

## 3. SharingResource

| Trường             | Kiểu     |   Bắt buộc   | Mô tả             |
| ------------------ | -------- | :----------: | ----------------- |
| `Id`               | UUID     |      Có      | Khóa chính        |
| `SharingContentId` | UUID     |      Có      | Bài chứa tài liệu |
| `Title`            | String   |      Có      | Tên tài liệu      |
| `Description`      | String   |    Không     | Mô tả             |
| `ResourceType`     | Enum     |      Có      | Link hoặc File    |
| `ExternalUrl`      | String   | Có điều kiện | URL khi là Link   |
| `OriginalFileName` | String   | Có điều kiện | Tên tệp gốc       |
| `StorageKey`       | String   | Có điều kiện | Khóa storage      |
| `FileSize`         | Long     | Có điều kiện | Kích thước byte   |
| `ContentType`      | String   | Có điều kiện | MIME type         |
| `SortOrder`        | Integer  |      Có      | Thứ tự            |
| `IsActive`         | Boolean  |      Có      | Trạng thái        |
| `CreatedByUserId`  | UUID     |      Có      | Người thêm        |
| `CreatedAtUtc`     | DateTime |      Có      | Thời điểm tạo     |

## 4. SharingTag và SharingContentTag

`SharingTag` gồm `Id`, `Name`, `Slug`, `Color`, `IsActive`, `CreatedAtUtc`, `UpdatedAtUtc`.

`SharingContentTag` là bảng nối nhiều-nhiều gồm `SharingContentId` và `SharingTagId`.

## 5. SharingSchedule

| Trường               | Kiểu     |   Bắt buộc   | Mô tả                                                       |
| -------------------- | -------- | :----------: | ----------------------------------------------------------- |
| `Id`                 | UUID     |      Có      | Khóa chính                                                  |
| `Title`              | String   |      Có      | Tên buổi sharing                                            |
| `Description`        | Text     |    Không     | Nội dung giới thiệu                                         |
| `SharingType`        | Enum     |      Có      | TechTalk, Workshop, StudyGroup, InternalTraining hoặc Other |
| `DeliveryMode`       | Enum     |      Có      | Online, Offline hoặc Hybrid                                 |
| `StartsAtUtc`        | DateTime |      Có      | Thời gian bắt đầu UTC                                       |
| `EndsAtUtc`          | DateTime |      Có      | Thời gian kết thúc UTC                                      |
| `TimeZoneId`         | String   |      Có      | Múi giờ IANA                                                |
| `Location`           | String   | Có điều kiện | Địa điểm                                                    |
| `MeetingUrl`         | String   | Có điều kiện | Link họp                                                    |
| `Status`             | Enum     |      Có      | Trạng thái lịch                                             |
| `AudienceScope`      | Enum     |      Có      | AllMembers hoặc SelectedAudience                            |
| `CancellationReason` | String   |    Không     | Lý do hủy                                                   |
| `CreatedByUserId`    | UUID     |      Có      | Member hoặc Admin tạo                                       |
| `CreatedAtUtc`       | DateTime |      Có      | Thời điểm tạo                                               |
| `UpdatedAtUtc`       | DateTime |    Không     | Thời điểm cập nhật                                          |
| `Version`            | Long     |      Có      | Optimistic concurrency                                      |

## 6. SharingSchedulePresenter

| Trường              | Kiểu    | Bắt buộc | Mô tả                                     |
| ------------------- | ------- | :------: | ----------------------------------------- |
| `Id`                | UUID    |    Có    | Khóa chính                                |
| `SharingScheduleId` | UUID    |    Có    | Lịch sharing                              |
| `UserId`            | UUID    |    Có    | Người trình bày                           |
| `PresenterRole`     | Enum    |    Có    | Host, Speaker, CoSpeaker hoặc Facilitator |
| `SortOrder`         | Integer |    Có    | Thứ tự hiển thị                           |

Unique index: `(SharingScheduleId, UserId, PresenterRole)`.

## 7. SharingScheduleContent

Bảng nối nhiều-nhiều:

| Trường              | Kiểu    | Bắt buộc |
| ------------------- | ------- | :------: |
| `SharingScheduleId` | UUID    |    Có    |
| `SharingContentId`  | UUID    |    Có    |
| `SortOrder`         | Integer |    Có    |

## 8. Audience theo Gen và Department

`SharingScheduleAudienceGeneration`:

- `SharingScheduleId`
- `ClubGenerationId`

`SharingScheduleAudienceDepartment`:

- `SharingScheduleId`
- `DepartmentId`

## 9. Quan hệ

```text
User N ─── N SharingContent       qua SharingContentAuthor
SharingContent 1 ─── N SharingResource
SharingContent N ─── N SharingTag
SharingSchedule N ─── N User      qua SharingSchedulePresenter
SharingSchedule N ─── N SharingContent
SharingSchedule N ─── N ClubGeneration
SharingSchedule N ─── N Department
```

## 10. Index truy vấn

```text
SharingContents(Status, PublishedAtUtc)
SharingContents(Slug)
SharingContentAuthors(UserId, SharingContentId)
SharingResources(SharingContentId, IsActive, SortOrder)
SharingSchedules(Status, StartsAtUtc)
SharingSchedules(StartsAtUtc, EndsAtUtc)
SharingSchedulePresenters(UserId, SharingScheduleId)
```

## 11. Quy tắc xóa

- Nội dung đã gửi duyệt hoặc công khai: không xóa cứng.
- Lịch Scheduled, Completed hoặc Cancelled: không xóa cứng.
- User, Gen hoặc Department đã được tham chiếu: `Restrict`.
- Tệp chỉ xóa vật lý sau khi transaction database thành công.

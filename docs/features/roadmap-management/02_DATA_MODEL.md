# Sprint 3 — Mô hình dữ liệu Roadmap

## 1. RoadmapCategory

| Trường         | Kiểu     | Bắt buộc | Mô tả                   |
| -------------- | -------- | :------: | ----------------------- |
| `Id`           | UUID     |    Có    | Khóa chính              |
| `Name`         | String   |    Có    | Tên danh mục            |
| `Slug`         | String   |    Có    | Slug duy nhất           |
| `Description`  | String   |  Không   | Mô tả                   |
| `Icon`         | String   |  Không   | Tên hoặc URL biểu tượng |
| `Color`        | String   |  Không   | Màu đại diện            |
| `SortOrder`    | Integer  |    Có    | Thứ tự hiển thị         |
| `IsActive`     | Boolean  |    Có    | Trạng thái hoạt động    |
| `CreatedAtUtc` | DateTime |    Có    | Thời điểm tạo           |
| `UpdatedAtUtc` | DateTime |  Không   | Thời điểm cập nhật      |

## 2. Roadmap

| Trường              | Kiểu     | Bắt buộc | Mô tả                          |
| ------------------- | -------- | :------: | ------------------------------ |
| `Id`                | UUID     |    Có    | Khóa chính                     |
| `CategoryId`        | UUID     |    Có    | Danh mục                       |
| `Title`             | String   |    Có    | Tên roadmap                    |
| `Slug`              | String   |    Có    | Slug duy nhất toàn hệ thống    |
| `ShortDescription`  | String   |    Có    | Mô tả ngắn                     |
| `Description`       | Text     |  Không   | Nội dung chi tiết              |
| `ThumbnailUrl`      | String   |  Không   | Ảnh đại diện                   |
| `Level`             | Enum     |    Có    | Mức độ                         |
| `EstimatedDuration` | String   |  Không   | Thời gian học dự kiến          |
| `Prerequisites`     | Text     |  Không   | Kiến thức cần có               |
| `Status`            | Enum     |    Có    | Draft, Published hoặc Archived |
| `SortOrder`         | Integer  |    Có    | Thứ tự hiển thị                |
| `CreatedByUserId`   | UUID     |    Có    | Người tạo                      |
| `UpdatedByUserId`   | UUID     |  Không   | Người cập nhật gần nhất        |
| `PublishedAtUtc`    | DateTime |  Không   | Thời điểm công khai            |
| `CreatedAtUtc`      | DateTime |    Có    | Thời điểm tạo                  |
| `UpdatedAtUtc`      | DateTime |  Không   | Thời điểm cập nhật             |

## 3. RoadmapNode

| Trường               | Kiểu     | Bắt buộc | Mô tả                       |
| -------------------- | -------- | :------: | --------------------------- |
| `Id`                 | UUID     |    Có    | Khóa chính                  |
| `RoadmapId`          | UUID     |    Có    | Roadmap chứa node           |
| `Title`              | String   |    Có    | Tên node                    |
| `Slug`               | String   |    Có    | Slug trong roadmap          |
| `Description`        | Text     |  Không   | Nội dung giới thiệu         |
| `LearningObjectives` | Text     |  Không   | Mục tiêu cần đạt            |
| `EstimatedDuration`  | String   |  Không   | Thời gian học dự kiến       |
| `NodeType`           | Enum     |    Có    | Topic, Group hoặc Milestone |
| `PositionX`          | Decimal  |    Có    | Tọa độ ngang trên canvas    |
| `PositionY`          | Decimal  |    Có    | Tọa độ dọc trên canvas      |
| `Width`              | Decimal  |  Không   | Chiều rộng tùy chỉnh        |
| `Color`              | String   |  Không   | Màu nền hoặc mã theme       |
| `Icon`               | String   |  Không   | Biểu tượng của node         |
| `SortOrder`          | Integer  |    Có    | Thứ tự fallback ổn định     |
| `IsActive`           | Boolean  |    Có    | Trạng thái hoạt động        |
| `CreatedAtUtc`       | DateTime |    Có    | Thời điểm tạo               |
| `UpdatedAtUtc`       | DateTime |  Không   | Thời điểm cập nhật          |

## 4. RoadmapEdge

| Trường         | Kiểu     | Bắt buộc | Mô tả                               |
| -------------- | -------- | :------: | ----------------------------------- |
| `Id`           | UUID     |    Có    | Khóa chính                          |
| `RoadmapId`    | UUID     |    Có    | Roadmap chứa đường nối              |
| `SourceNodeId` | UUID     |    Có    | Node bắt đầu                        |
| `TargetNodeId` | UUID     |    Có    | Node kết thúc                       |
| `RelationType` | Enum     |    Có    | Required, Recommended hoặc Optional |
| `Label`        | String   |  Không   | Nhãn giải thích                     |
| `LineStyle`    | Enum     |    Có    | Solid hoặc Dashed                   |
| `SortOrder`    | Integer  |    Có    | Thứ tự fallback                     |
| `IsActive`     | Boolean  |    Có    | Trạng thái hoạt động                |
| `CreatedAtUtc` | DateTime |    Có    | Thời điểm tạo                       |
| `UpdatedAtUtc` | DateTime |  Không   | Thời điểm cập nhật                  |

## 5. LearningResource

| Trường             | Kiểu     |   Bắt buộc   | Mô tả                    |
| ------------------ | -------- | :----------: | ------------------------ |
| `Id`               | UUID     |      Có      | Khóa chính               |
| `RoadmapNodeId`    | UUID     |      Có      | Node chứa tài liệu       |
| `Title`            | String   |      Có      | Tên tài liệu             |
| `Description`      | String   |    Không     | Mô tả ngắn               |
| `ResourceType`     | Enum     |      Có      | Link hoặc File           |
| `ExternalUrl`      | String   | Có điều kiện | URL khi loại là Link     |
| `OriginalFileName` | String   | Có điều kiện | Tên tệp ban đầu          |
| `StoredFileName`   | String   | Có điều kiện | Tên tệp do hệ thống sinh |
| `StorageKey`       | String   | Có điều kiện | Khóa tệp trong storage   |
| `FileSize`         | Long     | Có điều kiện | Kích thước theo byte     |
| `ContentType`      | String   | Có điều kiện | MIME type                |
| `SortOrder`        | Integer  |      Có      | Thứ tự hiển thị          |
| `IsActive`         | Boolean  |      Có      | Trạng thái hoạt động     |
| `CreatedByUserId`  | UUID     |      Có      | Người tạo                |
| `CreatedAtUtc`     | DateTime |      Có      | Thời điểm tạo            |
| `UpdatedAtUtc`     | DateTime |    Không     | Thời điểm cập nhật       |

## 6. Quan hệ

```text
RoadmapCategory 1 ─── N Roadmap
Roadmap         1 ─── N RoadmapNode
Roadmap         1 ─── N RoadmapEdge
RoadmapNode     1 ─── N LearningResource
RoadmapNode     1 ─── N RoadmapEdge (Source)
RoadmapNode     1 ─── N RoadmapEdge (Target)
User            1 ─── N Roadmap
User            1 ─── N LearningResource
```

## 7. Unique index

```text
RoadmapCategories.Slug
Roadmaps.Slug
RoadmapNodes(RoadmapId, Slug)
RoadmapEdges(RoadmapId, SourceNodeId, TargetNodeId, RelationType)
```

## 8. Index truy vấn

```text
Roadmaps(Status, SortOrder)
Roadmaps(CategoryId, Status)
RoadmapNodes(RoadmapId, IsActive)
RoadmapEdges(RoadmapId, IsActive)
LearningResources(RoadmapNodeId, IsActive, SortOrder)
```

## 9. Quy tắc xóa

- Category đang được sử dụng: `Restrict`.
- Roadmap có node: `Restrict`.
- Node có tài liệu hoặc đường nối: `Restrict`.
- Ưu tiên `Archived` hoặc `IsActive = false` thay cho xóa cứng.

## 10. Seed dữ liệu

Bổ sung theo yêu cầu: ngoài danh mục, seeder tạo 6 roadmap Published có node và edge để Admin chỉnh sửa và Member xem chung dữ liệu. Seed không ghi đè chỉnh sửa của Admin; xem [11_ROADMAP_SEED.md](11_ROADMAP_SEED.md).

Seeder phải có tính idempotent và tạo sáu danh mục ban đầu:

```text
Frontend
Backend
Database
Software Engineering
Fullstack
Artificial Intelligence
```

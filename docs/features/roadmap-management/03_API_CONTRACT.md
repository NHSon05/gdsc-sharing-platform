# Sprint 3 — API Contract Roadmap

## 1. API dành cho Member

### Lấy danh sách roadmap

```http
GET /api/roadmaps?search=&categoryId=&level=&page=1&pageSize=12
```

Trả về roadmap `Published` và `Archived`; Member không thấy `Draft`.

Quyết định đã xác nhận: Member cũng được mở liên kết/tải tệp của roadmap Archived nếu node và tài liệu đang hoạt động. Roadmap mới vẫn mặc định Published; khi chuyển từ Draft/Archived sang Published cần ít nhất một node hoạt động.

```json
{
  "items": [
    {
      "id": "uuid",
      "title": "Frontend",
      "slug": "frontend",
      "shortDescription": "Lộ trình học phát triển giao diện web",
      "thumbnailUrl": "/uploads/roadmaps/frontend.webp",
      "category": {
        "id": "uuid",
        "name": "Frontend"
      },
      "level": "Beginner",
      "estimatedDuration": "6 tháng",
      "sectionCount": 8
    }
  ],
  "page": 1,
  "pageSize": 12,
  "totalItems": 1,
  "totalPages": 1
}
```

### Xem chi tiết roadmap

```http
GET /api/roadmaps/{slug}
```

```json
{
  "id": "uuid",
  "title": "Frontend",
  "slug": "frontend",
  "description": "Lộ trình Frontend từ cơ bản đến nâng cao",
  "level": "Beginner",
  "estimatedDuration": "6 tháng",
  "prerequisites": "Không yêu cầu",
  "nodes": [
    {
      "id": "uuid",
      "title": "HTML và CSS",
      "slug": "html-va-css",
      "description": "Kiến thức giao diện web cơ bản",
      "nodeType": "Topic",
      "position": { "x": 320, "y": 180 },
      "color": "#FFE19A",
      "sortOrder": 1,
      "resourceCount": 3
    }
  ],
  "edges": [
    {
      "id": "uuid",
      "sourceNodeId": "source-uuid",
      "targetNodeId": "uuid",
      "relationType": "Required",
      "lineStyle": "Solid"
    }
  ]
}
```

### Xem chi tiết node

```http
GET /api/roadmaps/{roadmapId}/nodes/{nodeId}
```

Response gồm nội dung node, mục tiêu, tài liệu, các node cần học trước và node nên học tiếp theo.

### Tải tệp

```http
GET /api/roadmap-resources/{resourceId}/download
```

Backend kiểm tra quyền và trạng thái trước khi trả tệp.

## 2. API quản trị danh mục

```http
GET   /api/admin/roadmap-categories
POST  /api/admin/roadmap-categories
PATCH   /api/admin/roadmap-categories/{id}
PATCH /api/admin/roadmap-categories/{id}/status
```

## 3. API quản trị Roadmap

```http
GET   /api/admin/roadmaps
GET   /api/admin/roadmaps/{id}
POST  /api/admin/roadmaps
PATCH   /api/admin/roadmaps/{id}
PATCH /api/admin/roadmaps/{id}/status
PATCH   /api/admin/roadmaps/reorder
```

Request tạo roadmap:

```json
{
  "title": "Frontend",
  "slug": "frontend",
  "shortDescription": "Lộ trình phát triển giao diện web",
  "description": "Nội dung giới thiệu chi tiết",
  "categoryId": "uuid",
  "level": "Beginner",
  "estimatedDuration": "6 tháng",
  "prerequisites": "Không yêu cầu",
  "sortOrder": 1
}
```

Request thay đổi trạng thái:

```json
{
  "status": "Published"
}
```

## 4. API quản trị node và đường nối

```http
GET    /api/admin/roadmaps/{roadmapId}/nodes
POST   /api/admin/roadmaps/{roadmapId}/nodes
PATCH    /api/admin/roadmaps/{roadmapId}/nodes/{nodeId}
PATCH  /api/admin/roadmaps/{roadmapId}/nodes/{nodeId}/status
PATCH    /api/admin/roadmaps/{roadmapId}/nodes/positions
GET    /api/admin/roadmaps/{roadmapId}/edges
POST   /api/admin/roadmaps/{roadmapId}/edges
PATCH    /api/admin/roadmaps/{roadmapId}/edges/{edgeId}
DELETE /api/admin/roadmaps/{roadmapId}/edges/{edgeId}
```

Request tạo node:

```json
{
  "title": "HTML và CSS",
  "slug": "html-va-css",
  "description": "Tìm hiểu cấu trúc và giao diện website",
  "learningObjectives": "Xây dựng được trang web responsive cơ bản",
  "estimatedDuration": "4 tuần",
  "nodeType": "Topic",
  "positionX": 320,
  "positionY": 180,
  "color": "#FFE19A",
  "sortOrder": 1
}
```

Request tạo đường nối:

```json
{
  "sourceNodeId": "uuid",
  "targetNodeId": "uuid",
  "relationType": "Required",
  "lineStyle": "Solid",
  "label": null
}
```

Request lưu vị trí sau kéo thả:

```json
{
  "nodes": [
    { "id": "uuid-1", "positionX": 320, "positionY": 180 },
    { "id": "uuid-2", "positionX": 560, "positionY": 180 }
  ]
}
```

## 5. API quản trị tài liệu

Tạo liên kết:

```http
POST /api/admin/roadmap-nodes/{nodeId}/resources/links
```

```json
{
  "title": "Tài liệu HTML cơ bản",
  "description": "Tài liệu dành cho người mới",
  "externalUrl": "https://example.com/html",
  "sortOrder": 1
}
```

Tải tệp lên:

```http
POST /api/admin/roadmap-nodes/{nodeId}/resources/files
Content-Type: multipart/form-data
```

Form data:

```text
title
description
sortOrder
file
```

Cập nhật và sắp xếp:

```http
PATCH    /api/admin/roadmap-resources/{resourceId}
POST   /api/admin/roadmap-resources/{resourceId}/replace-file
PATCH  /api/admin/roadmap-resources/{resourceId}/status
DELETE /api/admin/roadmap-resources/{resourceId}
PATCH    /api/admin/roadmap-nodes/{nodeId}/resources/reorder
```

## 6. Mã lỗi

| Status | Trường hợp                               |
| -----: | ---------------------------------------- |
|  `400` | Request hoặc dữ liệu không hợp lệ        |
|  `401` | Chưa đăng nhập                           |
|  `403` | Không có quyền                           |
|  `404` | Không tìm thấy dữ liệu hoặc tệp          |
|  `409` | Trùng slug, tên hoặc xung đột trạng thái |
|  `413` | Tệp vượt dung lượng                      |
|  `415` | Định dạng tệp không hỗ trợ               |
|  `500` | Lỗi hệ thống hoặc storage                |

```json
{
  "status": 400,
  "title": "Validation failed",
  "detail": "The submitted data is invalid.",
  "validationErrors": {
    "title": ["Title is required."]
  },
  "traceId": "trace-id"
}
```

## 7. Chi tiết request bổ sung (Phase 2–4)

- Category POST/PATCH: `{ "name": "Frontend", "slug": "frontend", "description": null, "icon": null, "color": null, "sortOrder": 0 }`.
- PATCH trạng thái category/node/resource: `{ "isActive": false }`; trường này bắt buộc.
- Reorder roadmap/resource: `{ "ids": ["uuid-2", "uuid-1"] }`. Gửi toàn bộ ID trong collection, kể cả item inactive, mỗi ID đúng một lần. Array order trở thành `SortOrder` từ 0. Thiếu/thừa/trùng ID trả 400.
- PATCH resource: `{ "title": "Guide", "description": null, "externalUrl": "https://example.com", "sortOrder": 0 }`. Link bắt buộc URL; File bắt buộc URL null. Không đổi loại tài liệu qua PATCH.
- Replace file: multipart với trường `file`. Metadata khác của resource được giữ nguyên.
- DELETE resource là vô hiệu hóa; giữ metadata và tệp. DELETE edge xóa đường nối.
- GET `/api/roadmap-categories`: danh mục active cho bộ lọc Member.
- GET `/api/admin/roadmaps/{roadmapId}/nodes/{nodeId}`: chi tiết node cho Admin.
- GET `/api/admin/roadmap-nodes/{nodeId}/resources`: toàn bộ tài liệu để quản trị/reorder.

Response chi tiết node gồm `node` (NodeResponse với position, width, resourceCount), `learningObjectives`, `estimatedDuration`, `resources`, `prerequisites`, `nextNodes`. Mỗi node tham chiếu gồm `id`, `title`, `slug`, `relationType`.

Response resource không chứa StoredFileName hoặc StorageKey. File trả `downloadUrl`, `originalFileName`, `fileSize`, `contentType`; Link trả `externalUrl`. List roadmap thêm `nodeCount`; `sectionCount` được giữ như alias tương thích.

Quy tắc đầy đủ về storage, transaction và cấu hình nằm trong [ghi chú Phase 2–4](08_PHASE_2_4_NOTES.md).

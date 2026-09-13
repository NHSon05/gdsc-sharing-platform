# Sprint 3 — Giải thích API Endpoint Roadmap Management

Tài liệu này giải thích **từng endpoint mới được sinh ra trong Sprint 3 — Roadmap Management**. Phạm vi chỉ gồm các API trong nhóm Roadmap, Category, Node, Edge và Resource; không bao gồm API Authentication/Profile/Membership đã có từ sprint trước.

Nguồn đối chiếu chính:

- `03_API_CONTRACT.md`
- `backend/src/GdscSharingPlatform.Api/Controllers/Roadmaps/*Controller.cs`
- `backend/src/GdscSharingPlatform.Application/Features/Roadmaps/Models/*`

## 1. Quy ước chung

Các endpoint cập nhật, reorder và lưu vị trí của module Roadmap dùng `PATCH`.
Request body dùng các DTO được mô tả bên dưới, với các trường bắt buộc theo schema.
Swagger hiển thị nhóm bằng tên có khoảng trắng, ví dụ `Admin Roadmap Categories`,
`Admin Roadmap Nodes`, `Admin Roadmap Resources`. Đây là nhãn Swagger; tên class C# vẫn dùng PascalCase.


### Authentication và Authorization

| Nhóm API | Quyền truy cập |
| --- | --- |
| `/api/roadmap-*`, `/api/roadmaps`, `/api/roadmap-resources` | User đăng nhập, trạng thái active, role `Admin` hoặc `Member` |
| `/api/admin/...` | User đăng nhập, trạng thái active, role `Admin` theo `AdminPolicy` |

### Quy tắc hiển thị dữ liệu

- Member chỉ thấy roadmap `Published` và `Archived`.
- Member không thấy roadmap `Draft`, kể cả khi truyền filter `status=Draft`.
- Member chỉ thấy node/resource active.
- Member chỉ thấy edge active và cả source node/target node đều active.
- Admin thấy toàn bộ dữ liệu để quản trị, gồm cả `Draft` và inactive item.
- Member được xem/tải tài liệu của roadmap `Archived` nếu roadmap, node và resource thỏa điều kiện hiển thị.

### Mã lỗi phổ biến

| Status | Ý nghĩa |
| ---: | --- |
| `400 Bad Request` | Request sai schema, dữ liệu không hợp lệ, ID không thuộc collection, self-loop, Required cycle |
| `401 Unauthorized` | Chưa đăng nhập hoặc token không hợp lệ |
| `403 Forbidden` | Đăng nhập nhưng không đủ quyền hoặc user không active |
| `404 Not Found` | Không tìm thấy dữ liệu, dữ liệu bị ẩn với Member, hoặc file không còn tồn tại |
| `409 Conflict` | Trùng slug/tên/edge, trạng thái không cho phép thao tác, xung đột transaction |
| `413 Payload Too Large` | File hoặc multipart request vượt giới hạn |
| `415 Unsupported Media Type` | Extension/MIME/signature file không được hỗ trợ hoặc không khớp |

Problem Details luôn có `traceId`; lỗi validation có thêm `validationErrors` dạng camelCase.

## 2. Tổng quan endpoint

| # | Method | Endpoint | Nhóm | Mục đích ngắn |
| ---: | --- | --- | --- | --- |
| 1 | `GET` | `/api/roadmap-categories` | Member | Lấy danh mục active cho bộ lọc/xem roadmap |
| 2 | `GET` | `/api/roadmaps` | Member | Lấy danh sách roadmap public với filter/pagination |
| 3 | `GET` | `/api/roadmaps/{slug}` | Member | Xem chi tiết graph của một roadmap public |
| 4 | `GET` | `/api/roadmaps/{roadmapId}/nodes/{nodeId}` | Member | Xem chi tiết một node và tài liệu liên quan |
| 5 | `GET` | `/api/roadmap-resources/{id}/download` | Member/Admin | Tải file resource qua backend có kiểm tra quyền |
| 6 | `GET` | `/api/admin/roadmap-categories` | Admin | Liệt kê tất cả category, gồm inactive |
| 7 | `POST` | `/api/admin/roadmap-categories` | Admin | Tạo category roadmap |
| 8 | `PATCH` | `/api/admin/roadmap-categories/{id}` | Admin | Cập nhật category roadmap |
| 9 | `PATCH` | `/api/admin/roadmap-categories/{id}/status` | Admin | Bật/tắt category |
| 10 | `GET` | `/api/admin/roadmaps` | Admin | Liệt kê tất cả roadmap, gồm Draft/Archived |
| 11 | `GET` | `/api/admin/roadmaps/{id}` | Admin | Xem chi tiết một roadmap theo ID |
| 12 | `POST` | `/api/admin/roadmaps` | Admin | Tạo roadmap mới |
| 13 | `PATCH` | `/api/admin/roadmaps/{id}` | Admin | Cập nhật metadata roadmap |
| 14 | `PATCH` | `/api/admin/roadmaps/{id}/status` | Admin | Đổi trạng thái Draft/Published/Archived |
| 15 | `PATCH` | `/api/admin/roadmaps/reorder` | Admin | Sắp xếp thứ tự roadmap toàn hệ thống |
| 16 | `GET` | `/api/admin/roadmaps/{roadmapId}/nodes` | Admin | Liệt kê node của roadmap |
| 17 | `GET` | `/api/admin/roadmaps/{roadmapId}/nodes/{nodeId}` | Admin | Xem chi tiết node cho quản trị |
| 18 | `POST` | `/api/admin/roadmaps/{roadmapId}/nodes` | Admin | Tạo node trong roadmap |
| 19 | `PATCH` | `/api/admin/roadmaps/{roadmapId}/nodes/{nodeId}` | Admin | Cập nhật node |
| 20 | `PATCH` | `/api/admin/roadmaps/{roadmapId}/nodes/{nodeId}/status` | Admin | Bật/tắt node |
| 21 | `PATCH` | `/api/admin/roadmaps/{roadmapId}/nodes/positions` | Admin | Lưu tọa độ canvas sau kéo thả |
| 22 | `GET` | `/api/admin/roadmaps/{roadmapId}/edges` | Admin | Liệt kê đường nối của roadmap |
| 23 | `POST` | `/api/admin/roadmaps/{roadmapId}/edges` | Admin | Tạo đường nối giữa hai node |
| 24 | `PATCH` | `/api/admin/roadmaps/{roadmapId}/edges/{edgeId}` | Admin | Cập nhật đường nối |
| 25 | `DELETE` | `/api/admin/roadmaps/{roadmapId}/edges/{edgeId}` | Admin | Xóa cứng một edge |
| 26 | `GET` | `/api/admin/roadmap-nodes/{nodeId}/resources` | Admin | Liệt kê tài liệu của node |
| 27 | `POST` | `/api/admin/roadmap-nodes/{nodeId}/resources/links` | Admin | Tạo resource dạng link |
| 28 | `POST` | `/api/admin/roadmap-nodes/{nodeId}/resources/files` | Admin | Upload resource dạng file |
| 29 | `PATCH` | `/api/admin/roadmap-resources/{id}` | Admin | Cập nhật metadata resource |
| 30 | `POST` | `/api/admin/roadmap-resources/{id}/replace-file` | Admin | Thay file cho file resource |
| 31 | `PATCH` | `/api/admin/roadmap-resources/{id}/status` | Admin | Bật/tắt resource |
| 32 | `DELETE` | `/api/admin/roadmap-resources/{id}` | Admin | Soft-delete resource bằng cách tắt active |
| 33 | `PATCH` | `/api/admin/roadmap-nodes/{nodeId}/resources/reorder` | Admin | Sắp xếp tài liệu trong node |

## 3. API dành cho Member

### 3.1. `GET /api/roadmap-categories`

Lấy danh sách danh mục roadmap đang active để hiển thị ở màn hình member, ví dụ dropdown/filter category.

- Controller: `RoadmapCategoriesController.List`
- Service: `IRoadmapCategoryService.ListAsync(admin: false)`
- Auth: active `Admin` hoặc `Member`
- Request body: không có
- Response: `CategoryResponse[]`

Behavior:

- Chỉ trả category có `IsActive = true`.
- Sắp xếp theo `SortOrder`, sau đó theo `Id` để kết quả ổn định.
- Không trả category inactive cho Member.

Dùng khi:

- FE cần render filter category ở trang danh sách roadmap.
- FE cần biết các category public đang được phép chọn.

### 3.2. `GET /api/roadmaps`

Lấy danh sách roadmap mà Member được xem.

- Controller: `RoadmapsController.List`
- Service: `IRoadmapService.ListAsync(query, admin: false)`
- Auth: active `Admin` hoặc `Member`
- Query: `RoadmapQuery`
  - `search`: tìm theo title
  - `categoryId`: lọc category
  - `level`: lọc cấp độ roadmap
  - `status`: lọc trạng thái nhưng vẫn bị giới hạn bởi quyền Member
  - `page`: mặc định `1`
  - `pageSize`: mặc định `12`, tối đa `100`
- Response: `PageResponse<RoadmapSummary>`

Behavior:

- Chỉ trả roadmap `Published` hoặc `Archived`.
- Roadmap `Draft` luôn bị ẩn với Member.
- Nếu Member truyền `status=Draft`, response hợp lệ nhưng danh sách rỗng.
- `nodeCount`/`sectionCount` chỉ tính node Member được thấy.
- Response dùng phân trang để hỗ trợ listing lớn.

Dùng khi:

- FE hiển thị trang danh sách roadmap.
- FE tìm kiếm/lọc roadmap theo category, level hoặc trạng thái public.

### 3.3. `GET /api/roadmaps/{slug}`

Xem chi tiết một roadmap bằng slug public, gồm metadata và graph node–edge để FE vẽ sơ đồ.

- Controller: `RoadmapsController.Get`
- Service: `IRoadmapService.GetBySlugAsync(slug)`
- Auth: active `Admin` hoặc `Member`
- Path params:
  - `slug`: slug duy nhất toàn hệ thống của roadmap
- Request body: không có
- Response: `RoadmapResponse`

Behavior:

- Member chỉ mở được roadmap `Published` hoặc `Archived`.
- Roadmap `Draft` trả `404` với Member để tránh lộ dữ liệu nháp.
- `nodes` chỉ gồm node active.
- `edges` chỉ gồm edge active và nối giữa hai node active.
- Response có đủ `position.x`, `position.y`, `width`, `color`, `icon`, `sortOrder` để FE tái tạo canvas.

Dùng khi:

- FE mở trang chi tiết roadmap.
- FE cần dựng graph roadmap bằng node và edge.

### 3.4. `GET /api/roadmaps/{roadmapId}/nodes/{nodeId}`

Xem chi tiết một node trong roadmap, gồm mục tiêu học tập, tài liệu, node cần học trước và node tiếp theo.

- Controller: `RoadmapsController.Node`
- Service: `IRoadmapNodeService.GetAsync(roadmapId, nodeId)`
- Auth: active `Admin` hoặc `Member`
- Path params:
  - `roadmapId`: ID roadmap chứa node
  - `nodeId`: ID node cần xem
- Request body: không có
- Response: `NodeDetailResponse`

Behavior:

- Member chỉ xem được node active thuộc roadmap `Published` hoặc `Archived`.
- Nếu node không thuộc roadmap trong path, trả `404`.
- `resources` chỉ gồm resource active đối với Member.
- `prerequisites` lấy từ edge đi vào node.
- `nextNodes` lấy từ edge đi ra node.
- Edge inactive hoặc edge nối tới node inactive không xuất hiện với Member.

Dùng khi:

- FE mở drawer/detail panel cho một node.
- FE cần hiện danh sách tài liệu và liên kết điều hướng trong roadmap.

### 3.5. `GET /api/roadmap-resources/{id}/download`

Tải file resource thông qua backend thay vì static URL, để backend kiểm tra quyền và trạng thái dữ liệu trước khi mở file.

- Controller: `RoadmapResourcesController.Download`
- Service: `ILearningResourceService.DownloadAsync(id)`
- Auth: active `Admin` hoặc `Member`
- Path params:
  - `id`: ID của `LearningResource`
- Request body: không có
- Response: binary file stream

Behavior:

- Chỉ áp dụng cho resource loại `File`; resource loại `Link` trả `404` khi gọi download.
- Member chỉ tải được khi roadmap không phải Draft, node active và resource active.
- Admin tải được file để quản trị kể cả khi roadmap Draft hoặc resource inactive, miễn file còn tồn tại.
- Response đặt:
  - `Content-Disposition: attachment` với tên file gốc
  - `X-Content-Type-Options: nosniff`
  - `Cache-Control: private, no-store`
  - hỗ trợ byte range (`206 Partial Content`) cho file lớn
- Không trả `StorageKey` hoặc path nội bộ.

Dùng khi:

- FE xử lý nút download tài liệu file.
- Backend cần chặn truy cập trực tiếp vào thư mục storage.

## 4. API quản trị danh mục Roadmap

### 4.1. `GET /api/admin/roadmap-categories`

Liệt kê toàn bộ category để Admin quản trị.

- Controller: `AdminRoadmapCategoriesController.List`
- Service: `IRoadmapCategoryService.ListAsync(admin: true)`
- Auth: `AdminPolicy`
- Request body: không có
- Response: `CategoryResponse[]`

Behavior:

- Trả cả active và inactive category.
- Sắp xếp theo `SortOrder`, sau đó `Id`.
- Dùng cho màn hình quản trị category và reorder thủ công nếu FE cần.

### 4.2. `POST /api/admin/roadmap-categories`

Tạo category mới cho roadmap.

- Controller: `AdminRoadmapCategoriesController.Create`
- Service: `IRoadmapCategoryService.CreateAsync`
- Auth: `AdminPolicy`
- Request body: `CategoryRequest`
- Response: `201 Created` + `CategoryResponse`

Request chính:

```json
{
  "name": "Frontend",
  "slug": "frontend",
  "description": "Lộ trình giao diện web",
  "icon": "code",
  "color": "#FFE19A",
  "sortOrder": 0
}
```

Behavior:

- Normalize slug trước khi lưu.
- `name` và `slug` không được trùng category khác.
- `sortOrder` phải không âm.
- Category mới mặc định active.

Dùng khi:

- Admin thêm nhóm roadmap mới như Frontend, Backend, AI.

### 4.3. `PATCH /api/admin/roadmap-categories/{id}`

Cập nhật metadata category hiện có.

- Controller: `AdminRoadmapCategoriesController.Update`
- Service: `IRoadmapCategoryService.UpdateAsync`
- Auth: `AdminPolicy`
- Path params:
  - `id`: ID category
- Request body: `CategoryRequest`
- Response: `CategoryResponse`

Behavior:

- Cho phép đổi `name`, `slug`, `description`, `icon`, `color`, `sortOrder`.
- Vẫn kiểm tra trùng `name`/`slug` với category khác.
- Không tự thay đổi roadmap đang thuộc category này.
- Nếu category không tồn tại, trả `404`.

Dùng khi:

- Admin sửa tên/slug/hiển thị của category.

### 4.4. `PATCH /api/admin/roadmap-categories/{id}/status`

Bật hoặc tắt category.

- Controller: `AdminRoadmapCategoriesController.Status`
- Service: `IRoadmapCategoryService.SetStatusAsync`
- Auth: `AdminPolicy`
- Request body: `ActiveStatusRequest`
- Response: `204 No Content`

Request:

```json
{ "isActive": false }
```

Behavior:

- `isActive` là bắt buộc.
- Tắt category khiến Member không thấy category trong list category.
- Roadmap đã thuộc category đó không bị xóa và vẫn có thể public nếu status cho phép.
- Không hard-delete category.

Dùng khi:

- Admin muốn ẩn category khỏi bộ lọc public nhưng giữ dữ liệu lịch sử.

## 5. API quản trị Roadmap

### 5.1. `GET /api/admin/roadmaps`

Liệt kê roadmap cho Admin, gồm mọi trạng thái.

- Controller: `AdminRoadmapsController.List`
- Service: `IRoadmapService.ListAsync(query, admin: true)`
- Auth: `AdminPolicy`
- Query: `RoadmapQuery`
- Response: `PageResponse<RoadmapSummary>`

Behavior:

- Trả `Draft`, `Published`, `Archived`.
- Hỗ trợ `search`, `categoryId`, `level`, `status`, `page`, `pageSize`.
- `nodeCount` tính cả inactive node với Admin.
- Dùng cùng response shape với Member để FE tái sử dụng table/card.

Dùng khi:

- Admin xem dashboard danh sách roadmap.
- Admin filter roadmap nháp hoặc archived để xử lý.

### 5.2. `GET /api/admin/roadmaps/{id}`

Xem chi tiết roadmap theo ID cho Admin.

- Controller: `AdminRoadmapsController.Get`
- Service: `IRoadmapService.GetAsync(id)`
- Auth: `AdminPolicy`
- Response: `RoadmapResponse`

Behavior:

- Admin xem được cả `Draft`, `Published`, `Archived`.
- Response gồm tất cả node/edge, kể cả inactive, để quản trị hoặc khôi phục.
- Nếu roadmap không tồn tại, trả `404`.

Dùng khi:

- Admin mở editor roadmap theo ID.
- FE cần tải graph đầy đủ để chỉnh sửa.

### 5.3. `POST /api/admin/roadmaps`

Tạo roadmap mới.

- Controller: `AdminRoadmapsController.Create`
- Service: `IRoadmapService.CreateAsync`
- Auth: `AdminPolicy`
- Request body: `RoadmapRequest`
- Response: `201 Created` + `RoadmapResponse`

Request chính:

```json
{
  "categoryId": "uuid",
  "title": "Frontend",
  "slug": "frontend",
  "shortDescription": "Lộ trình phát triển giao diện web",
  "description": "Nội dung chi tiết",
  "thumbnailUrl": "/uploads/roadmaps/frontend.webp",
  "level": "Beginner",
  "estimatedDuration": "6 tháng",
  "prerequisites": "Không yêu cầu",
  "sortOrder": 0
}
```

Behavior:

- Category phải tồn tại và đang active.
- Slug roadmap là duy nhất toàn hệ thống.
- Roadmap mới hiện mặc định `Published` và có `PublishedAtUtc` theo quyết định triển khai Sprint 3.
- Response trả graph rỗng nếu chưa có node/edge.

Dùng khi:

- Admin tạo một roadmap mới trước khi thêm node/resource.

### 5.4. `PATCH /api/admin/roadmaps/{id}`

Cập nhật metadata roadmap.

- Controller: `AdminRoadmapsController.Update`
- Service: `IRoadmapService.UpdateAsync`
- Auth: `AdminPolicy`
- Request body: `RoadmapRequest`
- Response: `RoadmapResponse`

Behavior:

- Cho phép đổi category, title, slug, mô tả, level, thumbnail, duration, prerequisites, sortOrder.
- Category mới phải active.
- Slug mới không được trùng roadmap khác.
- Không tự thay đổi node/edge/resource.
- Response trả chi tiết roadmap sau cập nhật.

Dùng khi:

- Admin sửa thông tin hiển thị hoặc di chuyển roadmap sang category khác.

### 5.5. `PATCH /api/admin/roadmaps/{id}/status`

Đổi trạng thái roadmap.

- Controller: `AdminRoadmapsController.Status`
- Service: `IRoadmapService.SetStatusAsync`
- Auth: `AdminPolicy`
- Request body: `RoadmapStatusRequest`
- Response: `204 No Content`

Request:

```json
{ "status": "Archived" }
```

Behavior:

- Status hợp lệ: `Draft`, `Published`, `Archived`.
- Chuyển từ `Draft`/`Archived` sang `Published` cần ít nhất một node active.
- Publish lại không đổi `PublishedAtUtc` đầu tiên.
- `Draft` bị ẩn với Member.
- `Archived` vẫn xem/tải được với Member nhưng không cho thêm node/edge/resource mới.

Dùng khi:

- Admin publish roadmap, đưa về draft, hoặc archive roadmap đã cũ.

### 5.6. `PATCH /api/admin/roadmaps/reorder`

Sắp xếp thứ tự roadmap toàn hệ thống.

- Controller: `AdminRoadmapsController.Reorder`
- Service: `IRoadmapService.ReorderAsync`
- Auth: `AdminPolicy`
- Request body: `ReorderRequest`
- Response: `204 No Content`

Request:

```json
{ "ids": ["uuid-2", "uuid-1", "uuid-3"] }
```

Behavior:

- Phải gửi toàn bộ roadmap ID trong collection, mỗi ID đúng một lần.
- Thiếu, thừa hoặc trùng ID trả `400`.
- Thứ tự array trở thành `SortOrder` từ `0`.
- Chạy trong transaction để tránh cập nhật một phần.

Dùng khi:

- Admin kéo thả reorder roadmap trên màn hình quản trị.

## 6. API quản trị Node

### 6.1. `GET /api/admin/roadmaps/{roadmapId}/nodes`

Liệt kê tất cả node thuộc roadmap.

- Controller: `AdminRoadmapNodesController.List`
- Service: `IRoadmapNodeService.ListAsync`
- Auth: `AdminPolicy`
- Response: `NodeResponse[]`

Behavior:

- Trả cả active và inactive node.
- Sắp xếp theo `SortOrder`, sau đó `Id`.
- Mỗi node có `resourceCount`, position, width, color, icon.

Dùng khi:

- Admin editor cần tải node để vẽ canvas.

### 6.2. `GET /api/admin/roadmaps/{roadmapId}/nodes/{nodeId}`

Xem chi tiết một node cho Admin.

- Controller: `AdminRoadmapNodesController.Get`
- Service: `IRoadmapNodeService.GetAsync`
- Auth: `AdminPolicy`
- Response: `NodeDetailResponse`

Behavior:

- Admin xem được node inactive.
- Response gồm toàn bộ resource của node, kể cả inactive.
- Response có `prerequisites` và `nextNodes` dựa trên edge liên quan.
- Nếu node không thuộc roadmap, trả `404`.

Dùng khi:

- Admin mở panel chi tiết node để chỉnh mục tiêu học tập/tài liệu.

### 6.3. `POST /api/admin/roadmaps/{roadmapId}/nodes`

Tạo node mới trong roadmap.

- Controller: `AdminRoadmapNodesController.Create`
- Service: `IRoadmapNodeService.CreateAsync`
- Auth: `AdminPolicy`
- Request body: `NodeRequest`
- Response: `201 Created` + `NodeResponse`

Request chính:

```json
{
  "title": "HTML và CSS",
  "slug": "html-va-css",
  "nodeType": "Topic",
  "positionX": 320,
  "positionY": 180,
  "width": 240,
  "description": "Kiến thức giao diện web cơ bản",
  "learningObjectives": "Xây dựng được trang web responsive cơ bản",
  "estimatedDuration": "4 tuần",
  "color": "#FFE19A",
  "icon": "html",
  "sortOrder": 0
}
```

Behavior:

- Roadmap phải tồn tại.
- Không cho thêm node vào roadmap `Archived`.
- Slug node duy nhất trong cùng roadmap.
- Tọa độ âm hợp lệ; tọa độ bị giới hạn theo `numeric(18,4)`.
- Width nếu có phải dương.

Dùng khi:

- Admin thêm topic/group/milestone vào visual roadmap.

### 6.4. `PATCH /api/admin/roadmaps/{roadmapId}/nodes/{nodeId}`

Cập nhật node.

- Controller: `AdminRoadmapNodesController.Update`
- Service: `IRoadmapNodeService.UpdateAsync`
- Auth: `AdminPolicy`
- Request body: `NodeRequest`
- Response: `NodeResponse`

Behavior:

- Cho phép đổi title, slug, type, position, width, description, objectives, duration, color, icon, sortOrder.
- Slug mới không được trùng node khác trong cùng roadmap.
- Nếu `nodeId` không thuộc `roadmapId`, trả `404`.
- Không tự thay đổi edge/resource liên quan.

Dùng khi:

- Admin chỉnh nội dung hoặc layout của một node.

### 6.5. `PATCH /api/admin/roadmaps/{roadmapId}/nodes/{nodeId}/status`

Bật/tắt node.

- Controller: `AdminRoadmapNodesController.Status`
- Service: `IRoadmapNodeService.SetStatusAsync`
- Auth: `AdminPolicy`
- Request body: `ActiveStatusRequest`
- Response: `204 No Content`

Request:

```json
{ "isActive": false }
```

Behavior:

- Không xóa node khỏi database.
- Node inactive bị ẩn với Member.
- Edge nối tới node inactive cũng bị ẩn với Member.
- Resource trong node inactive không tải được với Member.
- Admin vẫn xem được node/resource/edge liên quan.

Dùng khi:

- Admin tạm ẩn một node khỏi roadmap public nhưng vẫn giữ dữ liệu.

### 6.6. `PATCH /api/admin/roadmaps/{roadmapId}/nodes/positions`

Lưu nhiều tọa độ node sau thao tác kéo thả trên canvas.

- Controller: `AdminRoadmapNodesController.Positions`
- Service: `IRoadmapNodeService.SavePositionsAsync`
- Auth: `AdminPolicy`
- Request body: `NodePositionsRequest`
- Response: `204 No Content`

Request:

```json
{
  "nodes": [
    { "id": "uuid-1", "positionX": 320, "positionY": 180 },
    { "id": "uuid-2", "positionX": 560, "positionY": 180 }
  ]
}
```

Behavior:

- Mỗi node ID chỉ được xuất hiện một lần.
- Tất cả node trong request phải thuộc cùng `roadmapId`.
- Request rỗng, ID trùng, ID ngoài roadmap hoặc tọa độ vượt giới hạn trả `400`.
- Width hiện có được giữ nguyên.
- Chạy trong transaction; lỗi giữa chừng rollback toàn bộ batch.

Dùng khi:

- FE lưu vị trí sau khi admin kéo thả nhiều node trên visual editor.

## 7. API quản trị Edge

### 7.1. `GET /api/admin/roadmaps/{roadmapId}/edges`

Liệt kê đường nối của roadmap.

- Controller: `AdminRoadmapEdgesController.List`
- Service: `IRoadmapEdgeService.ListAsync`
- Auth: `AdminPolicy`
- Response: `EdgeResponse[]`

Behavior:

- Trả cả active và inactive edge nếu có.
- Sắp xếp theo `SortOrder`, sau đó `Id`.
- Dùng cùng với node list để admin editor dựng graph đầy đủ.

Dùng khi:

- Admin mở visual editor và cần vẽ các đường nối.

### 7.2. `POST /api/admin/roadmaps/{roadmapId}/edges`

Tạo đường nối giữa hai node.

- Controller: `AdminRoadmapEdgesController.Create`
- Service: `IRoadmapEdgeService.CreateAsync`
- Auth: `AdminPolicy`
- Request body: `EdgeRequest`
- Response: `201 Created` + `EdgeResponse`

Request:

```json
{
  "sourceNodeId": "uuid-source",
  "targetNodeId": "uuid-target",
  "relationType": "Required",
  "lineStyle": "Solid",
  "label": "Học trước",
  "sortOrder": 0
}
```

Behavior:

- Source và target phải là hai node khác nhau trong cùng roadmap.
- Không cho self-loop.
- Không cho duplicate edge cùng `(RoadmapId, SourceNodeId, TargetNodeId, RelationType)`.
- `Required` edge không được tạo cycle.
- `Recommended`/`Optional` có thể tạo vòng.
- `Required` bắt buộc line style `Solid`.
- Không cho thêm edge mới vào roadmap `Archived`.

Dùng khi:

- Admin nối các node để biểu diễn thứ tự học hoặc quan hệ gợi ý.

### 7.3. `PATCH /api/admin/roadmaps/{roadmapId}/edges/{edgeId}`

Cập nhật đường nối.

- Controller: `AdminRoadmapEdgesController.Update`
- Service: `IRoadmapEdgeService.UpdateAsync`
- Auth: `AdminPolicy`
- Request body: `EdgeRequest`
- Response: `EdgeResponse`

Behavior:

- Cho phép đổi source, target, relationType, lineStyle, label, sortOrder.
- Vẫn kiểm tra node cùng roadmap, self-loop, duplicate và Required cycle.
- Khi kiểm tra cycle, loại chính edge đang cập nhật khỏi tập cạnh hiện có.
- Nếu edge không thuộc roadmap, trả `404`.

Dùng khi:

- Admin sửa quan hệ giữa hai node hoặc đổi loại quan hệ.

### 7.4. `DELETE /api/admin/roadmaps/{roadmapId}/edges/{edgeId}`

Xóa một edge khỏi roadmap.

- Controller: `AdminRoadmapEdgesController.Delete`
- Service: `IRoadmapEdgeService.DeleteAsync`
- Auth: `AdminPolicy`
- Response: `204 No Content`

Behavior:

- Edge được xóa khỏi database.
- Chỉ xóa edge, không xóa node/resource.
- Nếu edge không thuộc roadmap, trả `404`.

Dùng khi:

- Admin bỏ quan hệ giữa hai node.

## 8. API quản trị Resource

### 8.1. `GET /api/admin/roadmap-nodes/{nodeId}/resources`

Liệt kê tài liệu của một node.

- Controller: `AdminRoadmapResourcesController.List`
- Service: `ILearningResourceService.ListAsync`
- Auth: `AdminPolicy`
- Response: `ResourceResponse[]`

Behavior:

- Trả cả link/file resource, gồm inactive.
- Sắp xếp theo `SortOrder`, sau đó `Id`.
- Response file có `downloadUrl`, `originalFileName`, `fileSize`, `contentType`.
- Response link có `externalUrl`.
- Không trả `StoredFileName` hoặc `StorageKey`.

Dùng khi:

- Admin quản lý tài liệu trong một node.

### 8.2. `POST /api/admin/roadmap-nodes/{nodeId}/resources/links`

Tạo tài liệu dạng link.

- Controller: `AdminRoadmapResourcesController.Link`
- Service: `ILearningResourceService.CreateLinkAsync`
- Auth: `AdminPolicy`
- Request body: `LinkResourceRequest`
- Response: `201 Created` + `ResourceResponse`

Request:

```json
{
  "title": "Tài liệu HTML cơ bản",
  "description": "Dành cho người mới",
  "externalUrl": "https://example.com/html",
  "sortOrder": 0
}
```

Behavior:

- Node phải tồn tại.
- Roadmap chứa node không được ở trạng thái `Archived` nếu đang thêm resource mới.
- `externalUrl` bắt buộc là HTTP/HTTPS URL hợp lệ.
- Resource tạo ra có `ResourceType = Link` và không có file metadata.

Dùng khi:

- Admin gắn link học liệu bên ngoài vào node.

### 8.3. `POST /api/admin/roadmap-nodes/{nodeId}/resources/files`

Upload tài liệu dạng file.

- Controller: `AdminRoadmapResourcesController.Upload`
- Service: `ILearningResourceService.CreateFileAsync`
- Auth: `AdminPolicy`
- Content-Type: `multipart/form-data`
- Response: `201 Created` + `ResourceResponse`

Form data:

```text
title

description
sortOrder
file
```

Behavior:

- Node phải tồn tại.
- Roadmap chứa node không được `Archived` nếu đang thêm file mới.
- `file` bắt buộc và không rỗng.
- Giới hạn multipart request tối đa 21 MiB; file mặc định tối đa 20 MiB theo `RoadmapStorage`.
- Cho phép: PDF, PNG, JPEG, TXT, MD, DOCX, PPTX, XLSX.
- Kiểm tra extension, MIME type và signature/nội dung.
- File được lưu vào private storage ngoài `wwwroot`.
- Nếu lưu database thất bại sau upload, service dọn file mới nếu chưa được tham chiếu.
- Response không lộ storage path/key.

Dùng khi:

- Admin upload tài liệu nội bộ cho node.

### 8.4. `PATCH /api/admin/roadmap-resources/{id}`

Cập nhật metadata resource.

- Controller: `AdminRoadmapResourcesController.Update`
- Service: `ILearningResourceService.UpdateAsync`
- Auth: `AdminPolicy`
- Request body: `UpdateResourceRequest`
- Response: `ResourceResponse`

Request:

```json
{
  "title": "Guide đã cập nhật",
  "description": "Mô tả mới",
  "externalUrl": "https://example.com/updated",
  "sortOrder": 1
}
```

Behavior:

- Link resource bắt buộc có `externalUrl` HTTP/HTTPS hợp lệ khi cập nhật.
- File resource không được có `externalUrl`.
- Endpoint này không đổi loại resource.
- Endpoint này không thay file; thay file dùng `replace-file`.

Dùng khi:

- Admin sửa tiêu đề/mô tả/link/thứ tự của tài liệu.

### 8.5. `POST /api/admin/roadmap-resources/{id}/replace-file`

Thay file vật lý cho một file resource hiện có.

- Controller: `AdminRoadmapResourcesController.Replace`
- Service: `ILearningResourceService.ReplaceFileAsync`
- Auth: `AdminPolicy`
- Content-Type: `multipart/form-data`
- Response: `ResourceResponse`

Form data:

```text
file
```

Behavior:

- Chỉ áp dụng cho resource loại `File`.
- Gọi trên resource loại `Link` trả `409 Conflict`.
- Metadata title/description/sortOrder hiện có được giữ nguyên.
- File mới phải qua cùng kiểm tra size, extension, MIME và content signature như upload.
- Nếu replace thành công: commit metadata mới trước, sau đó dọn file cũ nếu không còn tham chiếu.
- Nếu replace thất bại: giữ metadata và file cũ.

Dùng khi:

- Admin cập nhật phiên bản mới của tài liệu đã upload.

### 8.6. `PATCH /api/admin/roadmap-resources/{id}/status`

Bật/tắt resource.

- Controller: `AdminRoadmapResourcesController.Status`
- Service: `ILearningResourceService.SetStatusAsync`
- Auth: `AdminPolicy`
- Request body: `ActiveStatusRequest`
- Response: `204 No Content`

Request:

```json
{ "isActive": false }
```

Behavior:

- Không xóa metadata/file.
- Resource inactive bị ẩn với Member.
- File resource inactive không tải được với Member.
- Admin vẫn thấy và tải được nếu file còn tồn tại.

Dùng khi:

- Admin tạm ẩn một tài liệu khỏi roadmap public.

### 8.7. `DELETE /api/admin/roadmap-resources/{id}`

Soft-delete resource.

- Controller: `AdminRoadmapResourcesController.Delete`
- Service: `ILearningResourceService.DeleteAsync`
- Auth: `AdminPolicy`
- Response: `204 No Content`

Behavior:

- Endpoint này gọi cùng logic với status `isActive = false`.
- Không hard-delete row `LearningResource`.
- Không xóa file vật lý ngay lập tức.
- Giữ metadata để admin có thể kiểm tra/khôi phục bằng status endpoint.

Dùng khi:

- Admin muốn xóa tài liệu khỏi trải nghiệm Member nhưng vẫn giữ audit/data an toàn.

### 8.8. `PATCH /api/admin/roadmap-nodes/{nodeId}/resources/reorder`

Sắp xếp tài liệu trong một node.

- Controller: `AdminRoadmapResourcesController.Reorder`
- Service: `ILearningResourceService.ReorderAsync`
- Auth: `AdminPolicy`
- Request body: `ReorderRequest`
- Response: `204 No Content`

Request:

```json
{ "ids": ["resource-2", "resource-1", "resource-3"] }
```

Behavior:

- Phải gửi toàn bộ resource ID thuộc node đó, kể cả inactive resource.
- Thiếu, thừa hoặc trùng ID trả `400`.
- Resource thuộc node khác trả `400`.
- Thứ tự array trở thành `SortOrder` từ `0`.
- Chạy trong transaction để tránh cập nhật một phần.

Dùng khi:

- Admin kéo thả reorder danh sách tài liệu của node.

## 9. Response model chính

### `CategoryResponse`

Dùng bởi category list/create/update.

```text
id, name, slug, description, icon, color, sortOrder, isActive
```

### `RoadmapSummary`

Dùng bởi list roadmap Member/Admin.

```text
id, title, slug, shortDescription, thumbnailUrl, category, level,
estimatedDuration, status, sortOrder, nodeCount, sectionCount
```

`sectionCount` là alias tương thích với contract cũ, giá trị bằng `nodeCount`.

### `RoadmapResponse`

Dùng bởi detail roadmap và create/update roadmap.

```text
id, title, slug, shortDescription, description, thumbnailUrl, category,
level, estimatedDuration, prerequisites, status, sortOrder,
publishedAtUtc, nodes, edges
```

### `NodeResponse`

Dùng bởi node list/create/update và nằm trong roadmap detail.

```text
id, title, slug, description, nodeType, position, width, color,
icon, sortOrder, isActive, resourceCount
```

### `NodeDetailResponse`

Dùng bởi endpoint chi tiết node Member/Admin.

```text
node, learningObjectives, estimatedDuration, resources, prerequisites, nextNodes
```

### `EdgeResponse`

Dùng bởi edge list/create/update và nằm trong roadmap detail.

```text
id, sourceNodeId, targetNodeId, relationType, lineStyle, label, sortOrder, isActive
```

### `ResourceResponse`

Dùng bởi resource list/create/update/replace.

```text
id, title, description, resourceType, externalUrl, originalFileName,
fileSize, contentType, downloadUrl, sortOrder, isActive
```

Không có `StoredFileName` hoặc `StorageKey` trong response.

## 10. Luồng sử dụng điển hình

### Luồng Member xem roadmap

```text
GET /api/roadmap-categories
→ GET /api/roadmaps?categoryId=&level=&page=1&pageSize=12
→ GET /api/roadmaps/{slug}
→ GET /api/roadmaps/{roadmapId}/nodes/{nodeId}
→ GET /api/roadmap-resources/{id}/download
```

### Luồng Admin tạo roadmap visual

```text
GET /api/admin/roadmap-categories
→ POST /api/admin/roadmaps
→ POST /api/admin/roadmaps/{roadmapId}/nodes
→ POST /api/admin/roadmaps/{roadmapId}/edges
→ POST /api/admin/roadmap-nodes/{nodeId}/resources/links
→ POST /api/admin/roadmap-nodes/{nodeId}/resources/files
→ PATCH /api/admin/roadmaps/{roadmapId}/nodes/positions
→ PATCH /api/admin/roadmaps/{roadmapId}/status
```

### Luồng Admin bảo trì tài liệu

```text
GET /api/admin/roadmap-nodes/{nodeId}/resources
→ PATCH /api/admin/roadmap-resources/{id}
→ POST /api/admin/roadmap-resources/{id}/replace-file
→ PATCH /api/admin/roadmap-nodes/{nodeId}/resources/reorder
→ PATCH /api/admin/roadmap-resources/{id}/status
```

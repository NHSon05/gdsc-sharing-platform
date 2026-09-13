# Sprint 3 — Kế hoạch triển khai Roadmap

## Phase 1 — Domain và Database

- [x] Tạo enum `RoadmapStatus`, `RoadmapLevel` và `ResourceType`.
- [x] Tạo entity `RoadmapCategory`.
- [x] Tạo entity `Roadmap`.
- [x] Tạo entity `RoadmapNode`.
- [x] Tạo entity `RoadmapEdge`.
- [x] Tạo entity `LearningResource`.
- [x] Cấu hình quan hệ và index trong EF Core.
- [x] Tạo migration.
- [x] Seed sáu danh mục ban đầu theo cơ chế idempotent.

Kết quả đầu ra:

- Domain model hợp lệ.
- Migration chạy được trên database sạch và database đã có Sprint 2.
- Không hard-delete dữ liệu có quan hệ.

Đã triển khai và kiểm tra: 198 unit test + 39 integration test pass (bao gồm PostgreSQL thật). Xem [ghi chú Phase 1](07_PHASE_1_NOTES.md) về các quyết định và cách chạy lại.

## Phase 2 — Application Layer

- [x] Tạo request và response DTO.
- [x] Tạo validator cho Category, Roadmap, Node, Edge và Resource.
- [x] Tạo interface service hoặc repository.
- [x] Tạo `IFileStorage`.
- [x] Tạo mapping entity sang DTO node–edge.
- [x] Cài đặt quy tắc publish, archive và reorder.
- [x] Chuẩn hóa lỗi nghiệp vụ.

## Phase 3 — Infrastructure

- [x] Cài đặt Category service.
- [x] Cài đặt Roadmap service.
- [x] Cài đặt Node service và Edge service.
- [x] Cài đặt Resource service.
- [x] Cài đặt local file storage cho development.
- [x] Bảo đảm transaction khi lưu hàng loạt vị trí node.
- [x] Kiểm tra self-loop, duplicate edge và Required cycle.
- [x] Bảo đảm cơ chế rollback và dọn tệp.
- [x] Map unique constraint sang 409 Conflict.

## Phase 4 — API

- [x] Tạo controller dành cho Member.
- [x] Tạo controller quản trị.
- [x] Áp dụng authentication và `AdminPolicy`.
- [x] Cấu hình upload limit.
- [x] Tạo endpoint download có kiểm tra quyền.
- [x] Chuẩn hóa Problem Details và `traceId`.
- [x] Bổ sung Swagger request, response và authorization.

Đã hoàn tất Phase 2–4. Build không warning/error; 268 test pass (217 unit, 51 integration), gồm PostgreSQL thật và regression. Xem [ghi chú Phase 2–4](08_PHASE_2_4_NOTES.md).

## Phase 5 — Testing

- [x] Viết Unit Test cho domain.
- [x] Viết Unit Test cho validator.
- [x] Viết Integration Test cho Member API.
- [x] Viết Integration Test cho Admin API.
- [x] Kiểm thử upload, download và replace file.
- [x] Kiểm thử rollback và race condition.
- [x] Chạy regression test cho Authentication và Profile.

Đã hoàn tất Phase 5. Xác nhận ngày 2026-09-11 với PostgreSQL thật: 329 test pass (271 unit, 58 integration), 0 failed, 0 skipped, bao gồm regression Authentication/Profile và kiểm tra PATCH/tên nhóm Swagger. Xem [ghi chú Phase 5](09_PHASE_5_NOTES.md).

## Thứ tự triển khai bắt buộc

```text
Domain
→ Persistence
→ Application contract
→ Infrastructure service
→ API
→ Unit Test
→ Integration Test
→ Documentation
```

## Definition of Done

Sprint 3 hoàn thành khi:

- Migration chạy thành công.
- Seeder không tạo dữ liệu trùng khi chạy lại.
- Admin quản lý được danh mục, roadmap, node, edge và tài liệu.
- Member xem được roadmap Published và Archived; Draft bị ẩn.
- API trả đủ tọa độ node và đường nối để tái tạo đúng sơ đồ.
- Hỗ trợ tài liệu Link và File.
- Upload kiểm tra dung lượng, extension, MIME type và signature.
- Download kiểm tra quyền tại backend.
- Không có hard-delete ngoài quy tắc.
- Lỗi nghiệp vụ được ánh xạ đúng mã HTTP.
- Unit Test và Integration Test đều pass.
- Swagger mô tả đầy đủ API.
- Authentication và Profile của các sprint trước không bị ảnh hưởng.

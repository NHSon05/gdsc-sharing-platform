# Roadmap — Phase 5 Testing

## Phạm vi đã xác nhận

Phase 5 tập trung khóa lại bộ test cho Roadmap Management sau khi Phase 2–4 đã có DTO, validator, service, storage và API.

Các nhóm test hiện có và đã bổ sung bao phủ:

- Domain behavior: roadmap status, publish/archive, node layout, edge self-loop, resource Link/File, mutation rollback trong entity.
- Validator boundary: category, roadmap, node, edge, resource, status, reorder, node positions, query pagination, metadata length và giới hạn payload bulk 10.000 item.
- Member API: danh sách category active, danh sách roadmap Published/Archived, Draft bị ẩn, detail graph node–edge, node detail, resource visibility.
- Admin API: quản lý category/roadmap/node/edge/resource, status, reorder, batch positions, filter đủ mọi trạng thái gồm Draft.
- File workflow: upload, download, byte range, replace file, MIME/extension/signature, request quá dung lượng, missing file, private storage không lộ metadata hoặc static path.
- Rollback/race condition: rollback batch position khi database lỗi, dọn file mới khi SaveChanges lỗi, giữ file cũ khi replace lỗi, unique violation map sang conflict, concurrent Required edge không tạo cycle.
- Regression Authentication/Profile: login, refresh token, cookie token flow, authorization guard, profile update, avatar, email change.

## Test được bổ sung trong Phase 5

- `RoadmapValidatorBoundaryTests.MetadataLengthLimits_AreEnforcedAcrossRequests`
  - Kiểm tra giới hạn description/icon/color/estimated duration/label/thumbnail/resource metadata theo validator.
- `RoadmapValidatorBoundaryTests.BulkOperationValidatorsLimitPayloadSize`
  - Kiểm tra `ReorderRequest` và `NodePositionsRequest` nhận tối đa 10.000 phần tử và từ chối 10.001 phần tử.
- `RoadmapAccessAndBoundaryTests.AdminPaginationIncludesEveryStatusAndCanFilterDrafts`
  - Kiểm tra Admin list thấy đủ `Draft`, `Archived`, `Published`, phân trang đúng và filter `status=Draft` không bị ẩn như Member API.

## Cách chạy kiểm tra

Từ root repo:

```bash
dotnet test backend/GdscSharingPlatform.slnx --no-restore
```

Chạy riêng unit test:

```bash
dotnet test backend/tests/GdscSharingPlatform.UnitTests/GdscSharingPlatform.UnitTests.csproj --no-restore --logger "console;verbosity=minimal"
```

Chạy riêng integration test:

```bash
dotnet test backend/tests/GdscSharingPlatform.IntegrationTests/GdscSharingPlatform.IntegrationTests.csproj --no-restore --logger "console;verbosity=minimal"
```

Chạy riêng regression Authentication/Profile:

```bash
dotnet test backend/tests/GdscSharingPlatform.IntegrationTests/GdscSharingPlatform.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~GdscSharingPlatform.IntegrationTests.Auth|FullyQualifiedName~GdscSharingPlatform.IntegrationTests.Profile" --logger "console;verbosity=minimal"
```

Các test PostgreSQL thật cần biến môi trường `ROADMAP_TEST_POSTGRES` trỏ đến PostgreSQL disposable có quyền tạo database:

```bash
ROADMAP_TEST_POSTGRES='Host=127.0.0.1;Port=55439;Username=postgres;Database=postgres' \
  dotnet test backend/GdscSharingPlatform.slnx --no-restore
```

Nếu thiếu biến này, các test đánh dấu `PostgresFact`/`PostgresTheory` sẽ skip theo thiết kế. Các test này kiểm tra migration thật, constraint, rollback PostgreSQL và race condition transaction mà InMemory provider không mô phỏng đầy đủ.

## Kết quả xác nhận 2026-09-11

Đã chạy toàn bộ solution với `ROADMAP_TEST_POSTGRES` trên PostgreSQL 17 riêng cho kiểm thử.
Roadmap API tests cũng dùng PostgreSQL khi có biến môi trường này, thay vì InMemory.

- Build: 0 warning, 0 error.
- Unit tests: 271 pass, 0 failed, 0 skipped.
- Integration tests: 58 pass, 0 failed, 0 skipped.
- Toàn bộ backend: 329 test pass, 0 failed, 0 skipped.
- Regression Authentication/Profile nằm trong lần chạy toàn bộ này.
- Đã xác nhận 8 endpoint cập nhật/reorder dùng PATCH và tên nhóm Swagger có khoảng trắng, ví dụ `Admin Roadmap Categories`.

Phase 5 đã được xác nhận với PostgreSQL thật. Database ứng dụng không được thay đổi; database/container kiểm thử đã được dọn sau khi chạy.

# Roadmap — Ghi chú triển khai Phase 1

## Quyết định đã chốt

- Roadmap mới mặc định `Published`, kể cả khi chưa có node; `PublishedAtUtc` được gán bằng thời điểm tạo. Đây là lựa chọn người dùng xác nhận, ưu tiên hơn kỳ vọng `Draft` trong test plan.
- Seed đúng sáu danh mục theo `02_DATA_MODEL.md` và Phase 1: Frontend, Backend, Database, Software Engineering, Fullstack, Artificial Intelligence. Danh sách 16 danh mục trong yêu cầu nghiệp vụ chưa áp dụng vào seed Phase 1.
- Khi bắt đầu Phase 2–4, người dùng xác nhận Member được xem và tải cả Published lẫn Archived. Xem ghi chú Phase 2–4 để biết triển khai hiện tại.

## Domain và persistence

- Năm entity nằm trong `Domain/Roadmaps`, enum nằm trong `Domain/Enums`.
- Bổ sung enum `RoadmapNodeType`, `RoadmapRelationType`, `RoadmapLineStyle` để biểu diễn đầy đủ mô hình node–edge.
- Slug được trim, chuyển chữ thường, bỏ dấu tiếng Việt và chuẩn hóa dấu gạch nối; tối đa 150 ký tự.
- `Title`/tên danh mục tối đa 150, `ShortDescription` tối đa 500, `SortOrder >= 0`.
- Tọa độ và width lưu `numeric(18,4)`; tọa độ âm hợp lệ, width nếu có phải dương.
- Tất cả quan hệ mới dùng `Restrict`, bao gồm người tạo/người cập nhật. Không cascade-delete nội dung từ user, category, roadmap hoặc node.
- Khóa ngoại ghép từ edge đến `(RoadmapId, Id)` của node ngăn đường nối khác roadmap. Unique index ngăn slug/edge trùng, kể cả dữ liệu đã vô hiệu hóa.
- Check constraint ngăn self-loop, enum ngoài miền giá trị, thứ tự âm, width không hợp lệ và metadata Link/File lẫn nhau hoặc thiếu trường bắt buộc.
- Domain chỉ chấp nhận URL HTTP/HTTPS; service storage ở phase sau chịu trách nhiệm bảo đảm tệp đã lưu thành công trước khi gọi `CreateFile`.
- Seeder chạy trong luồng `DatabaseSeeder.SeedAsync`, chỉ thêm slug chưa tồn tại và không ghi đè tên, thứ tự, mô tả hoặc trạng thái danh mục hiện có.

Migration: `20260909190532_AddRoadmapManagement`. Migration chỉ thêm năm bảng roadmap cùng ràng buộc/index, không sửa migration Sprint 2.

## Chạy kiểm tra

Từ thư mục `backend`:

```bash
dotnet build --no-restore --disable-build-servers -m:1
dotnet test --no-restore --disable-build-servers -m:1
```

Test PostgreSQL thật dùng biến môi trường `ROADMAP_TEST_POSTGRES`. Tài khoản test cần quyền tạo database; mỗi test tự tạo database `roadmap_tests_<uuid>` và xóa sau khi hoàn tất. Nếu thiếu biến này, các test PostgreSQL được báo skipped; các test InMemory không thay thế được việc kiểm tra constraint hoặc migration thật.

Ví dụ với PostgreSQL cục bộ chuyên dùng cho kiểm thử:

```bash
ROADMAP_TEST_POSTGRES='Host=127.0.0.1;Port=55439;Username=postgres;Database=postgres'   dotnet test --no-restore --disable-build-servers -m:1
```

Test bao gồm database sạch, nâng cấp từ Sprint 2 có dữ liệu, rollback/reapply migration, seed hai lần, lưu/đọc graph, bảo toàn nội dung khi vô hiệu hóa, unique constraint, cross-roadmap FK, metadata Link/File và chống hard-delete.

## Phạm vi còn lại

Phase 2–4 tiếp tục DTO/validator/service/API, chuyển trạng thái, danh mục hoạt động, quyền Member/Admin, lọc nội dung theo trạng thái, phát hiện Required cycle, reorder và lưu trữ/upload/download tệp. Phase 1 chưa cung cấp endpoint hay giao diện roadmap.

## Kết quả kiểm tra

- Build thành công, 0 warning và 0 error.
- 198 unit test và 39 integration test pass, 0 skipped khi chạy với PostgreSQL 17 tạm.
- Regression Authentication, refresh-token rotation, Profile và Membership vẫn pass.
- Database ứng dụng hiện tại chưa được cập nhật; migration chỉ được áp dụng trên database kiểm thử tạm.

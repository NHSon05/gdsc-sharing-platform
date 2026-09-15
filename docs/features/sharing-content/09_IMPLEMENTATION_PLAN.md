# Sprint 4 — Kế hoạch triển khai

Phase 3–5 đã triển khai backend. Hành vi API và phạm vi kiểm thử được ghi tại [10_PHASE_3_5_NOTES.md](./10_PHASE_3_5_NOTES.md). Migration được xác minh trên PostgreSQL kiểm thử; triển khai database ứng dụng là bước riêng.

## Phase 1 — Domain

- Tạo enum trạng thái, loại tài nguyên, hình thức tổ chức và vai trò Presenter.
- Tạo `SharingContent`, `SharingContentAuthor`, `SharingResource`.
- Tạo `SharingTag`, `SharingContentTag`.
- Tạo `SharingSchedule`, `SharingSchedulePresenter`.
- Tạo bảng nối Content và Schedule.
- Tạo bảng audience theo Gen và Department.
- Cài đặt state transition trong domain.

## Phase 2 — Persistence

- Cấu hình EF Core.
- Tạo unique index và query index.
- Cấu hình restrict delete.
- Cấu hình concurrency token.
- Tạo migration.
- Seed các tag mặc định nếu cần.

## Phase 3 — Application

- Tạo DTO request và response.
- Tạo validator.
- Tạo interface service.
- Tạo policy kiểm tra Owner, Contributor, Content Owner và Admin.
- Tự động gán Member tạo lịch làm Presenter của lịch đó.
- Tạo query filter cho audience.
- Tạo kiểm tra trùng lịch Presenter.
- Tái sử dụng `IFileStorage`.

## Phase 4 — Infrastructure

- Cài đặt Content service và review workflow.
- Cài đặt Resource service.
- Cài đặt Tag service.
- Cài đặt Schedule service.
- Cài đặt transaction cho author, audience, presenter và content replacement.
- Map database conflict sang 409.
- Cài đặt audit log.

## Phase 5 — API

- Tạo Member Content API.
- Tạo Admin Review API.
- Tạo Resource API.
- Tạo Member Schedule API.
- Tạo Admin Schedule API.
- Áp dụng authentication, authorization và rate limiting.
- Bổ sung Problem Details và Swagger.

## Phase 6 — Frontend

- Tạo feature `sharing-content` và `sharing-schedule` hoặc public API `sharing` thống nhất.
- Tạo query-key factory, API layer và TanStack hooks.
- Tạo trang danh sách, chi tiết và nội dung của tôi.
- Tạo trình soạn Markdown.
- Tạo hàng chờ duyệt cho Admin.
- Tạo List View và Calendar View cho lịch.
- Không lưu server state trong Zustand.

## Phase 7 — Testing

Đã bổ sung và chạy backend test ngày 2026-09-15: **367 unit + 77 integration passed, 0 skipped**. Xem [11_PHASE_7_TESTING.md](./11_PHASE_7_TESTING.md) để biết phạm vi, lệnh chạy lại và các finding review vẫn còn mở. Kết quả này không thay thế frontend/E2E testing.

- Unit Test domain và validator.
- Integration Test quyền và workflow.
- Test file storage rollback.
- Test audience và trùng lịch.
- Test concurrency.
- Regression Test Sprint 1–3.

## Thứ tự triển khai

```text
Domain
→ Persistence
→ Application contract
→ Infrastructure
→ API
→ Tests backend
→ Frontend API layer
→ Frontend UI
→ Tests frontend
→ Documentation
```

## Definition of Done

- Migration chạy thành công trên database đã có Sprint 1–3.
- Member tạo, sửa và gửi duyệt được content.
- Admin duyệt, từ chối, công khai và lưu trữ được content.
- Một content hỗ trợ nhiều tác giả, tag, link và file.
- Admin tạo và quản lý được schedule.
- Một schedule hỗ trợ nhiều Presenter và nhiều content.
- Audience theo AllMembers, Gen và Department hoạt động đúng.
- Trùng lịch Presenter được phát hiện.
- Thời gian được lưu UTC và hiển thị đúng múi giờ.
- Upload/download tuân thủ quyền và chính sách bảo mật.
- Optimistic concurrency hoạt động.
- Audit log được tạo cho hành động quan trọng.
- Unit Test, Integration Test và Frontend Test đều pass.
- Swagger mô tả đầy đủ request, response và quyền.
- Authentication, Profile và Roadmap không bị ảnh hưởng.

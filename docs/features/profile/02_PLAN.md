## 36. Task Breakdown

### Backend — Domain và Persistence

- [x] Tạo ClubGeneration.
- [x] Mở rộng Department thành danh mục động.
- [x] Tạo ClubRole.
- [x] Tạo ClubMembership.
- [x] Tạo DepartmentMembership.
- [x] Tạo RoleAssignment.
- [x] Cấu hình unique constraints và indexes.
- [x] Cấu hình restrict delete/history behavior.
- [x] Thêm GitHubUrl và Bio vào User.
- [x] Tạo migration mở rộng schema.
- [x] Viết backfill legacy.
- [x] Seed Club Role và Department idempotent.

### Backend — Application và API

- [x] Tạo Profile DTO/validator/service.
- [x] Tạo membership DTO/validator/service.
- [x] Tạo Department CRUD service.
- [x] Tạo Generation CRUD service.
- [x] Tạo ProfileController.
- [x] Tạo lookup endpoints.
- [x] Tạo Admin Department endpoints.
- [x] Tạo Admin Generation endpoints.
- [x] Tạo Admin Member Membership endpoints.
- [x] Thực hiện role replacement transaction.
- [x] Thực hiện avatar storage.
- [x] Thực hiện email change flow.
- [x] Thêm audit.
- [x] Viết Unit Test và Integration Test.

### Frontend

- [ ] Tạo profile feature.
- [ ] Tạo member-management feature.
- [ ] Tạo profile page.
- [ ] Tạo membership history UI.
- [ ] Tạo generation card.
- [ ] Tạo department membership card.
- [ ] Tạo role badge list.
- [ ] Tạo Admin membership editor.
- [ ] Tạo Department CRUD UI.
- [ ] Tạo Gen CRUD UI.
- [ ] Dùng multi-select cho Department và Role.
- [ ] Đồng bộ TanStack Query cache.
- [ ] Hoàn thiện avatar và email flow.
- [ ] Kiểm tra responsive/accessibility.
- [ ] Viết frontend tests.

### Documentation và Operations

- [ ] Cập nhật README.
- [ ] Cập nhật Swagger examples.
- [ ] Tài liệu hóa migration/backfill report.
- [ ] Tài liệu hóa avatar storage.
- [ ] Cập nhật `.env.example` nếu thêm storage/email configuration.

## 37. Definition of Done

- [ ] Một Member tham gia được nhiều Gen.
- [ ] Một Member tham gia được nhiều Department trong từng Gen.
- [ ] Một DepartmentMembership giữ được nhiều Role.
- [ ] Role luôn xác định được Gen và Department tương ứng.
- [ ] Member không tự nâng Club Role.
- [ ] Admin thêm và deactivate Department được.
- [ ] Admin tạo và quản lý Gen được.
- [ ] Lịch sử membership không bị hard delete.
- [ ] Profile cá nhân chỉnh sửa được đúng quyền.
- [ ] Avatar upload/delete hoạt động an toàn.
- [ ] Email change có xác nhận và revoke session cũ.
- [ ] Migration/backfill không làm mất dữ liệu legacy.
- [ ] Seeder idempotent.
- [ ] Response không lộ Identity internal fields.
- [ ] TanStack Query cache cập nhật đúng.
- [ ] UI có loading, empty, validation, conflict và success states.
- [ ] Unit Test pass.
- [ ] Integration Test pass trên PostgreSQL Testcontainers.
- [ ] Frontend lint/build/test pass.
- [ ] Swagger và tài liệu setup được cập nhật.
- [ ] Không có secret/token trong source hoặc log.

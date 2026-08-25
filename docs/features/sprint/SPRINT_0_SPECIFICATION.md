# Đặc tả Sprint 0 – Foundation

## 1. Thông tin Sprint

| Thuộc tính | Nội dung                         |
| ---------- | -------------------------------- |
| Dự án      | GDSC Sharing Platform            |
| Sprint     | Sprint 0 – Foundation            |
| Backend    | ASP.NET Core trên .NET 10        |
| Frontend   | Next.js 16, React 19, TypeScript |
| Database   | PostgreSQL 17                    |
| Trạng thái | Baseline nghiệm thu Sprint 0     |

## 2. Bối cảnh

GDSC Sharing Platform hướng tới việc xây dựng nền tảng chia sẻ tri thức cho cộng đồng với các nhóm chức năng:

- Dashboard
- AI Assistant
- Roadmap
- Sharing Content
- Sharing Schedule
- Social Interaction
- Notification
- User Management

Sprint 0 chưa triển khai các nghiệp vụ trên.

Sprint này tập trung xây dựng nền móng kỹ thuật để các Sprint tiếp theo có thể phát triển chức năng một cách an toàn, nhất quán và có khả năng kiểm thử.

## 3. Sprint Goal

Sau Sprint 0, lập trình viên phải có thể:

1. Clone source code.
2. Cấu hình secret.
3. Khởi động hệ thống bằng Docker Compose.
4. Truy cập API và Swagger.
5. Kiểm tra Health Check.
6. Áp dụng migration.
7. Kiểm tra seed data.
8. Chạy Unit Test.
9. Chạy Integration Test.
10. Phát triển tiếp Sprint 1 mà không cần thay đổi kiến trúc nền tảng.

## 4. Phạm vi thực hiện

### 4.1. Solution architecture

Solution gồm các project:

```text
src/
├── GdscSharing.Api
├── GdscSharing.Application
├── GdscSharing.Domain
└── GdscSharing.Infrastructure

tests/
├── GdscSharing.UnitTests
└── GdscSharing.IntegrationTests
```

Trách nhiệm:

| Project          | Trách nhiệm                                        |
| ---------------- | -------------------------------------------------- |
| Domain           | Entity, enum, constant và business rule            |
| Application      | Use case và application contract                   |
| Infrastructure   | EF Core, PostgreSQL, Identity, migration và seeder |
| API              | HTTP endpoint, middleware và Dependency Injection  |
| UnitTests        | Kiểm tra business rule độc lập                     |
| IntegrationTests | Kiểm tra API và database thật                      |

Nguyên tắc:

- Domain không phụ thuộc Infrastructure.
- Domain không phụ thuộc ASP.NET Core.
- Domain không phụ thuộc Entity Framework Core.
- Infrastructure hiện thực cơ chế lưu trữ dữ liệu.
- API là composition root.
- Dependency phải hướng vào Domain và Application.

### 4.2. Domain và Identity

Sprint 0 triển khai:

- `ApplicationUser`
- `Department`
- `UserStatus`
- `SystemRoles`
- `SystemDepartments`

`ApplicationUser` kế thừa:

```csharp
IdentityUser<Guid>
```

System role:

```text
Admin
Member
```

System Department:

```text
Management
Software
Design
Photography
Media
```

### 4.3. ApplicationUser

`ApplicationUser` chứa các thông tin nền tảng:

- `Id`
- `UserName`
- `Email`
- `DisplayName`
- `StudentCode`
- `DateOfBirth`
- `DepartmentId`
- `Generation`
- `Status`
- `AvatarUrl`
- `TokenVersion`
- `CreatedAtUtc`
- `UpdatedAtUtc`
- `DeletedAtUtc`

Yêu cầu:

- ID sử dụng `Guid`.
- `DisplayName` bắt buộc.
- `DisplayName` tối đa 150 ký tự.
- `StudentCode` tối đa 30 ký tự.
- `StudentCode` có unique index.
- `Generation` tối đa 30 ký tự.
- `AvatarUrl` tối đa 2048 ký tự.
- Email phải duy nhất.
- User hỗ trợ soft delete.

### 4.4. Department

`Department` gồm:

- `Id`
- `Name`
- `IsActive`
- `CreatedAtUtc`
- `UpdatedAtUtc`
- `DeletedAtUtc`

Yêu cầu:

- `Name` bắt buộc.
- Tên được trim trước khi lưu.
- Tên tối đa 100 ký tự.
- Tên có unique index.
- Department hỗ trợ soft delete.
- Department mặc định ở trạng thái active.

### 4.5. Quan hệ dữ liệu

```text
Departments (1) ──────── (0..n) Users
                            │
                            │ n
                            │
                            │ n
                          Roles
```

Quy tắc:

- Một User có thể chưa thuộc Department.
- Một Department có thể có nhiều User.
- `ApplicationUser.DepartmentId` có thể `NULL`.
- Khi Department bị xóa vật lý, `DepartmentId` của User được đặt thành `NULL`.
- User và Role có quan hệ nhiều-nhiều.
- Quan hệ User–Role được lưu trong `UserRoles`.

## 5. ApplicationDbContext

`ApplicationDbContext` phải kế thừa:

```csharp
IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
```

Yêu cầu:

- Sử dụng PostgreSQL.
- Schema mặc định là `gdsc`.
- Cấu hình Identity table name.
- Cấu hình index.
- Cấu hình foreign key.
- Cấu hình delete behavior.
- Cấu hình soft-delete query filter.
- Expose `DbSet<Department>`.

## 6. Phạm vi Migration

Migration đầu tiên:

```text
202608150001_InitialIdentity
```

Migration chỉ được tạo các bảng:

| Bảng                    | Trách nhiệm                  |
| ----------------------- | ---------------------------- |
| `gdsc.Users`            | Identity và hồ sơ người dùng |
| `gdsc.Roles`            | Danh mục role                |
| `gdsc.UserRoles`        | Quan hệ User–Role            |
| `gdsc.UserClaims`       | Claim của user               |
| `gdsc.UserLogins`       | External login               |
| `gdsc.UserTokens`       | Identity token               |
| `gdsc.RoleClaims`       | Claim của role               |
| `gdsc.Departments`      | Danh mục Department          |
| `__EFMigrationsHistory` | Lịch sử migration            |

Migration phải:

- Chạy được trên PostgreSQL rỗng.
- Tạo đúng schema `gdsc`.
- Tạo đầy đủ khóa chính.
- Tạo đúng foreign key.
- Tạo đúng unique index.
- Không chứa bảng ngoài Sprint 0.
- Không còn pending migration sau khi áp dụng.

## 7. Database Seeder

Seeder phải thực hiện theo thứ tự:

```text
Apply Migration
      ↓
Seed Roles
      ↓
Seed Departments
      ↓
Validate Admin Configuration
      ↓
Create Admin
      ↓
Assign Roles
```

### 7.1. Seed role

Role bắt buộc:

```text
Admin
Member
```

Nếu role đã tồn tại, seeder không tạo lại.

### 7.2. Seed Department

Department bắt buộc:

```text
Management
Software
Design
Photography
Media
```

Nếu Department đã tồn tại, seeder không tạo lại.

Việc so sánh tên Department không phân biệt chữ hoa và chữ thường.

### 7.3. Seed Admin

Admin được tạo từ configuration:

```text
AdminSeed:Email
AdminSeed:Password
AdminSeed:DisplayName
```

Admin mặc định:

- Email đã xác nhận.
- Trạng thái `Active`.
- Được gán system roles.
- Không bị tạo lại nếu email đã tồn tại.

Nếu thiếu email hoặc password, ứng dụng phải báo lỗi:

```text
AdminSeed credentials must be provided through configuration.
```

### 7.4. Idempotency

Seeder phải an toàn khi chạy nhiều lần.

Ví dụ:

```text
Lần chạy 1:
- 2 role
- 5 department
- 1 admin
- Quan hệ role tương ứng

Lần chạy 2:
- 2 role
- 5 department
- 1 admin
- Không có bản ghi trùng
```

## 8. Password Policy

Identity được cấu hình:

- Độ dài tối thiểu: 8 ký tự.
- Bắt buộc chữ hoa.
- Bắt buộc chữ thường.
- Bắt buộc chữ số.
- Bắt buộc ký tự đặc biệt.
- Email phải duy nhất.
- Lockout sau tối đa 5 lần đăng nhập sai.

Mật khẩu production không được lưu trong:

- Source code
- `appsettings.json`
- Migration
- README
- Git history

## 9. API Foundation

Sprint 0 phải có:

- Swagger/OpenAPI.
- Authentication middleware.
- Authorization middleware.
- CORS configuration.
- Request logging middleware.
- Global exception handler.
- Problem Details.
- `traceId` trong error response.

API information endpoint:

```http
GET /api/v1
```

Kết quả mong đợi:

```json
{
  "name": "GDSC Sharing Platform API",
  "version": "v1",
  "status": "running"
}
```

## 10. Health Check

### 10.1. Liveness

```http
GET /health/live
```

Mục đích:

- Kiểm tra API process đang chạy.
- Không phụ thuộc PostgreSQL.

Kết quả:

```text
200 OK
```

### 10.2. Readiness

```http
GET /health/ready
```

Mục đích:

- Kiểm tra API sẵn sàng phục vụ.
- Kiểm tra kết nối PostgreSQL.

Kết quả khi database hoạt động:

```text
200 OK
```

Kết quả khi database không hoạt động:

```text
503 Service Unavailable
```

### 10.3. Aggregate Health Check

```http
GET /health
```

Mục đích:

- Trả về trạng thái tổng hợp.
- Bao gồm API và PostgreSQL.

## 11. Docker

Docker Compose gồm:

```text
postgres
api
web
```

### PostgreSQL container

Yêu cầu:

- Image `postgres:17-alpine`.
- Có health check bằng `pg_isready`.
- Sử dụng named volume.
- Database name lấy từ environment.
- Username lấy từ environment.
- Password bắt buộc lấy từ environment.

### API container

Yêu cầu:

- Build bằng multi-stage Dockerfile.
- Runtime dùng ASP.NET image.
- Lắng nghe tại port `8080` trong container.
- Public port hiện tại là `5080`.
- Kết nối database qua hostname `postgres`.
- Chỉ khởi động sau khi PostgreSQL healthy.

### Web container

Yêu cầu:

- Build Next.js frontend.
- Public port `3000`.
- API URL lấy từ environment.
- Phụ thuộc API service.

## 12. Unit Test

Unit Test phải kiểm tra:

### Department test

- Constructor trim tên.
- Constructor từ chối chuỗi rỗng.
- Constructor từ chối chuỗi chỉ chứa khoảng trắng.

### System Department test

- Có đúng năm Department.
- Không có tên rỗng.
- Không có tên trùng.
- So sánh trùng tên không phân biệt hoa thường.

### System Role test

- Có đúng hai role.
- Có role `Admin`.
- Có role `Member`.
- Không có role trùng.

Unit Test không được phụ thuộc:

- PostgreSQL
- Docker
- Network
- File system bên ngoài

## 13. Integration Test

Integration Test sử dụng:

- xUnit
- `WebApplicationFactory`
- Testcontainers
- PostgreSQL 17 thật

Không sử dụng EF InMemory để kiểm tra migration.

### Health Check Test

Kiểm tra:

```text
GET /health/live  → 200 OK
GET /health/ready → 200 OK khi PostgreSQL hoạt động
```

### Migration Test

Quy trình:

1. Tạo PostgreSQL database rỗng.
2. Kiểm tra `InitialIdentity` đang pending.
3. Chạy `MigrateAsync()`.
4. Kiểm tra migration đã được áp dụng.
5. Kiểm tra không còn pending migration.
6. Kiểm tra database kết nối được.

### Database Seeder Test

Quy trình:

1. Reset database test.
2. Chạy seeder lần thứ nhất.
3. Chạy seeder lần thứ hai.
4. Kiểm tra role.
5. Kiểm tra Department.
6. Kiểm tra Admin.
7. Kiểm tra quan hệ User–Role.
8. Kiểm tra không có dữ liệu trùng.

Test database phải:

- Độc lập với development database.
- Dùng credential dành riêng cho test.
- Tự tạo trước test.
- Tự xóa sau test.

## 14. Yêu cầu bảo mật

- Không commit `deploy/.env`.
- Không commit production password.
- Không commit production connection string.
- Không ghi password vào log.
- Không ghi connection string đầy đủ vào log.
- Admin password lấy từ environment hoặc secret manager.
- Credential trong Integration Test chỉ dùng cho container tạm.
- Secret phải được quản lý riêng theo từng môi trường.

## 15. Yêu cầu phi chức năng

| Nhóm                 | Yêu cầu                              |
| -------------------- | ------------------------------------ |
| Maintainability      | Solution phân lớp rõ ràng            |
| Reliability          | Migration và seeder chạy lặp an toàn |
| Observability        | Có logging, trace ID và health check |
| Portability          | Chạy được bằng Docker Compose        |
| Testability          | Có Unit Test và Integration Test     |
| Security             | Không hard-code production secret    |
| Data integrity       | Có foreign key và unique index       |
| Developer experience | Có README và lệnh setup đầy đủ       |

## 16. Ngoài phạm vi Sprint 0

Những nội dung sau chưa thực hiện:

- Login API
- JWT access token
- Refresh token
- Logout
- Dashboard
- AI Assistant
- Roadmap
- Sharing Content
- File storage
- Sharing Schedule
- Like
- Comment
- Social Interaction
- Notification
- Search
- Moderation
- Analytics
- Production deployment
- Production monitoring

Không thêm các bảng trên vào migration `InitialIdentity`.

## 17. Tiêu chí nghiệm thu

| ID       | Scenario                        | Kết quả mong đợi                |
| -------- | ------------------------------- | ------------------------------- |
| S0-AC-01 | Build solution                  | Không có compile error          |
| S0-AC-02 | Khởi động Docker Compose        | PostgreSQL, API và Web chạy     |
| S0-AC-03 | Gọi `/health/live`              | HTTP 200                        |
| S0-AC-04 | Gọi `/health/ready`             | HTTP 200 khi PostgreSQL healthy |
| S0-AC-05 | Áp migration trên database rỗng | Tạo đúng bảng Sprint 0          |
| S0-AC-06 | Kiểm tra pending migration      | Danh sách rỗng                  |
| S0-AC-07 | Chạy seeder lần đầu             | Tạo Admin, role và Department   |
| S0-AC-08 | Chạy seeder lần hai             | Không tạo dữ liệu trùng         |
| S0-AC-09 | Thiếu Admin password            | Báo lỗi cấu hình                |
| S0-AC-10 | Chạy Unit Test                  | Tất cả pass                     |
| S0-AC-11 | Chạy Integration Test           | Tất cả pass                     |
| S0-AC-12 | Kiểm tra Git                    | Không chứa secret               |

## 18. Definition of Done

### Đã hoàn thành trong source code

- [x] Có solution phân lớp.
- [x] Có `ApplicationUser`.
- [x] Có `Department`.
- [x] Có `UserStatus`.
- [x] Có `ApplicationDbContext`.
- [x] Có Identity configuration.
- [x] Có Department configuration.
- [x] Có migration `InitialIdentity`.
- [x] Migration chỉ chứa bảng Sprint 0.
- [x] Có role seeder.
- [x] Có Department seeder.
- [x] Có Admin seeder.
- [x] Không hard-code production Admin password.
- [x] Có Swagger.
- [x] Có Problem Details.
- [x] Có global exception handler.
- [x] Có request logging.
- [x] Có Health Check.
- [x] Có Dockerfile.
- [x] Có Docker Compose.
- [x] Có Unit Test.
- [x] Có Integration Test.
- [x] Có README.
- [x] Có đặc tả Sprint 0.

### Cần xác nhận trên production

- [ ] `dotnet build GdscSharing.slnx` thành công.
- [ ] `dotnet test GdscSharing.slnx` pass.
- [ ] `npm --prefix web run build` thành công.
- [ ] PostgreSQL container healthy.
- [ ] API container healthy.
- [ ] Web container hoạt động.
- [ ] `/health/live` trả về HTTP 200.
- [ ] `/health/ready` trả về HTTP 200.
- [ ] Database có migration `InitialIdentity`.
- [ ] Database có đúng seed data.
- [ ] Repository không chứa secret.

## 19. Lệnh nghiệm thu Sprint 0

```bash
dotnet restore GdscSharing.slnx
```

```bash
dotnet build \
  GdscSharing.slnx \
  --no-restore
```

```bash
dotnet test \
  GdscSharing.slnx \
  --no-build
```

```bash
npm --prefix web ci
npm --prefix web run build
```

```bash
docker compose \
  --env-file deploy/.env \
  -f deploy/docker-compose.yml \
  up -d --build
```

```bash
docker compose \
  --env-file deploy/.env \
  -f deploy/docker-compose.yml \
  ps
```

```bash
curl --fail http://localhost:5080/health/live
curl --fail http://localhost:5080/health/ready
```

Kiểm tra seed data:

```sql
SELECT *
FROM "__EFMigrationsHistory";

SELECT "Name"
FROM gdsc."Roles"
ORDER BY "Name";

SELECT "Name"
FROM gdsc."Departments"
ORDER BY "Name";

SELECT
    "Email",
    "DisplayName",
    "EmailConfirmed"
FROM gdsc."Users";
```

## 20. Bàn giao sang Sprint 1

Sau khi Definition of Done được xác nhận, Sprint 1 bắt đầu với Authentication và Authorization:

1. Login bằng email/password.
2. JWT access token.
3. Refresh token.
4. Refresh token rotation.
5. Revoke refresh token.
6. Logout.
7. Endpoint `/auth/me`.
8. Authorization theo role `Admin`.
9. Authorization theo role `Member`.
10. Unit Test cho authentication.
11. Integration Test cho authentication flow.

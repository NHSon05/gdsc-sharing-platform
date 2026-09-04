# Báo cáo Tổng kết Tính năng Backend: Member Profile & Membership Management (Sprint 2)

Tài liệu này tổng hợp toàn diện các tính năng đã thay đổi, sơ đồ luồng dữ liệu (workflow) và giải thích chi tiết các khối mã nguồn nghiệp vụ cốt lõi theo đúng chuẩn kỹ thuật trong [02_PROFILE_SPEC.md](./02_PROFILE_SPEC.md), [02_API_CONTRACT.md](./02_API_CONTRACT.md), và [02_STRUCTURE.md](./02_STRUCTURE.md).

---

## 1. Bảng thống kê các thành phần đã triển khai

| Tầng kiến trúc | Thành phần / File | Loại thay đổi | Mô tả chức năng |
| :--- | :--- | :---: | :--- |
| **Domain** | `BaseEntity.cs` | **NEW** | Lớp thực thể cơ sở cung cấp `Id`, `CreatedAtUtc`, `UpdatedAtUtc`. |
| | `ClubGeneration.cs` | **NEW** | Quản lý nhiệm kỳ CLB (`Number`, `Name`, `StartDate`, `EndDate`, `IsActive`). |
| | `ClubRole.cs` | **NEW** | Quản lý chức danh CLB (`Code`, `Name`, `Level`, `IsActive`). |
| | `SystemClubRoles.cs` | **NEW** | Hằng số 4 chức danh chuẩn: `LEAD`, `SUBLEAD`, `CORETEAM`, `ALUMNI`. |
| | `ClubMembership.cs` | **NEW** | Quan hệ Member tham gia vào một Generation cụ thể. |
| | `DepartmentMembership.cs` | **NEW** | Quan hệ Member tham gia vào Ban trong Generation (`IsPrimary`, `JoinedAt`, `LeftAt`). |
| | `RoleAssignment.cs` | **NEW** | Gán chức danh trong Ban (`AssignedAtUtc`, `AssignedByUserId`, `EndedAtUtc`, `IsActive`). |
| | `Department.cs` | **REFACTOR** | Mở rộng danh mục phòng ban động (`Slug`, `Color`, `Icon`, `SortOrder`), chuyển vào namespace `GdscSharingPlatform.Domain.Departments`. |
| | `SystemDepartments.cs` | **NEW** | Danh mục hằng số 5 phòng ban chuẩn (`Software`, `AI`, `Marketing`, `Media`, `Community`). |
| | `UserStatus.cs` | **KEPT** | Enum trạng thái tài khoản: `Active`, `Suspended`, `Inactive`. |
| **Application** | `ProfileDto.cs` | **NEW** | DTO trả về hồ sơ hoàn chỉnh, cây phân cấp Generation $\rightarrow$ Department $\rightarrow$ Roles, % hoàn thiện và danh sách trường còn thiếu. |
| | `UpdateProfileRequest.cs` | **NEW** | DTO cập nhật `DisplayName`, `PhoneNumber`, `StudentCode`, `GitHubUrl`, `Bio`. |
| | `ProfileValidators.cs` | **NEW** | FluentValidation cho Profile, Đổi Email, Xác nhận Email. |
| | `GenerationModels.cs` | **NEW** | DTO CRUD Generation (`GenerationDto`, `CreateGenerationRequest`, `UpdateGenerationRequest`). |
| | `DepartmentModels.cs` | **NEW** | DTO CRUD Department (`DepartmentDetailDto`, `CreateDepartmentRequest`, `UpdateDepartmentRequest`). |
| | `AdminMembershipModels.cs` | **NEW** | DTO gán Gen, gán Department, cập nhật `isPrimary`, `ReplaceRolesRequest`. |
| | `MembershipValidators.cs` | **NEW** | FluentValidation kiểm tra Slug, Hex color `#RGB/#RRGGBB`, ngày bắt đầu/kết thúc Gen. |
| | `IProfileService.cs` | **NEW** | Giao diện nghiệp vụ Profile cá nhân. |
| | `IMembershipServices.cs` | **NEW** | Giao diện nghiệp vụ: `ILookupService`, `IDepartmentService`, `IGenerationService`, `IMemberMembershipService`. |
| | `IFileStorageService.cs` | **NEW** | Hợp đồng lưu trữ file ảnh avatar. |
| | `IEmailSender.cs` | **NEW** | Hợp đồng gửi email xác nhận đổi email. |
| | `PayloadTooLargeException.cs` | **NEW** | Ngoại lệ khi file vượt quá 5MB (ánh xạ HTTP 413). |
| | `UnsupportedMediaTypeException.cs` | **NEW** | Ngoại lệ khi định dạng/magic bytes file không hợp lệ (ánh xạ HTTP 415). |
| **Infrastructure** | `ProfileService.cs` | **NEW** | Cài đặt logic Profile, thuật toán tính tỷ lệ hoàn thiện hồ sơ, unique MSSV, email change flow. |
| | `DepartmentService.cs` | **NEW** | CRUD Department, kích hoạt / hủy kích hoạt soft delete, kiểm tra trùng lặp Name/Slug. |
| | `GenerationService.cs` | **NEW** | Quản lý Gen, kiểm tra trùng số hiệu Gen, kích hoạt / hủy kích hoạt. |
| | `MemberMembershipService.cs` | **NEW** | Quản lý gán Gen, gán Ban, **Role Replacement Transaction**, kết thúc mềm bảo lưu lịch sử. |
| | `LookupService.cs` | **NEW** | Dịch vụ tra cứu nhanh Gen, Ban, Role hỗ trợ cờ `includeInactive`. |
| | `LocalFileStorageService.cs` | **NEW** | Kiểm tra kích thước $\le$ 5MB, magic bytes signature (JPEG, PNG, WebP), lưu trữ `wwwroot/uploads/avatars/`. |
| | `LoggingEmailSender.cs` | **NEW** | Giả lập gửi email xác nhận đổi email qua log có cấu trúc. |
| | `Configurations/` | **REFACTOR** | Tổ chức lại cấu hình EF Core vào các thư mục con: `Departments/`, `Memberships/`, `Identity/`. |
| | `LegacyProfileBackfillService.cs` | **NEW** | Migration và backfill dữ liệu cũ sang cấu trúc đa nhiệm kỳ, đa ban, đa chức danh. |
| | `DatabaseSeeder.cs` | **UPDATE** | Seed idempotent Club Roles và Departments động. |
| **API** | `ProfileController.cs` | **NEW** | 5 endpoints: `GET /me`, `PATCH /me` (hỗ trợ cập nhật thông tin hồ sơ từng phần & email), `PATCH /me/email` (đổi email trực tiếp không cần confirm), `POST /me/avatar`, `DELETE /me/avatar` (hỗ trợ backward-compatible cả PUT). |
| | `GenerationsController.cs` | **NEW** | Lookup Generation (`GET /api/generations`). |
| | `DepartmentsController.cs` | **NEW** | Lookup Department (`GET /api/departments`). |
| | `ClubRolesController.cs` | **NEW** | Lookup Club Role (`GET /api/club-roles`). |
| | `AdminDepartmentsController.cs` | **NEW** | Admin CRUD Department (`POST`, `PUT`, `DELETE`, `POST /activate`). |
| | `AdminGenerationsController.cs` | **NEW** | Admin CRUD Generation (`POST`, `PUT`, `DELETE`). |
| | `AdminMemberMembershipsController.cs` | **NEW** | Admin Member Membership & Role Assignment management. |
| | `GlobalExceptionHandler.cs` | **UPDATE** | Bổ sung ánh xạ FluentValidation (400), `PayloadTooLargeException` (413), `UnsupportedMediaTypeException` (415). |
| | `Program.cs` | **UPDATE** | Thêm `app.UseStaticFiles()` phục vụ ảnh avatar. |

---

## 2. Sơ đồ luồng hoạt động (Workflows)

### 2.1. Cấu trúc quan hệ thực thể (Domain Entity Hierarchy)

```mermaid
erDiagram
    ApplicationUser ||--o{ ClubMembership : "participates in"
    ClubGeneration ||--o{ ClubMembership : "has members"
    ClubMembership ||--o{ DepartmentMembership : "belongs to depts"
    Department ||--o{ DepartmentMembership : "has department members"
    DepartmentMembership ||--o{ RoleAssignment : "has assigned roles"
    ClubRole ||--o{ RoleAssignment : "assigned via"
    ApplicationUser ||--o{ RoleAssignment : "assigned by admin"

    ClubMembership {
        Guid Id PK
        Guid UserId FK
        Guid GenerationId FK
        DateOnly JoinedAt
        DateOnly LeftAt
        bool IsActive
    }

    DepartmentMembership {
        Guid Id PK
        Guid ClubMembershipId FK
        Guid DepartmentId FK
        bool IsPrimary
        DateOnly JoinedAt
        DateOnly LeftAt
        bool IsActive
    }

    RoleAssignment {
        Guid Id PK
        Guid DepartmentMembershipId FK
        Guid ClubRoleId FK
        Guid AssignedByUserId FK
        DateTimeOffset AssignedAtUtc
        DateTimeOffset EndedAtUtc
        bool IsActive
    }
```

---

### 2.2. Luồng lấy thông tin Profile và tính toán tỷ lệ hoàn thiện (`GET /api/profile/me`)

```mermaid
sequenceDiagram
    autonumber
    actor Member
    participant API as ProfileController
    participant Svc as ProfileService
    participant DB as ApplicationDbContext (PostgreSQL)

    Member->>API: GET /api/profile/me (Bearer Token)
    API->>Svc: GetMyProfileAsync(userId)
    Svc->>DB: Query User with ClubMemberships -> DeptMemberships -> RoleAssignments
    DB-->>Svc: Full User Entity Graph
    Note over Svc: Tính toán 9 tiêu chí hoàn thiện hồ sơ:<br/>1. DisplayName<br/>2. PhoneNumber<br/>3. StudentCode<br/>4. GitHubUrl<br/>5. Bio<br/>6. AvatarUrl<br/>7. Có >= 1 ClubMembership<br/>8. Có >= 1 DeptMembership<br/>9. Có >= 1 Active RoleAssignment
    Svc->>Svc: ProfileCompletionPercentage = (Count * 100) / 9
    Svc->>Svc: MissingProfileFields = list các tiêu chí chưa đạt
    Svc-->>API: ProfileDto
    API-->>Member: 200 OK (Full profile JSON)
```

---

### 2.3. Luồng Upload và Kiểm duyệt Avatar (`POST /api/profile/me/avatar`)

```mermaid
flowchart TD
    A["Member: POST /api/profile/me/avatar (Multipart/form-data)"] --> B{"File null hoặc rỗng?"}
    B -- Có --> C["Ném 400 Bad Request (avatar required)"]
    B -- Không --> D{"File stream > 5 MB?"}
    D -- Có --> E["Ném 413 Payload Too Large"]
    D -- Không --> F{"MIME hợp lệ (JPEG, PNG, WebP)?"}
    F -- Không --> G["Ném 415 Unsupported Media Type"]
    F -- Có --> H["Đọc 12 bytes đầu tiên (Magic Bytes Inspection)"]
    H --> I{"Signature hợp lệ?"}
    I -- Không --> J["Ném 415 Unsupported Media Type (Invalid file signature)"]
    I -- Có --> K{"User đã có Avatar cũ?"}
    K -- Có --> L["Xóa file avatar cũ khỏi ổ đĩa"]
    K -- Không --> M["Bỏ qua"]
    L --> N["Lưu file mới: /wwwroot/uploads/avatars/{userId}/{guid}.ext"]
    M --> N
    N --> O["Cập nhật user.AvatarUrl và user.UpdatedAt trong CSDL"]
    O --> P["Trả về 200 OK: { avatarUrl: '/uploads/avatars/...' }"]
```

---

### 2.4. Luồng Đổi Email trực tiếp (`PATCH /api/profile/me/email` hoặc `PATCH /api/profile/me`)

Thành viên có thể tự do cập nhật Email trực tiếp trong hồ sơ mà không cần kiểm tra mật khẩu hay gửi token xác nhận (hệ thống chỉ kiểm tra trùng lặp email với người dùng khác):

```mermaid
sequenceDiagram
    autonumber
    actor Member
    participant API as ProfileController
    participant Svc as ProfileService
    participant UM as UserManager
    participant DB as ApplicationDbContext

    Member->>API: PATCH /api/profile/me/email { newEmail } (hoặc qua PATCH /api/profile/me)
    API->>Svc: ChangeEmailAsync(userId, request)
    Svc->>UM: FindByEmailAsync(newEmail)
    alt Email đã có người khác dùng
        Svc-->>API: Ném ConflictException
        API-->>Member: 409 Conflict
    end
    Note over Svc,DB: Cập nhật trực tiếp:<br/>1. user.Email = newEmail<br/>2. user.UserName = newEmail<br/>3. user.EmailConfirmed = true<br/>4. user.UpdatedAt = UtcNow
    Svc->>DB: SaveChangesAsync()
    Svc-->>API: Trả về ProfileDto mới nhất
    API-->>Member: 200 OK (Profile updated with new Email)
```

---

### 2.5. Transaction Thay thế Chức danh (`PUT /api/admin/.../roles`)

```mermaid
flowchart TD
    A["Admin: PUT /api/admin/members/{userId}/department-memberships/{deptMemId}/roles"] --> B["Validate danh sách RoleIds gửi lên"]
    B --> C{"Tất cả RoleIds có tồn tại và IsActive = true?"}
    C -- Không --> D["404 Not Found hoặc 400 Bad Request"]
    C -- Có --> E["Bắt đầu Transaction CSDL"]
    E --> F["Load các RoleAssignment đang Active của DeptMembership này"]
    F --> G["Bước 1: Kết thúc các Role không còn trong danh sách gửi lên (EndedAtUtc = UtcNow, IsActive = false)"]
    G --> H["Bước 2: Giữ nguyên các Role đang active và vẫn có trong danh sách"]
    H --> I["Bước 3: Tạo mới RoleAssignment cho các Role mới được bổ sung"]
    I --> J["SaveChangesAsync()"]
    J --> K{"Lỗi xảy ra?"}
    K -- Có --> L["Rollback Transaction"]
    K -- Không --> M["Commit Transaction"]
    M --> N["Trả về 200 OK: Danh sách Role active hiện tại"]
```

---

## 3. Giải thích chi tiết mã nguồn cốt lõi (Code Highlights)

### 3.1. Thuật toán tính tỷ lệ hoàn thiện hồ sơ (`ProfileService.cs`)
Theo mục 13 của `02_PROFILE_SPEC.md`, hệ thống chia tổng cộng **9 tiêu chí** (6 trường cá nhân + 3 trường tổ chức):
```csharp
var missingFields = new List<string>();

// 6 tiêu chí thông tin cá nhân
if (string.IsNullOrWhiteSpace(user.DisplayName)) missingFields.Add("displayName");
if (string.IsNullOrWhiteSpace(user.PhoneNumber)) missingFields.Add("phoneNumber");
if (string.IsNullOrWhiteSpace(user.StudentCode)) missingFields.Add("studentCode");
if (string.IsNullOrWhiteSpace(user.GitHubUrl)) missingFields.Add("githubUrl");
if (string.IsNullOrWhiteSpace(user.Bio)) missingFields.Add("bio");
if (string.IsNullOrWhiteSpace(user.AvatarUrl)) missingFields.Add("avatarUrl");

// 3 tiêu chí tổ chức & nhiệm kỳ
var hasClubMembership = user.ClubMemberships != null && user.ClubMemberships.Count > 0;
if (!hasClubMembership) missingFields.Add("clubMemberships");

var allDeptMemberships = user.ClubMemberships?.SelectMany(cm => cm.DepartmentMemberships).ToList() ?? new();
var hasDeptMembership = allDeptMemberships.Count > 0;
if (!hasDeptMembership) missingFields.Add("departmentMemberships");

var hasActiveRole = allDeptMemberships.SelectMany(dm => dm.RoleAssignments).Any(ra => ra.IsActive);
if (!hasActiveRole) missingFields.Add("roleAssignments");

// Tính phần trăm động không lưu cứng vào DB
var completedCount = 9 - missingFields.Count;
var completionPercentage = (completedCount * 100) / 9;
```

---

### 3.2. Kiểm tra Magic Bytes và an toàn lưu trữ Avatar (`LocalFileStorageService.cs`)
Thay vì chỉ tin tưởng đuôi file hoặc MIME header (có thể bị giả mạo), service đọc trực tiếp byte đầu của file:
```csharp
private static bool IsValidImageSignature(byte[] header, int length, out string extension)
{
    extension = string.Empty;
    if (length < 4) return false;

    // JPEG: FF D8 FF
    if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
    {
        extension = ".jpg";
        return true;
    }

    // PNG: 89 50 4E 47
    if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
    {
        extension = ".png";
        return true;
    }

    // WebP: RIFF (4 bytes) + File Size (4 bytes) + WEBP (4 bytes)
    if (length >= 12 &&
        header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
        header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
    {
        extension = ".webp";
        return true;
    }

    return false;
}
```

---

### 3.3. Transaction Thay thế Chức danh an toàn (`MemberMembershipService.cs`)
Đảm bảo tính bất biến của lịch sử: không bao giờ xóa cứng dòng role cũ, mà đánh dấu `EndedAtUtc` và kết thúc mềm:
```csharp
var activeAssignments = await _dbContext.RoleAssignments
    .Where(ra => ra.DepartmentMembershipId == departmentMembershipId && ra.IsActive)
    .ToListAsync(cancellationToken);

// 1. Kết thúc các role không còn nằm trong target list
foreach (var assignment in activeAssignments)
{
    if (!targetRoleIds.Contains(assignment.ClubRoleId))
    {
        assignment.End(); // Đặt IsActive = false, EndedAtUtc = UtcNow
    }
}

// 2. Thêm mới các role chưa có
var activeRoleIds = activeAssignments.Where(ra => ra.IsActive).Select(ra => ra.ClubRoleId).ToHashSet();
foreach (var roleId in targetRoleIds)
{
    if (!activeRoleIds.Contains(roleId))
    {
        var newAssignment = new RoleAssignment(departmentMembershipId, roleId, currentUserId);
        _dbContext.RoleAssignments.Add(newAssignment);
    }
}

await _dbContext.SaveChangesAsync(cancellationToken);
```

---

### 3.4. Ánh xạ lỗi chuẩn RFC 7807 (`GlobalExceptionHandler.cs`)
Toàn bộ mã lỗi từ `02_API_CONTRACT.md` được tự động chuyển thành JSON ProblemDetails:
* `400 Bad Request`: Lỗi validation dữ liệu (`FluentValidation` hoặc `ApplicationValidationException`).
* `401 Unauthorized`: Chưa đăng nhập hoặc mật khẩu sai (`AuthenticationException`).
* `403 Forbidden`: Người dùng không có quyền Admin (`ForbiddenAccessException`).
* `404 Not Found`: Không tìm thấy thực thể (`NotFoundException`).
* `409 Conflict`: Trùng lặp `StudentCode`, trùng `Slug` ban, trùng số `Number` Gen, hoặc thành viên đã ở trong Gen/Ban (`ConflictException`).
* `413 Payload Too Large`: Upload file vượt quá 5MB (`PayloadTooLargeException`).
* `415 Unsupported Media Type`: File không đúng định dạng ảnh hoặc magic bytes sai (`UnsupportedMediaTypeException`).

---

## 4. Tổng kết kiểm thử tự động (Test Suite)

Hệ thống đã bổ sung đầy đủ Unit Tests và Integration Tests cho toàn bộ tính năng mới:

```text
======================= TEST RUN SUMMARY =======================
Project: GdscSharingPlatform.UnitTests.dll (net10.0)
Passed:  166 / 166 tests (100%) - Thời gian: ~2s
- Domain entity tests (Department, Memberships, ApplicationUser)
- FluentValidation tests (Profile, Memberships, Generations, Departments)
- LocalFileStorageService tests (JPEG, PNG, WebP, >5MB, invalid magic bytes, file deletion)
- Service tests (ProfileService, DepartmentService, GenerationService, MemberMembershipService)

Project: GdscSharingPlatform.IntegrationTests.dll (net10.0)
Passed:  28 / 28 tests (100%) - Thời gian: ~6s
- Auth endpoints tests (Login, Refresh, Logout)
- Profile endpoints tests (GET /me, PUT /me, Avatar upload/delete, direct Email change)
- Lookup endpoints tests (Generations, Departments, ClubRoles with Member vs Admin permissions)
- Admin Department endpoints tests (Create, Update, Deactivate, Activate, 403 Forbidden check)
- Admin Generation endpoints tests (Create, Deactivate)

TỔNG CỘNG: 194 / 194 TESTS PASS (0 FAILED, 0 SKIPPED)
================================================================
```

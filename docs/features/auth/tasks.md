# Tasks Breakdown: Authentication and Authorization (Sprint 1)

**Feature**: `001-authentication-authorization`  
**Feature Spec**: [spec.md](file:///Users/nguyenhongson/Documents/GDG/gdsc-sharing-platform/docs/features/auth/spec.md)  
**Implementation Plan**: [plan.md](file:///Users/nguyenhongson/Documents/GDG/gdsc-sharing-platform/docs/features/auth/plan.md)  
**Target Milestone**: Sprint 1 — Baseline Authentication & Authorization  
**Status**: Ready for Implementation  

---

## 📊 Tiến độ tổng quan (Progress Summary)

- **Tổng số Tasks**: 24 tasks
- **Đã hoàn thành**: 6 / 24 (25%)
- **Trạng thái**: 🟢 Phase 1 hoàn thành (Ready for Review)

```text
[🟩🟩🟩⬜⬜⬜⬜⬜⬜⬜] 25%
```

---

## 📋 Danh sách công việc chi tiết (Tasks Breakdown)

### Phase 1: Domain & Database Foundation (Entities, Migration, Seeding)
> Mục tiêu: Xây dựng entity `RefreshToken`, cấu hình quan hệ với `ApplicationUser` và tạo database migration an toàn không làm ảnh hưởng dữ liệu Sprint 0.

- [x] **TASK-101**: Bổ sung Policy names & Claim constants vào `GdscSharingPlatform.Application/Common/Security/RoleNames.cs` (hoặc tạo `AuthConstants.cs`).
  - *Output*: Khai báo các hằng số Policy (`AdminOnly`, `MemberOnly`, `RequireActiveUser`) và Custom Claims (`department_id`, `status`).
  - *FR Mapping*: FR-030, FR-034.
- [x] **TASK-102**: Tạo entity `RefreshToken` trong `GdscSharingPlatform.Infrastructure/Identity/RefreshToken.cs`.
  - *Fields*: `Id`, `UserId`, `TokenHash`, `CreatedAt`, `ExpiresAt`, `IsRevoked`, `RevokedAt`, `ReplacedByTokenHash`, `RevocationReason`, `CreatedByIp`, `UserAgent`.
  - *FR Mapping*: FR-009, FR-010, FR-011, SR-003.
- [x] **TASK-103**: Thêm navigation property `RefreshTokens` vào `ApplicationUser.cs` (`public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();`).
  - *FR Mapping*: FR-010, FR-012.
- [x] **TASK-104**: Cấu hình Fluent API `RefreshTokenConfiguration` trong `GdscSharingPlatform.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs`.
  - *Rules*: Table `RefreshTokens`, PK `Id`, Unique Index trên `TokenHash`, Index trên `(UserId, IsRevoked, ExpiresAt)`, FK với `ApplicationUser` (Cascade delete).
  - *FR Mapping*: FR-009, FR-012.
- [x] **TASK-105**: Đăng ký `DbSet<RefreshToken> RefreshTokens` vào `ApplicationDbContext.cs`.
  - *FR Mapping*: FR-011.
- [x] **TASK-106**: Tạo EF Core Migration `AddRefreshTokenTable` và chạy migration trên database test/dev.
  - *Validation*: Đảm bảo không làm mất dữ liệu tài khoản Admin và Department đã seed ở Sprint 0.
  - *FR Mapping*: FR-043, FR-044, SC-017, SC-019.

---

### Phase 2: Security Infrastructure & Token Services
> Mục tiêu: Xây dựng các dịch vụ sinh JWT Access Token chuẩn, sinh/băm Refresh Token an toàn và quản lý thông tin User hiện tại.

- [ ] **TASK-201**: Tạo model `JwtOptions.cs` trong `GdscSharingPlatform.Infrastructure/Identity/Options/JwtOptions.cs` và cấu hình `ValidateOnStart()` trong `DependencyInjection.cs`.
  - *Options*: `Issuer`, `Audience`, `SecretKey` (min 32 ký tự), `AccessTokenExpirationMinutes` (15m), `RefreshTokenExpirationDays` (7d), `ClockSkewSeconds` (0s).
  - *FR Mapping*: FR-042, SR-001, SR-004.
- [ ] **TASK-202**: Khai báo interface `IJwtTokenGenerator` trong `GdscSharingPlatform.Application/Common/Interfaces/IJwtTokenGenerator.cs`.
  - *Methods*:
    - `GenerateAccessToken(Guid userId, string email, string fullName, IEnumerable<string> roles, Guid? departmentId, string status) -> (string token, int expiresInSeconds)`
    - `GenerateRefreshToken() -> string` (Sinh chuỗi ngẫu nhiên cryptographically secure 64 bytes)
    - `HashToken(string rawToken) -> string` (Tính băm SHA-256)
  - *FR Mapping*: FR-007, FR-008, FR-009, SR-002, SR-003, SR-005.
- [ ] **TASK-203**: Triển khai `JwtTokenGenerator` trong `GdscSharingPlatform.Infrastructure/Identity/Services/JwtTokenGenerator.cs`.
  - *Claims*: `sub`, `email`, `name`, `role` (multiple), `department_id`, `status`, `jti`, `iss`, `aud`, `exp`, `nbf` (tính theo UTC).
  - *FR Mapping*: FR-027, FR-049, SR-005, SR-006, SR-007.
- [ ] **TASK-204**: Khai báo interface `ICurrentUserService` trong `Application/Common/Interfaces/ICurrentUserService.cs` và triển khai `CurrentUserService` trong `Infrastructure`.
  - *Properties*: `UserId`, `Email`, `Roles`, `IsAuthenticated`.
  - *FR Mapping*: FR-026, FR-029.

---

### Phase 3: Application DTOs, Validation & AuthService Use Cases
> Mục tiêu: Triển khai toàn bộ logic nghiệp vụ xác thực, token rotation, reuse detection, logout và truy xuất profile.

- [ ] **TASK-301**: Định nghĩa các DTOs trong `GdscSharingPlatform.Application/Features/Auth/Models/`:
  - `LoginRequest.cs` (Email, Password)
  - `AuthResponse.cs` (AccessToken, RefreshToken, TokenType, ExpiresIn, User)
  - `RefreshTokenRequest.cs` (RefreshToken)
  - `TokenResponse.cs` (AccessToken, RefreshToken, TokenType, ExpiresIn)
  - `LogoutRequest.cs` (RefreshToken)
  - `CurrentUserDto.cs` & `DepartmentDto.cs`
  - *FR Mapping*: FR-027, FR-028, FR-038.
- [ ] **TASK-302**: Viết FluentValidation Validators:
  - `LoginRequestValidator.cs`: Email required & valid email format & max 256 ký tự; Password required & max 128 ký tự (không trim password).
  - `RefreshTokenRequestValidator.cs`: RefreshToken required & not empty.
  - `LogoutRequestValidator.cs`: RefreshToken required.
  - *FR Mapping*: FR-002, FR-003, FR-038, FR-046.
- [ ] **TASK-303**: Khai báo interface `IAuthService` trong `GdscSharingPlatform.Application/Features/Auth/Interfaces/IAuthService.cs`.
  - *Methods*:
    - `Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent, CancellationToken ct)`
    - `Task<TokenResponse> RefreshTokenAsync(string refreshToken, string? ipAddress, string? userAgent, CancellationToken ct)`
    - `Task LogoutAsync(string refreshToken, Guid? currentUserId, CancellationToken ct)`
    - `Task LogoutAllAsync(Guid userId, CancellationToken ct)`
    - `Task<CurrentUserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct)`
- [ ] **TASK-304**: Triển khai `AuthService.LoginAsync` trong `GdscSharingPlatform.Infrastructure/Identity/Services/AuthService.cs`:
  - Chuẩn hóa email (trim + lowercase).
  - Kiểm tra `UserStatus == Active` và `IsDeleted == false`.
  - Kiểm tra mật khẩu qua `UserManager.CheckPasswordSignInAsync` (hỗ trợ đếm sai mật khẩu để lockout 5 lần / 15 phút).
  - Thông báo lỗi chung ("Invalid email or password.") khi sai email hoặc mật khẩu (Chống User Enumeration).
  - Sinh cặp token, lưu `TokenHash` mới vào DB và cập nhật `LastLoginAt`.
  - *FR Mapping*: FR-001 -> FR-006, FR-040, SR-008, SC-001, SC-002, SC-003.
- [ ] **TASK-305**: Triển khai `AuthService.RefreshTokenAsync` (Rotation & Reuse Detection):
  - Băm SHA-256 token gửi lên và tìm trong DB.
  - **Nếu token đã bị thu hồi (`IsRevoked == true`)**: Phát hiện tái sử dụng -> Thu hồi toàn bộ session của User đó, ghi log bảo mật và trả về 401 Unauthorized.
  - **Nếu token hết hạn hoặc User bị khóa/vô hiệu hóa**: Trả về 401 Unauthorized.
  - **Nếu hợp lệ**: Thu hồi token cũ (`ReplacedByTokenHash = newHash`), tạo token mới, lưu DB và trả về cặp token mới.
  - *FR Mapping*: FR-013 -> FR-019, FR-047, FR-048, SR-011, SR-012, SC-007, SC-008, SC-009.
- [ ] **TASK-306**: Triển khai `AuthService.LogoutAsync` & `LogoutAllAsync`:
  - `LogoutAsync`: Thu hồi duy nhất token được cung cấp (`IsRevoked = true`, `RevokedAt = UtcNow`, `RevocationReason = "User logout"`). Idempotent (không lỗi nếu gọi lại).
  - `LogoutAllAsync`: Thu hồi tất cả refresh tokens đang hoạt động (`IsActive == true`) của User.
  - *FR Mapping*: FR-020 -> FR-025, SC-010, SC-011, SC-012.
- [ ] **TASK-307**: Triển khai `AuthService.GetCurrentUserAsync`:
  - Nạp thông tin User kèm `Department` và `Roles`.
  - Trả về `CurrentUserDto` không chứa thông tin bảo mật nhạy cảm.
  - *FR Mapping*: FR-026, FR-027, FR-028, SC-013.

---

### Phase 4: API Layer, Authentication Middleware & OpenAPI
> Mục tiêu: Thiết lập HTTP endpoints, cấu hình Authentication/Authorization pipeline và Swagger UI Bearer auth.

- [ ] **TASK-401**: Cấu hình JWT Bearer Authentication & Authorization Policies trong `Program.cs` / `AddApiAuthentication`.
  - *TokenValidationParameters*: ValidateIssuer, ValidateAudience, ValidateLifetime, ValidateIssuerSigningKey, ClockSkew = TimeSpan.Zero.
  - *Policies*: `AdminOnly` (`Roles = "Admin"`), `MemberOnly` (`Roles = "Member,Admin"`).
  - *FR Mapping*: FR-030, FR-031, FR-032, FR-033, FR-036, FR-037.
- [ ] **TASK-402**: Cấu hình Swagger / OpenAPI hỗ trợ Bearer Token Authentication (Authorize button trong Swagger UI).
  - *Config*: `AddSecurityDefinition("Bearer", ...)` & `AddSecurityRequirement(...)`.
  - *FR Mapping*: SC-020.
- [ ] **TASK-403**: Tạo `AuthController` (`GdscSharingPlatform.Api/Controllers/AuthController.cs`):
  - `POST /api/v1/auth/login` [AllowAnonymous]
  - `POST /api/v1/auth/refresh` [AllowAnonymous]
  - `POST /api/v1/auth/logout` [AllowAnonymous / Authorize]
  - `POST /api/v1/auth/logout-all` [Authorize]
  - `GET /api/v1/auth/me` [Authorize]
  - `GET /api/v1/auth/test/member` [Authorize(Roles = "Member,Admin")] (FR-045)
  - `GET /api/v1/auth/test/admin` [Authorize(Roles = "Admin")] (FR-045)
  - *FR Mapping*: FR-001, FR-013, FR-020, FR-023, FR-026, FR-045.
- [ ] **TASK-404**: Xác thực định dạng lỗi RFC 7807 và `traceId` với `GlobalExceptionHandler`.
  - Đảm bảo 401 Unauthorized và 403 Forbidden trả về format nhất quán có `traceId`.
  - *FR Mapping*: FR-034, FR-039, SR-010, SC-014.

---

### Phase 5: Automated Testing Suite (Unit & Integration Tests)
> Mục tiêu: Viết và thực thi 100% test case kiểm thử tự động, bao phủ toàn bộ Functional & Security Requirements.

- [ ] **TASK-501**: Viết Unit Tests trong `GdscSharingPlatform.UnitTests`:
  - `JwtTokenGeneratorTests`: Kiểm tra tính hợp lệ của access token, claims, expiration, và hàm băm SHA-256.
  - `ValidationTests`: Kiểm tra `LoginRequestValidator`, `RefreshTokenRequestValidator` với các input biên (empty, max length, invalid email, trim password).
  - `AuthServiceTests`: Mock DbContext & UserManager kiểm tra các nhánh logic đăng nhập, token reuse, logout.
  - *FR Mapping*: FR-002, FR-003, FR-009, FR-038, SC-015, SC-016.
- [ ] **TASK-502**: Viết Integration Tests trong `GdscSharingPlatform.IntegrationTests`:
  - `LoginEndpointTests`: Đăng nhập thành công với admin seed; Đăng nhập thất bại (sai email/password); Kiểm tra anti-enumeration; Thử đăng nhập 5 lần sai để kiểm tra lockout.
  - `RefreshTokenEndpointTests`: Luồng refresh thành công; Token hết hạn; **Mô phỏng tấn công Token Reuse** (dùng lại token cũ -> kiểm tra toàn bộ session của user bị thu hồi).
  - `LogoutEndpointsTests`: Logout đơn phiên (idempotent); Logout all phiên; Xác nhận token cũ không refresh được nữa.
  - `CurrentUserEndpointTests`: Gọi `/me` khi chưa đăng nhập (401); Gọi `/me` với token hợp lệ (kiểm tra đầy đủ thông tin department và roles).
  - `RoleAuthorizationEndpointTests`: Gọi `/test/member` và `/test/admin` với tài khoản Admin, Member và Anonymous.
  - *FR Mapping*: SC-001 -> SC-014, SC-016, SC-018.
- [ ] **TASK-503**: Chạy Regression Testing cho toàn bộ test suite Sprint 0:
  - Đảm bảo tất cả các test của Sprint 0 (HealthChecks, Migration, Seeder) tiếp tục PASS 100%.
  - *FR Mapping*: FR-043, FR-044, SC-017, SC-019.

---

### Phase 6: Nghiệm thu & Tài liệu bàn giao (Verification & Sign-off)
> Mục tiêu: Đối chiếu hoàn thành 20 tiêu chí thành công và cập nhật tài liệu kỹ thuật.

- [ ] **TASK-601**: Kiểm tra đối chiếu bảng 20 tiêu chí nghiệm thu (**SC-001 -> SC-020**).
- [ ] **TASK-602**: Kiểm tra OpenAPI Swagger UI hoạt động thông suốt (Authorize -> Gọi thử các endpoint).
- [ ] **TASK-603**: Cập nhật tài liệu nghiệm thu Sprint 1 (`walkthrough.md` hoặc báo cáo nghiệm thu).

---

## 📌 Hướng dẫn thực hiện (Execution Guidelines)

1. **Thứ tự thực hiện**: Thực hiện tuần tự từ **Phase 1 -> Phase 2 -> Phase 3 -> Phase 4 -> Phase 5 -> Phase 6**.
2. **Quy tắc Commit & Code Style**:
   - Tuân thủ Clean Architecture: Domain không phụ thuộc tầng ngoài, Infrastructure hiện thực các interface từ Application.
   - Mọi mốc thời gian phải sử dụng `DateTimeOffset.UtcNow`.
   - Tuyệt đối không log thông tin nhạy cảm (Password, Plain Refresh Token, Secret Key).
3. **Cập nhật tiến độ**: Sau khi hoàn thành mỗi task, đánh dấu `[x]` vào ô checkbox tương ứng.

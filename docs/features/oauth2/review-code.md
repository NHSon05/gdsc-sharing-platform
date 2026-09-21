## Kết quả xử lý — 2026-09-21

- Đã sửa mục 1–6 (P0–P2): khôi phục kiểm tra mật khẩu; chỉ tạo một phiên qua `UserSessionService`; thêm refresh token vào DbContext và await thao tác lưu; chỉ mở/commit transaction khi provider là relational; sửa namespace và tên tham số; xóa helper/comment cũ.
- Sửa thêm `UserSessionService` để dùng `AuthenticationException` của Application, tương thích bộ xử lý lỗi HTTP hiện có.
- Mục 7 (P3) là gợi ý tích hợp Phase 4, chưa đổi response JSON thành redirect. Next.js BFF cần cơ chế bàn giao phiên trước khi redirect được coi là hoàn tất đăng nhập frontend.
- Kiểm chứng: build trong quá trình chạy test thành công; 12 auth integration tests và 367 unit tests pass; `git diff --check` không có lỗi.
- Test hồi quy kiểm tra: password sai/rỗng không tạo phiên; login hợp lệ chỉ lưu một refresh token dưới dạng hash; Google first/returning login không tạo trùng user và refresh được; email chưa verified/trùng tài khoản nội bộ bị từ chối; user deleted bị từ chối với đúng exception của Application.
- Auth tests sử dụng cấu hình Google/JWT giả trong môi trường Testing. Không gọi Google thật. EF InMemory không kiểm chứng rollback/unique constraints của PostgreSQL; các kiểm chứng đó và flow browser OAuth thật chưa được thực hiện trong lần sửa này.

Các nhận xét gốc được giữ bên dưới để đối chiếu.

## P0 - Critical (Lỗi nghiêm trọng / Bảo mật / Dữ liệu)

1. @AuthService.cs:L85-97 - Bypass kiểm tra mật khẩu (Authentication Bypass)
   - Vấn đề: Toàn bộ block if (!passwordResult.Succeeded) ... throw new AuthenticationException(...) bị comment lại. Bất kỳ ai nhập mật khẩu sai hoặc trống đều đăng nhập thành công vào mọi tài khoản!

   - Đồng thời: Trong LoginAsync, code vừa gọi \_sessionService.CreateAsync(...), vừa tiếp tục chạy code cũ CreateTokenPair + PersistUserLoginAsync, dẫn đến tạo token kép và ghi đè session.

2. @UserSessionService.cs - Refresh Token không được lưu vào DB & Unawaited Async Task
   - Vấn đề 1: Khởi tạo var refreshToken = new RefreshToken(...) nhưng không gọi dbContext.RefreshTokens.Add(refreshToken);. Refresh Token trả về cho client sẽ hoàn toàn không tồn tại trong DB, khiến client gọi /api/auth/refresh bị lỗi 401 ngay lập tức.
   - Vấn đề 2: Dòng 68 \_ = dbContext.SaveChangesAsync(cancellationToken); bị bỏ await. Thao tác lưu DB chạy ngầm không kiểm soát (fire-and-forget), gây race condition và ObjectDisposedException khi HTTP request kết thúc.

## P1 - High (Ảnh hưởng kiểm thử & Tương thích)

3. @ExternalLoginService.cs:L33-35 - Lỗi tương thích khi chạy Unit/Integration Tests với InMemory Database
   - Vấn đề: ExternalLoginService.LoginAsync gọi trực tiếp await dbContext.Database.BeginTransactionAsync(cancellationToken) mà không kiểm tra dbContext.Database.IsRelational().
   - EF Core InMemory Provider không hỗ trợ transaction và sẽ ném InvalidOperationException làm gãy các bộ test tự động.

## P2 - Medium (Chất lượng mã & Chuẩn đặt tên)

4. @CurrentUserMapper.cs:L3 - Sai chính tả namespace Servies → Services
   - namespace GdscSharingPlatform.Infrastructure.Identity.Servies; bị gõ sai chính tả từ Services.

5. @IExternalLoginService.cs:L9 & UserSessionService.cs:L22 - Sai chính tả tham số ipAdress → ipAddress

6. @AuthService.cs:L85-97 - Dọn dẹp code rác & commented code
   - Xóa bỏ các đoạn mã comment thừa sau khi hoàn tất refactor sang UserSessionService hoặc giữ nguyên kiến trúc chuẩn.

## P3 - Low (Gợi ý hoàn thiện)

7. GoogleAuthController.cs:L102 - Redirect flow cho Frontend
   - Hiện tại /api/auth/google/complete trả về JSON Ok(response.User) sau khi set Cookie. Khi kết nối với Web Client, nên cấu hình redirect về trang Dashboard/Home của Frontend (ví dụ return Redirect("/admin") hoặc qua tham số returnUrl an toàn).

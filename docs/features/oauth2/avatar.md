# Avatar từ Google OIDC

Luồng: claim `picture` của principal đã được OIDC middleware xác minh →
`VerifiedExternalIdentity.AvatarUrl` → `ApplicationUser.AvatarUrl` → profile trong
session response và `/api/auth/me` → BFF → giao diện.

- Scope `profile` đã có sẵn; không thêm quyền hoặc lưu provider token.
- Khi tạo tài khoản mới hoặc đăng nhập lại tài khoản Google chưa có avatar,
  nhập URL ảnh nếu hợp lệ. Không ghi đè avatar đã có, kể cả ảnh Google trước đó.
- Chỉ nhận URL HTTPS tuyệt đối, host DNS không phải loopback, cổng mặc định,
  không credentials/fragment/control characters, tối đa 2048 ký tự.
- Claim thiếu hoặc URL không hợp lệ: bỏ qua ảnh, vẫn đăng nhập bình thường.
- Thay đổi được lưu cùng session trong transaction hiện tại. Không cần migration
  vì cột AvatarUrl đã tồn tại. Tài khoản bị khóa/inactive/deleted không được cập nhật.
- Frontend tải ảnh HTTPS ngoài trực tiếp từ browser (`unoptimized`), không mở
  image proxy cho mọi host. Không gửi Referer. Avatar upload nội bộ giữ cách xử lý cũ.
- Không tải/copy file Google về server. URL ảnh có thể đổi/hết hiệu lực; người dùng
  vẫn có thể upload avatar riêng. Nếu xóa avatar, lần đăng nhập Google kế tiếp
  có thể nhập lại ảnh vì tài khoản không còn avatar.

Tài khoản cũ: đăng xuất rồi đăng nhập Google lại để bổ sung avatar còn thiếu.
Chỉ refresh token hoặc tải lại trang không nhập lại metadata từ Google.

Tham khảo: [Google OIDC — picture claim](https://developers.google.com/identity/openid-connect/openid-connect).

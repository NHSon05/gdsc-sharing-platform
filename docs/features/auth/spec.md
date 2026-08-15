Sprint 1 — Authentication & Member Management
Mục tiêu: Admin có thể tạo và quản lý tài khoản; user có thể đăng nhập an toàn.
Backend
Đăng nhập bằng email và mật khẩu.
Tạo access token.
Refresh token rotation.
Lưu refresh token dạng hash.
Đăng xuất và thu hồi refresh token.
Đổi mật khẩu.
Admin reset mật khẩu tạm thời.
Chặn đăng nhập với user:
Banned
Inactive
Deleted
CRUD thành viên.
Tìm kiếm, lọc và phân trang.
Soft delete user.
Chặn và mở chặn.
Gán hoặc thu hồi role Admin.
Không cho phép xóa Admin cuối cùng.
API trang cá nhân.
API hồ sơ công khai.
Ghi audit đăng nhập và hành động quản trị.
Frontend
Trang đăng nhập.
Trang thông tin cá nhân.
Danh sách thành viên.
Form thêm và cập nhật thành viên.
Chặn/mở chặn thành viên.
Gán role Admin.
Trang hồ sơ công khai.
Protected routes theo role.

POST /api/v1/auth/login
POST /api/v1/auth/refresh
POST /api/v1/auth/logout
POST /api/v1/auth/change-password

GET /api/v1/users
POST /api/v1/users
GET /api/v1/users/{id}
PATCH /api/v1/users/{id}
DELETE /api/v1/users/{id}

POST /api/v1/users/{id}/roles
DELETE /api/v1/users/{id}/roles/{role}

GET /api/v1/me
PATCH /api/v1/me
GET /api/v1/members/{id}

Testing bắt buộc
Active user đăng nhập thành công.
Banned user không thể đăng nhập.
Member gọi API quản lý user nhận 403.
Admin có thể nâng Member thành Admin.
User sau khi nâng quyền có cả hai role.
Không thể thu hồi Admin cuối cùng.
Hồ sơ công khai không làm lộ dữ liệu nhạy cảm.

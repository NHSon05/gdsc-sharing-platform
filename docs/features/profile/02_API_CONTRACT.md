## 14. API Contract — Member Profile

### 14.1. Lấy profile hiện tại

```http
GET /api/profile/me
Authorization: Bearer <access-token>
```

Response `200 OK`:

```json
{
  "id": "4c2adaca-0950-4fb5-a97b-e57cd36f8365",
  "displayName": "Nguyen Van An",
  "email": "an@example.com",
  "phoneNumber": "+84901234567",
  "studentCode": "21IT001",
  "githubUrl": "https://github.com/nguyenvanan",
  "bio": "Backend developer interested in distributed systems.",
  "avatarUrl": "https://cdn.example.com/avatars/user-id/avatar.webp",
  "systemRoles": "MEMBER",
  "memberships": [
    {
      "id": "club-membership-id-gen-2",
      "generation": {
        "id": "generation-id-2",
        "number": 2,
        "name": "Gen 2"
      },
      "isActive": false,
      "departments": [
        {
          "id": "department-membership-id-1",
          "department": {
            "id": "software-id",
            "name": "Software",
            "slug": "software"
          },
          "isPrimary": true,
          "roles": [
            { "id": "core-role-id", "code": "CORE TEAM", "name": "Core Team" }
          ]
        }
      ]
    },
    {
      "id": "club-membership-id-gen-3",
      "generation": {
        "id": "generation-id-3",
        "number": 3,
        "name": "Gen 3"
      },
      "isActive": true,
      "departments": [
        {
          "id": "department-membership-id-2",
          "department": {
            "id": "software-id",
            "name": "Software",
            "slug": "software"
          },
          "isPrimary": true,
          "roles": [
            { "id": "lead-role-id", "code": "LEAD", "name": "Lead" },
            { "id": "core-role-id", "code": "CORETEAM", "name": "Core Team" }
          ]
        },
        {
          "id": "department-membership-id-3",
          "department": {
            "id": "ai-id",
            "name": "AI",
            "slug": "ai"
          },
          "isPrimary": false,
          "roles": [
            { "id": "sublead-role-id", "code": "SUBLEAD", "name": "Sub Lead" },
            { "id": "core-role-id", "code": "CORETEAM", "name": "Core Team" }
          ]
        }
      ]
    }
  ],
  "profileCompletionPercentage": 100,
  "missingProfileFields": [],
  "updatedAtUtc": "2026-09-02T10:30:00Z"
}
```

### 14.2. Cập nhật thông tin cá nhân

```http
PUT /api/profile/me
Authorization: Bearer <access-token>
Content-Type: application/json
```

Request:

```json
{
  "displayName": "Nguyen Van An",
  "phoneNumber": "+84901234567",
  "studentCode": "21IT001",
  "githubUrl": "https://github.com/nguyenvanan",
  "bio": "Backend developer interested in distributed systems."
}
```

Response:

- `200 OK`: trả profile mới nhất.
- `400 Bad Request`: validation thất bại.
- `401 Unauthorized`: chưa đăng nhập hoặc token sai.
- `404 Not Found`: user không tồn tại.
- `409 Conflict`: StudentCode đã được sử dụng.

### 14.3. Upload avatar

```http
POST /api/profile/me/avatar
Authorization: Bearer <access-token>
Content-Type: multipart/form-data
```

Form field:

```text
avatar
```

Response `200 OK`:

```json
{
  "avatarUrl": "https://cdn.example.com/avatars/user-id/avatar.webp"
}
```

### 14.4. Xóa avatar

```http
DELETE /api/profile/me/avatar
Authorization: Bearer <access-token>
```

Response:

```text
204 No Content
```

### 14.5. Yêu cầu đổi email

```http
POST /api/profile/me/email/change-request
Authorization: Bearer <access-token>
Content-Type: application/json
```

Request:

```json
{
  "newEmail": "new-email@example.com",
  "currentPassword": "current-password"
}
```

Response:

```text
202 Accepted
```

### 14.6. Xác nhận đổi email

```http
POST /api/profile/email/confirm
Content-Type: application/json
```

Sau xác nhận:

- cập nhật email bằng `UserManager`;
- tăng `TokenVersion`;
- revoke refresh token đang hoạt động;
- yêu cầu đăng nhập lại.

## 15. API Contract — Lookup

### 15.1. Danh sách Gen

```http
GET /api/generations?includeInactive=false
Authorization: Bearer <access-token>
```

### 15.2. Danh sách Department

```http
GET /api/departments?includeInactive=false
Authorization: Bearer <access-token>
```

### 15.3. Danh sách Club Role

```http
GET /api/club-roles?includeInactive=false
Authorization: Bearer <access-token>
```

Member được xem lookup đang hoạt động. Chỉ Admin được yêu cầu dữ liệu inactive.

## 16. API Contract — Admin Department

### 16.1. Tạo Department

```http
POST /api/admin/departments
Authorization: Bearer <admin-access-token>
Content-Type: application/json
```

Request:

```json
{
  "name": "Research",
  "slug": "research",
  "description": "Research and technical exploration team.",
  "color": "#2563EB",
  "icon": "flask",
  "sortOrder": 60
}
```

Response:

```text
201 Created
```

### 16.2. Cập nhật Department

```http
PUT /api/admin/departments/{departmentId}
Authorization: Bearer <admin-access-token>
```

### 16.3. Deactivate Department

```http
DELETE /api/admin/departments/{departmentId}
Authorization: Bearer <admin-access-token>
```

`DELETE` thực hiện deactivate/soft delete, không xóa lịch sử assignment.

### 16.4. Kích hoạt lại Department

```http
POST /api/admin/departments/{departmentId}/activate
Authorization: Bearer <admin-access-token>
```

## 17. API Contract — Admin Gen

### 17.1. Tạo Gen

```http
POST /api/admin/generations
Authorization: Bearer <admin-access-token>
```

Request:

```json
{
  "number": 4,
  "startDate": "2026-09-01",
  "endDate": "2027-08-31"
}
```

### 17.2. Cập nhật Gen

```http
PUT /api/admin/generations/{generationId}
```

### 17.3. Deactivate Gen

```http
DELETE /api/admin/generations/{generationId}
```

Không xóa ClubMembership lịch sử.

## 18. API Contract — Admin Member Membership

### 18.1. Gán Member vào Gen

```http
POST /api/admin/members/{userId}/memberships
Authorization: Bearer <admin-access-token>
Content-Type: application/json
```

Request:

```json
{
  "generationId": "generation-id-3",
  "joinedAt": "2026-09-01"
}
```

Response:

```text
201 Created
```

### 18.2. Thêm Member vào Department của Gen

```http
POST /api/admin/members/{userId}/memberships/{clubMembershipId}/departments
```

Request:

```json
{
  "departmentId": "software-id",
  "isPrimary": true,
  "roleIds": ["lead-role-id", "core-role-id"]
}
```

Hệ thống tạo DepartmentMembership và nhiều RoleAssignment trong một transaction.

### 18.3. Cập nhật Department Membership

```http
PUT /api/admin/members/{userId}/department-memberships/{departmentMembershipId}
```

Request:

```json
{
  "isPrimary": false,
  "isActive": true
}
```

### 18.4. Thay thế danh sách Role đang hoạt động

```http
PUT /api/admin/members/{userId}/department-memberships/{departmentMembershipId}/roles
```

Request:

```json
{
  "roleIds": ["sublead-role-id", "core-role-id"]
}
```

Quy tắc transaction:

1. Validate toàn bộ Role trước khi cập nhật.
2. Kết thúc RoleAssignment không còn trong danh sách.
3. Giữ assignment đã tồn tại và còn hợp lệ.
4. Tạo assignment mới.
5. Nếu một bước thất bại, rollback toàn bộ.

### 18.5. Kết thúc Department Membership

```http
DELETE /api/admin/members/{userId}/department-memberships/{departmentMembershipId}
```

Hệ thống:

- đặt `IsActive = false`;
- đặt `LeftAt`;
- kết thúc tất cả RoleAssignment đang hoạt động;
- giữ dữ liệu lịch sử.

### 18.6. Kết thúc Club Membership

```http
DELETE /api/admin/members/{userId}/memberships/{clubMembershipId}
```

Hệ thống kết thúc toàn bộ DepartmentMembership và RoleAssignment còn hoạt động
trong Gen đó.

## 19. Status Code

| Trường hợp                                    | HTTP Status |
| --------------------------------------------- | ----------: |
| Thành công lấy/cập nhật                       |       `200` |
| Tạo mới thành công                            |       `201` |
| Email confirmation được tiếp nhận             |       `202` |
| Xóa/deactivate thành công                     |       `204` |
| Validation thất bại                           |       `400` |
| Không đăng nhập/token sai                     |       `401` |
| Không có quyền Admin                          |       `403` |
| Không tìm thấy resource                       |       `404` |
| StudentCode, Name, Slug hoặc assignment trùng |       `409` |
| File vượt giới hạn                            |       `413` |
| MIME/file signature không hợp lệ              |       `415` |

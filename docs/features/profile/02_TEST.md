## 31. Unit Test

### 31.1. ClubGeneration

- Tạo Gen với Number hợp lệ.
- Từ chối Number bằng 0 hoặc âm.
- Từ chối EndDate trước StartDate.
- Name được chuẩn hóa đúng.

### 31.2. Department

- Tạo Department hợp lệ.
- Trim Name và chuẩn hóa Slug.
- Từ chối Name/Slug rỗng.
- Deactivate giữ dữ liệu.

### 31.3. Membership

- Một User tham gia nhiều Gen.
- Từ chối duplicate User + Gen.
- Một ClubMembership có nhiều Department.
- Từ chối duplicate Department trong cùng ClubMembership.
- Một DepartmentMembership có nhiều Role.
- Từ chối duplicate active Role.
- Kết thúc DepartmentMembership kết thúc active Roles.
- Member không tự gán Role.

### 31.4. Profile

- DisplayName validation.
- StudentCode normalization và uniqueness handling.
- Bio max length.
- Profile completion calculation.

## 32. Integration Test

- Anonymous GET profile trả `401`.
- Profile hợp lệ trả nhiều Gen/Department/Role đúng cấu trúc.
- Response không lộ trường Identity nhạy cảm.
- Member update personal fields thành công.
- Member gửi membership fields trong profile update bị từ chối/ignore theo
  contract đã chọn.
- Admin tạo Department thành công.
- Department trùng Name/Slug trả `409`.
- Deactivate Department giữ membership cũ.
- Admin tạo Gen thành công.
- Gen Number trùng trả `409`.
- Một Member được gán nhiều Gen.
- Một Member được gán nhiều Department trong cùng Gen.
- Một DepartmentMembership được gán nhiều Role.
- Duplicate assignment trả `409`.
- Member gọi Admin membership API trả `403`.
- Role replacement rollback khi có một RoleId không hợp lệ.
- Kết thúc membership giữ dữ liệu lịch sử.
- Migration backfill dữ liệu legacy hợp lệ.
- Seeder chạy hai lần không tạo record trùng.
- Avatar và email-change tests vẫn pass.

Integration Test dùng PostgreSQL thật qua Testcontainers.

## 33. Frontend Test

- Hiển thị nhiều Gen đúng thứ tự.
- Hiển thị nhiều Department trong một Gen.
- Hiển thị nhiều Role badge trong một Department.
- Empty membership state rõ ràng.
- Inactive Gen/Department vẫn hiển thị lịch sử.
- Member không thấy control sửa Role.
- Admin role multi-select hoạt động.
- Admin department multi-select hoạt động.
- Department mới xuất hiện sau mutation/cache invalidation.
- Profile validation hiển thị cạnh field.
- Avatar upload và email change flow hoạt động.
- Mobile không tràn ngang.

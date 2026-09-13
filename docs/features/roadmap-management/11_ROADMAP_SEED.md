# Seed roadmap và luồng Admin → Member

Danh mục chỉ dùng để phân loại. Roadmap, node và edge là dữ liệu riêng trong database, được dùng chung cho cả API Admin và Member.

- `GET /api/admin/roadmaps`: Admin xem tất cả roadmap, kể cả Draft.
- `PATCH /api/admin/roadmaps/{id}`: Admin chỉnh sửa thông tin roadmap.
- `PATCH /api/admin/roadmaps/{roadmapId}/nodes/{nodeId}`: Admin chỉnh sửa bài học trong graph.
- `GET /api/roadmaps`: Member xem danh sách Published và Archived.
- `GET /api/roadmaps/{slug}`: Member xem graph cùng dữ liệu đã được Admin lưu. Draft bị ẩn.
- Member gọi endpoint chỉnh sửa của Admin nhận `403 Forbidden`.

Không có bản sao roadmap dành riêng cho Member và không cần seed lại sau mỗi lần Admin chỉnh sửa. Member lấy dữ liệu mới khi tải lại trang hoặc truy vấn API.

## Dữ liệu khởi đầu

`DatabaseSeeder.SeedAsync` chạy `SeedRoadmapsAsync` sau khi khởi tạo tài khoản. Khi có ít nhất một tài khoản Admin đang hoạt động, seeder bổ sung các roadmap Published sau:

| Roadmap | Slug | Node | Edge |
| --- | --- | ---: | ---: |
| Frontend | frontend | 5 | 4 |
| Backend | backend | 5 | 4 |
| Database | database | 5 | 4 |
| Software Engineering | software-engineering | 5 | 4 |
| Fullstack | fullstack | 5 | 4 |
| Artificial Intelligence | artificial-intelligence | 5 | 4 |

Mỗi node có mục tiêu học tập và vị trí trên canvas. Các node nối theo thứ tự bằng Required/Solid; Admin có thể biên soạn lại, thêm tài liệu và điều chỉnh graph bằng API quản trị hiện có.

Seeder sử dụng tài khoản Admin thực làm tác giả, không tạo tài khoản giả. Nếu chưa có Admin, seeder ghi log và hoãn việc tạo roadmap; lần khởi động tiếp theo khi đã có Admin sẽ bổ sung. Danh mục tương ứng phải tồn tại và đang hoạt động.

ID seed cố định và kiểm tra cả ID lẫn slug giúp tránh tạo trùng, kể cả khi Admin đã đổi slug. Roadmap có sẵn không bị ghi đè hoặc thêm lại node/edge đã sửa hay vô hiệu hóa. Tất cả graph mới được ghi bằng một `SaveChanges`, có transaction khi chạy PostgreSQL.

## Khởi chạy

Seeder được gọi tự động khi API khởi động. Với Docker local, chạy từ thư mục `backend`:

```sh
docker compose up -d --build --no-deps api
```

API chạy migration còn thiếu theo cơ chế khởi động hiện có, sau đó chạy seeder. Sau khi khởi động, đăng nhập Admin để chỉnh sửa; đăng nhập Member và mở `/roadmaps` để xem dữ liệu đã lưu.

## Xác nhận triển khai local

- Build backend và publish Release thành công; 274 unit test và 59 integration test pass, không có test bị bỏ qua. Integration test sử dụng PostgreSQL tạm, có kiểm tra Admin chỉnh sửa roadmap seed, Member đọc thay đổi, Member sửa bị từ chối và seed lại giữ nguyên nội dung.
- Đã nạp database local: 6 roadmap Published, 30 node và 24 edge; readiness báo Healthy.
- Đọc API local bằng tài khoản Admin cấu hình: `/api/admin/roadmaps` và `/api/roadmaps` đều trả 200 với 6 bản ghi; `/api/roadmaps/frontend` trả 5 node và 4 edge.
- Image local được đóng gói từ bản publish Release và runtime API đã có để tránh tải lại SDK. Dockerfile của repository giữ nguyên cho các lần build thông thường.

# Sprint 3 — Yêu cầu nghiệp vụ Roadmap

## 1. Vai trò người dùng

### Member

Member có thể:

- Xem danh sách roadmap đã công khai.
- Tìm kiếm theo tên roadmap.
- Lọc theo lĩnh vực và mức độ.
- Xem roadmap dưới dạng sơ đồ node–edge.
- Phóng to, thu nhỏ và kéo vùng hiển thị.
- Xem các nhánh chính, nhánh phụ và quan hệ phụ thuộc.
- Bấm vào từng node để xem nội dung chi tiết.
- Mở tài liệu dạng liên kết.
- Xem hoặc tải tài liệu dạng tệp.

Member không được:

- Xem roadmap ở trạng thái Draft.
- Tạo, sửa, sắp xếp hoặc thay đổi trạng thái roadmap.
- Quản lý node, đường nối hoặc tài liệu.

### Admin

Admin có toàn bộ quyền của Member và có thể:

- Tạo và cập nhật roadmap.
- Công khai, đưa về bản nháp hoặc lưu trữ roadmap.
- Tạo, cập nhật, kéo thả vị trí và vô hiệu hóa node.
- Tạo, cập nhật hoặc xóa đường nối giữa các node.
- Thêm tài liệu dạng liên kết.
- Tải tài liệu dạng tệp lên hệ thống.
- Cập nhật, thay thế, sắp xếp hoặc vô hiệu hóa tài liệu.
- Quản lý danh mục roadmap động.

## 2. Trạng thái Roadmap

| Trạng thái  | Ý nghĩa                                  |
| ----------- | ---------------------------------------- |
| `Draft`     | Bản nháp, chỉ Admin được xem             |
| `Published` | Được công khai cho Member                |
| `Archived`  | Đã lưu trữ, và được công khai cho member |

Quy tắc:

- Roadmap mới mặc định là `Published`.
- Khi công khai lần đầu, hệ thống lưu `PublishedAtUtc`.
- Roadmap `Archived` không được thêm node, đường nối hoặc tài liệu mới.
- Roadmap đã có nội dung không bị xóa cứng.

## 3. Danh mục và mức độ

Danh mục ban đầu:

```text
Frontend
Backend
Database
Software Engineering
Fullstack
AI
Data Engineering
Machine Learning
MLOps
Data Analyst
Solution Architecture
Business Analyst
Business
Marketing
UX/UI Design
Media
```

Danh mục là dữ liệu động. Admin có thể thêm, cập nhật, kích hoạt hoặc vô hiệu hóa danh mục.

Mức độ roadmap:

```text
Beginner
Intermediate
Advanced
AllLevels
```

## 4. Quy tắc Roadmap

- Tên roadmap bắt buộc và tối đa 150 ký tự.
- Slug bắt buộc, được chuẩn hóa và duy nhất toàn hệ thống.
- Roadmap phải thuộc một danh mục đang hoạt động.
- `SortOrder` là số nguyên không âm.
- Vô hiệu hóa danh mục không làm mất các roadmap đã thuộc danh mục đó.

## 5. Quy tắc node và đường nối

- Một roadmap có nhiều node và nhiều đường nối.
- Node phải thuộc một roadmap tồn tại.
- Mỗi node có loại `Topic`, `Group` hoặc `Milestone`.
- Node lưu tọa độ để frontend tái tạo đúng bố cục do Admin thiết kế.
- Node có thể dùng màu và icon để phân biệt nhóm kiến thức.
- Đường nối phải thuộc cùng roadmap với node nguồn và node đích.
- Không cho phép một node nối đến chính nó.
- Không cho phép hai đường nối hoàn toàn trùng nhau.
- Mặc định không cho phép tạo chu trình đối với quan hệ `Required`.
- `Required` hiển thị bằng đường liền; `Recommended` hoặc `Optional` có thể dùng nét đứt.
- Node không hoạt động không hiển thị cho Member.
- Khi node bị vô hiệu hóa, đường nối và tài liệu liên quan không hiển thị nhưng vẫn được giữ.

## 6. Quy tắc tài liệu

Sprint 3 hỗ trợ hai loại:

```text
Link
File
```

Với tài liệu `Link`:

- Bắt buộc có `ExternalUrl`.
- Chỉ chấp nhận URL HTTP hoặc HTTPS hợp lệ.
- Không được chứa thông tin tệp.

Với tài liệu `File`:

- Bắt buộc có tệp đã được lưu thành công.
- Không được chứa `ExternalUrl`.
- Phải lưu tên gốc, tên lưu trữ, kích thước và loại nội dung.
- Chỉ hiển thị khi roadmap, node và chính tài liệu đều khả dụng.

## 7. Phân quyền

| Chức năng                  | Member | Admin |
| -------------------------- | :----: | :---: |
| Xem roadmap Published      |   Có   |  Có   |
| Xem roadmap Archived      |   Có   |  Có   |
| Xem roadmap Draft         | Không  |  Có   |
| Xem và tương tác với sơ đồ |   Có   |  Có   |
| Mở liên kết hoặc tải tệp   |   Có   |  Có   |
| Quản lý danh mục           | Không  |  Có   |
| Quản lý roadmap            | Không  |  Có   |
| Quản lý node và đường nối  | Không  |  Có   |
| Quản lý tài liệu           | Không  |  Có   |

Quyền phải được kiểm tra tại backend, không phụ thuộc vào việc ẩn hoặc hiện nút trên frontend.

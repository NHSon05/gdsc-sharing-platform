# Sprint 3 — Giao diện Roadmap trực quan

## 1. Mục tiêu hiển thị

Roadmap phải được trình bày dưới dạng sơ đồ trực quan tương tự roadmap công nghệ: mỗi kiến thức là một node hình chữ nhật, các node được nối với nhau bằng đường có hướng để thể hiện lộ trình học, nhánh kiến thức và quan hệ phụ thuộc.

Không hiển thị roadmap dưới dạng danh sách bài học hoặc timeline một cột.

## 2. Thành phần của canvas

### Node chính

- Thể hiện chủ đề hoặc cột mốc quan trọng.
- Có màu nổi bật hơn node con.
- Có thể là điểm bắt đầu của nhiều nhánh.
- Ví dụ: `Frontend Fundamentals`, `Testing`, `Security`.

### Node kiến thức

- Thể hiện một công nghệ hoặc nội dung cụ thể.
- Có tiêu đề ngắn gọn.
- Bấm vào node để mở chi tiết.
- Ví dụ: `HTML`, `CSS`, `React`, `Jest`.

### Node nhóm

- Đại diện hoặc bao quanh một nhóm kiến thức liên quan.
- Có thể chứa nhiều node con về mặt hiển thị.
- Ví dụ: `Design Patterns`, `State Management`.

### Đường nối

- Đường liền: nội dung bắt buộc hoặc lộ trình chính.
- Đường nét đứt: nội dung khuyến nghị, tùy chọn hoặc tham khảo.
- Đường nối có hướng từ node nguồn đến node đích.
- Các đường nên tránh che chữ và hạn chế giao nhau.

## 3. Tương tác của Member

- Kéo canvas theo mọi hướng.
- Phóng to và thu nhỏ.
- Đưa toàn bộ roadmap vừa với màn hình.
- Trở về node bắt đầu.
- Bấm node để mở drawer hoặc modal chi tiết.
- Node được chọn có viền hoặc màu nổi bật.
- Tìm node theo tên và đưa kết quả vào giữa màn hình.
- Xem chú giải màu và kiểu đường nối.

## 4. Nội dung khi bấm vào node

Panel chi tiết hiển thị:

- Tên node.
- Mô tả kiến thức.
- Mục tiêu học tập.
- Thời gian dự kiến.
- Các node cần học trước.
- Các node nên học tiếp theo.
- Tài liệu dạng liên kết.
- Tài liệu dạng tệp.

Không chuyển sang trang mới chỉ để xem thông tin ngắn. Có thể dùng trang riêng nếu nội dung rất dài.

## 5. Tương tác của Admin

- Tạo node mới trên canvas.
- Chọn loại node: Topic, Group hoặc Milestone.
- Kéo thả node đến vị trí mong muốn.
- Nối hai node bằng thao tác kéo từ điểm kết nối.
- Chọn loại quan hệ và kiểu đường.
- Sửa tiêu đề, màu, icon và trạng thái.
- Mở panel quản trị tài liệu của node.
- Lưu vị trí nhiều node trong một request, không gọi API liên tục trong lúc kéo.

## 6. Quy tắc bố cục

- Backend lưu `PositionX` và `PositionY` theo hệ tọa độ canvas.
- Frontend không thay đổi dữ liệu gốc theo kích thước màn hình.
- Trên màn hình nhỏ, canvas giữ bố cục và cho phép zoom/pan.
- Node có kích thước tối thiểu để tiêu đề đọc được.
- Tiêu đề dài được xuống dòng và không tràn khỏi node.
- Có khoảng trống đủ giữa các node để đường nối dễ quan sát.

## 7. Trạng thái hiển thị

| Trạng thái        | Cách hiển thị                          |
| ----------------- | -------------------------------------- |
| Bình thường       | Màu theo cấu hình node                 |
| Hover             | Tăng độ nổi hoặc hiện tooltip          |
| Đang chọn         | Viền nổi bật                           |
| Không hoạt động   | Chỉ Admin thấy, hiển thị mờ            |
| Không có tài liệu | Vẫn mở được phần mô tả                 |
| Có tài liệu       | Hiện biểu tượng hoặc số lượng tài liệu |

## 8. Khả năng truy cập

- Node có thể được chọn bằng bàn phím.
- Có focus state rõ ràng.
- Màu sắc không phải dấu hiệu duy nhất để phân biệt quan hệ.
- Node và đường nối có nhãn hỗ trợ screen reader khi thư viện cho phép.
- Panel chi tiết quản lý focus đúng khi mở và đóng.

## 9. Dữ liệu tối thiểu để render

```json
{
  "roadmap": { "id": "uuid", "title": "Frontend" },
  "nodes": [
    {
      "id": "html-node",
      "title": "HTML",
      "nodeType": "Topic",
      "position": { "x": 120, "y": 180 },
      "color": "#FFE19A",
      "resourceCount": 2
    }
  ],
  "edges": [
    {
      "id": "edge-1",
      "sourceNodeId": "start-node",
      "targetNodeId": "html-node",
      "relationType": "Required",
      "lineStyle": "Solid"
    }
  ]
}
```

## 10. Đề xuất kỹ thuật frontend

Có thể sử dụng React Flow để triển khai canvas, kết nối, zoom, pan, fit view và kéo thả. Thư viện chỉ chịu trách nhiệm hiển thị; dữ liệu node, edge, quyền và validation vẫn do backend quản lý.

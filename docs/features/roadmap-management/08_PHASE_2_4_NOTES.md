# Roadmap — Phase 2–4

## Đã triển khai

Application có request/response DTO, FluentValidation, mapping graph, quy tắc trạng thái/reorder/cycle, năm interface service và `IFileStorage`. Infrastructure triển khai Category, Roadmap, Node, Edge, Resource service và private local storage. API cung cấp toàn bộ route trong `03_API_CONTRACT.md`, dùng authentication, AdminPolicy và Swagger.

Member được xem và tải tài liệu của cả Published và Archived theo xác nhận của người dùng. Draft chỉ Admin truy cập. Node/resource inactive bị ẩn với Member; edge bị ẩn nếu chính nó hoặc một trong hai node không hoạt động. Vô hiệu hóa danh mục không ẩn roadmap đã thuộc danh mục đó.

Roadmap mới mặc định Published và có PublishedAtUtc. Khi chuyển từ Draft/Archived sang Published cần node active; gọi lại trạng thái Published hiện có không bị từ chối chỉ vì chưa có node. Publish lại không đổi thời điểm công khai đầu tiên. Archived ngăn tạo node, edge, link và file mới; chỉnh sửa nội dung hiện có hoặc thay tệp vẫn được phép.

## API và lỗi

Các route giữ prefix `/api/...` theo feature contract và backend hiện tại. AdminPolicy dùng cùng chính sách AdminOnly hiện có: authenticated, claim Active và role Admin. Service cũng kiểm tra role để tránh bỏ qua quyền khi gọi trực tiếp ngoài controller.

JSON enum nhận tên như `Published`, `Beginner`, `Topic`, `Required`, `Solid`, `Link`. Input enum ngoài miền giá trị bị từ chối. DTO không trả entity, storage key, tên tệp lưu nội bộ hoặc audit user ID.

- 400: validation, ID không thuộc collection, self-loop hoặc Required cycle.
- 401/403: chưa đăng nhập/không đủ quyền.
- 404: không có dữ liệu, tài liệu bị ẩn hoặc tệp không tồn tại.
- 409: trùng slug/edge/tên category, trạng thái không cho phép, cạnh tranh transaction.
- 413: vượt giới hạn tệp hoặc multipart request.
- 415: extension/MIME/signature không được hỗ trợ hoặc không khớp.
- 500: lỗi database/storage ngoài các trường hợp nghiệp vụ trên.

Problem Details chứa `traceId`, `validationErrors` dùng tên field camelCase; `errors` được giữ để tương thích API cũ. Swagger mô tả request/response, authorization, upload limit và binary download.

## Transaction và ràng buộc graph

Mỗi thao tác ghi chạy trong transaction Serializable trên PostgreSQL. SaveChanges chỉ thực hiện sau khi validation nghiệp vụ hoàn tất. Predicate checks đối với cycle, slug và reorder không thể bị hai writer cùng vượt qua rồi commit trạng thái không hợp lệ; bên thua nhận 409 để client tải lại và thử lại.

Batch positions cho phép cập nhật một phần node trong cùng roadmap, mỗi ID đúng một lần. Node ngoài roadmap làm cả request thất bại. Width hiện có được giữ khi chỉ lưu PositionX/PositionY. Reorder yêu cầu toàn bộ ID collection, kể cả inactive, và lưu thứ tự từ 0.

Required-cycle detection kiểm tra cả node/edge inactive để việc kích hoạt lại không tạo chu trình. Khi cập nhật edge, loại chính edge đó khỏi tập cạnh đang kiểm tra. Recommended/Optional có thể tạo vòng. Unique index ở database vẫn là rào bảo vệ cuối cùng cho trùng slug và edge; lỗi unique, serialization, deadlock được chuyển thành ConflictException/409.

## File storage

Mặc định:

```json
{
  "RoadmapStorage": {
    "RootPath": "App_Data/roadmap-resources",
    "MaxFileBytes": 20971520
  }
}
```

RootPath có thể là đường dẫn tuyệt đối hoặc tương đối với ContentRootPath; phải nằm ngoài wwwroot. Docker Compose dùng volume `roadmap_resources` để giữ tệp qua việc tạo lại container. App_Data được bỏ qua trong Git.

- Giới hạn mặc định 20 MiB; cấu hình có thể giảm xuống, không vượt 20 MiB.
- Request multipart tối đa 21 MiB để chừa phần metadata.
- Cho phép PDF, PNG, JPEG, TXT, MD, DOCX, PPTX, XLSX với MIME tương ứng.
- Kiểm tra chữ ký PDF/ảnh; Office kiểm tra cấu trúc ZIP có entry tương ứng và loại bỏ gói chứa VBA; TXT/MD phải là UTF-8 không có byte NUL. Đây là kiểm tra định dạng, không phải quét malware.
- Đọc stream có đếm byte thực tế, không chỉ tin Length. Tệp rỗng/length sai bị từ chối.
- Tên gốc được bỏ thành phần đường dẫn; storage key là UUID và extension do server sinh.
- Upload viết tệp `.upload`, kiểm tra nội dung rồi mới chuyển thành tệp hoàn chỉnh. Lỗi upload dọn tệp tạm.
- Database insert/update thất bại: dọn tệp mới sau khi xác nhận không có resource nào tham chiếu.
- Replace thành công: commit metadata mới trước, sau đó mới dọn tệp cũ. Replace thất bại giữ nguyên metadata và tệp cũ.
- Nếu kết quả commit không chắc chắn hoặc database/storage không truy cập được lúc cleanup, giữ tệp và ghi log `Resource file cleanup needs retry` kèm key để xử lý lại; không xóa tệp có thể còn được database tham chiếu.
- DELETE resource chỉ vô hiệu hóa; giữ tệp và metadata để có thể kích hoạt lại.

Download luôn kiểm tra authentication/role và trạng thái roadmap–node–resource trước khi mở stream. Response dùng attachment với tên gốc, Content-Type thực tế, `X-Content-Type-Options: nosniff` và `Cache-Control: private, no-store`. Không có URL static để vượt qua kiểm tra quyền.

## Kiểm thử

Từ thư mục backend, với PostgreSQL chuyên dùng cho test:

```bash
ROADMAP_TEST_POSTGRES='Host=127.0.0.1;Port=55439;Username=postgres;Database=postgres' \
  dotnet test --no-restore --disable-build-servers -m:1
```

Mỗi test PostgreSQL tự tạo và xóa database `roadmap_tests_<uuid>`. CI đã thêm PostgreSQL service và biến môi trường này, nên các test transaction/migration không bị skipped trong CI.

Đã kiểm tra unit/domain/validator/storage, workflow API, 27 route Admin từ chối Member, swagger schema, giới hạn multipart, lọc/tìm kiếm, published/archive/draft, batch positions, reorder, file replacement, metadata an toàn và lỗi validation. PostgreSQL tests gồm migration, unique-to-conflict, hai transaction thêm edge tạo cycle, rollback batch khi câu UPDATE lỗi và dọn file khi SaveChanges thất bại. Regression Auth, Profile và Membership được chạy cùng bộ test.

Phase này không thay đổi schema so với migration Phase 1 và không triển khai frontend. Database ứng dụng đang chạy không được cập nhật trong quá trình kiểm thử; chỉ database tạm được dùng.

## Kết quả xác nhận

Build: 0 warning, 0 error. Toàn bộ 268 test pass: 217 unit test và 51 integration test; 0 skipped khi chạy với PostgreSQL 17 tạm. Đã bổ sung 33 API action và 12 validator.

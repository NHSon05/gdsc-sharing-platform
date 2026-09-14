# Sprint 4 — Phase 3–5: Backend Sharing

## Phạm vi triển khai

- Application: DTO, interface service, FluentValidation, quyền Owner/Contributor và chuyển giờ địa phương sang UTC.
- Infrastructure: Content, Resource, Tag, Schedule service; transaction Serializable; kiểm tra audience, presenter overlap và version.
- API: các route trong `03_API_CONTRACT.md`, authentication/authorization, Problem Details, Swagger, upload và submit rate limit.
- Audit: migration `20260913155147_AddSharingAuditLog` bổ sung `gdsc.SharingAuditEntries`. Audit được lưu cùng transaction với nghiệp vụ, gồm actor/action/entity/id/time/trace và metadata không chứa nội dung bài hay URL họp.

## Quyền và hành vi

- Draft Content chỉ Owner/Admin xem và sửa. Contributor được xem PendingReview/Rejected/Archived của bài mình tham gia, sửa nội dung và tài liệu khi Rejected; không đổi tác giả/tag hoặc gửi/rút duyệt.
- Owner được sửa toàn bộ Published và tài liệu đính kèm trực tiếp, giữ trạng thái Published và thời điểm công khai; thay đổi tăng version và ghi audit.
- Chỉ Owner submit/withdraw. Admin approve/reject PendingReview, return-to-draft Published/Archived và archive khi hợp lệ.
- Member tạo Schedule được tự gán làm Speaker, `SortOrder = 0`. Có thể gửi `presenters: []` hoặc chính mình với vai trò trên; không gán người khác.
- Member/Content Owner sửa lịch do mình tạo; quyền sở hữu Content không cho sửa lịch của người khác. Chỉ Admin đổi danh sách Presenter và publish/start/complete/cancel/delete Draft.
- Lịch Completed/Cancelled được khóa chỉnh sửa để giữ lịch sử. DELETE chỉ áp dụng Schedule Draft.
- Draft Schedule chỉ creator/Presenter/Admin được xem. Lịch công bố lọc theo AllMembers hoặc membership Gen/Department đang hoạt động (khớp ít nhất một điều kiện).
- Content liên kết và tài liệu áp dụng quyền riêng của Content; Contributor không thấy Draft ngay cả khi được xem Schedule.
- Đường dẫn họp chỉ trả cho audience hợp lệ, Presenter hoặc Admin. Response không trả danh sách thành viên trong audience.

## Request, response và concurrency

- Route giữ `/api/sharing/...` và `/api/admin/sharing/...` theo contract feature và convention API hiện có.
- Các endpoint cập nhật của Sprint 4 dùng `PATCH`; `PUT` đã được gỡ và trả 405. Request DTO, các field bắt buộc và kiểm tra `If-Match` giữ nguyên; đây không phải JSON Patch hoặc cơ chế bỏ qua field để cập nhật từng phần.
- JSON enum dùng tên, ví dụ `Published`, `Speaker`, `SelectedAudience`.
- Collection request phải có mặt, dùng `[]` khi rỗng: `tagIds`, `contributorUserIds`, `presenters`, `contentIds`, `generationIds`, `departmentIds`.
- `AllMembers` yêu cầu danh sách Gen/Department rỗng. `SelectedAudience` yêu cầu ít nhất một Gen hoặc Department.
- Content detail: `{ content, bodyMarkdown, reviewNote, submittedAtUtc, reviewedAtUtc, reviewedByUserId, authors, tags, resources, schedules }`; `content` chứa id/title/slug/summary/status/publishedAtUtc/version và authors/tags. Từng item của API danh sách cũng có authors/tags để hiển thị card mà không gọi detail từng bài; phân trang được áp dụng trước khi lấy các collection.
- Danh sách phân trang: `{ items, totalCount, page, pageSize }`, pageSize tối đa 100; sort Content: `newest`, `oldest`, `title`. Admin PendingReview mặc định ưu tiên thời gian gửi sớm.
- Các mutation Content/Schedule và mutation Resource phải có `If-Match: "<version>"`. Header thiếu/sai trả 400, version cũ trả 412. Resource dùng version của Content cha; cập nhật file, link, reorder hay soft-delete đều tăng version đó.
- Resource mutation trả `{ resource, version }`; DELETE/reorder trả 204 và ETag mới. Get detail/create/update trả ETag; CORS expose `ETag`, `Retry-After`, `Content-Disposition`.
- Các write dùng transaction Serializable, không tự retry. Trùng slug/Presenter hoặc xung đột predicate giữa các lịch trả 409; xung đột trên version của cùng aggregate trả 412.
- Thời gian đầu vào không có `Z`/offset, đi kèm `TimeZoneId` IANA; từ chối giờ DST không tồn tại/nhập nhằng. Lưu UTC. Lỗi giờ bắt đầu/kết thúc gắn đúng field `startsAtLocal`/`endsAtLocal`.

## Storage và Markdown

- Dùng lại `IFileStorage`/`LocalRoadmapFileStorage` và cấu hình `RoadmapStorage`, ngoài wwwroot. Giới hạn mặc định 20 MiB/tệp, giới hạn multipart 21 MiB.
- Định dạng thực tế theo storage Sprint 3: PDF, PNG, JPG/JPEG, TXT, MD, DOCX, PPTX, XLSX; kiểm tra extension/MIME/signature. Các định dạng DOC/PPT/XLS/ZIP trong đề xuất chưa được mở thêm.
- Upload/replace thất bại: chỉ dọn file mới sau khi xác nhận không được tham chiếu; replace thành công mới dọn file cũ. Nếu cleanup lỗi thì ghi log để vận hành xử lý. Soft-delete giữ metadata/file để truy vết, chặn download.
- Download yêu cầu quyền Content, trả attachment và `nosniff`; response Sharing dùng `no-store`.
- Backend lưu/trả Markdown gốc như text JSON, không render HTML. Phase 6 phải dùng Markdown renderer tắt raw HTML hoặc sanitizer được phê duyệt.
- Upload/replace giới hạn 10 request/phút/user; submit 5 request/phút/user. Vượt giới hạn trả 429 Problem Details và Retry-After.

## Kiểm thử

Xác minh ngày 2026-09-14: build thành công; 287 unit test và 70 integration test pass, không có test bị skip. Integration suite chạy với PostgreSQL 18 tạm, bao gồm migration và regression Sprint 1–3.

- `SharingRulesTests`: URL, quyền Draft/Published, DST, validation collection/audience.
- `SharingResourceFailureTests`: lỗi database không thay đổi version/metadata/audit và dọn file mới.
- `SharingEndpointsTests`: HTTP workflow, liên kết tác giả/tag/presenter/content, audience, version, upload/download/replace, rate limit, Swagger.
- `SharingPostgresTests`: hai lịch trùng Presenter publish đồng thời chỉ một thành công; hai update Content cùng version trả một lỗi 412; database failure rollback file metadata/version/audit.
- PostgreSQL tests dùng `ROADMAP_TEST_POSTGRES`, tự tạo/xóa database tên ngẫu nhiên. Không apply migration vào database ứng dụng trong bước kiểm thử.

Frontend thuộc Phase 6; các quyền và validation bảo mật đã được kiểm tra ở backend.

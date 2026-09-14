# Sprint 4 — Sharing Content và Sharing Schedule

## Mục tiêu

Sprint 4 xây dựng không gian chia sẻ kiến thức nội bộ cho thành viên GDSC, gồm hai phân hệ liên kết với nhau:

- `Sharing Content`: Thành viên soạn bài chia sẻ, đính kèm tài liệu và gửi Admin duyệt.
- `Sharing Schedule`: Member và Admin có thể lập lịch các buổi sharing. Member khi tự lập lịch sẽ được gán làm Presenter của chính lịch đó; Admin có thể chọn Member để gán làm Presenter, xác định đối tượng tham dự và liên kết nội dung liên quan.

## Cấu trúc nghiệp vụ

```text
SharingContent
├── SharingContentAuthor
├── SharingResource
├── SharingContentTag
└── SharingScheduleContent
    └── SharingSchedule
        ├── SharingSchedulePresenter
        ├── SharingScheduleAudienceGeneration
        └── SharingScheduleAudienceDepartment
```

## Danh sách tài liệu

1. [01_REQUIREMENTS.md](./01_REQUIREMENTS.md): Phạm vi, vai trò, phân quyền và quy tắc nghiệp vụ.
2. [02_DATA_MODEL.md](./02_DATA_MODEL.md): Entity, quan hệ, index và migration.
3. [03_API_CONTRACT.md](./03_API_CONTRACT.md): Endpoint, request, response và mã lỗi.
4. [04_SHARING_CONTENT_WORKFLOW.md](./04_SHARING_CONTENT_WORKFLOW.md): Vòng đời bài chia sẻ và quy trình duyệt.
5. [05_SHARING_SCHEDULE_WORKFLOW.md](./05_SHARING_SCHEDULE_WORKFLOW.md): Vòng đời lịch sharing, người trình bày và đối tượng xem.
6. [06_UI_UX.md](./06_UI_UX.md): Màn hình và hành vi giao diện đề xuất.
7. [07_FILE_STORAGE_AND_SECURITY.md](./07_FILE_STORAGE_AND_SECURITY.md): Tệp, liên kết, phân quyền và bảo mật.
8. [08_TEST_PLAN.md](./08_TEST_PLAN.md): Acceptance Criteria và kế hoạch kiểm thử.
9. [09_IMPLEMENTATION_PLAN.md](./09_IMPLEMENTATION_PLAN.md): Các phase triển khai và Definition of Done.
10. [10_PHASE_3_5_NOTES.md](./10_PHASE_3_5_NOTES.md): API thực tế, phân quyền, concurrency, storage và kiểm thử backend.

## Phạm vi Sprint 4

Bao gồm:

- Thành viên tạo và quản lý bản nháp của bài chia sẻ.
- Hỗ trợ nhiều tác giả cho một bài chia sẻ.
- Gửi duyệt, duyệt, từ chối, công khai và lưu trữ nội dung.
- Đính kèm liên kết hoặc tệp.
- Admin tạo và quản lý lịch sharing.
- Một lịch có nhiều người trình bày và nhiều nội dung liên quan.
- Lịch có thể áp dụng cho toàn bộ thành viên hoặc theo Gen/Department.
- Hiển thị lịch dạng danh sách và lịch tháng/tuần.

Chưa bao gồm:

- Đăng ký giữ chỗ hoặc giới hạn số người tham dự.
- Điểm danh và thống kê tham dự.
- Gửi email, push notification hoặc nhắc lịch tự động.
- Lịch lặp lại.
- Đồng bộ Google Calendar hoặc Outlook Calendar.
- Bình luận, lượt thích và đánh giá nội dung.
- Livestream hoặc lưu trữ video trực tiếp.

# Dataset luyện phỏng vấn

## Seed PostgreSQL local

```sh
node luyenphongvan/seed-postgres.mjs
```

Target cố định: container `gdsc-sharing-platform-postgres`, database `gdsc_sharing_platform`, bảng `gdsc."InterviewQuestions"`. Script dùng thông tin đăng nhập trong container, không ghi mật khẩu vào repository.

Bảng lưu toàn bộ record nguyên vẹn trong cột `Data` kiểu JSONB; các cột `Question`, `Slug`, `Level`, `Access`, `NeedsReview` được PostgreSQL tự sinh từ JSON. `Id` là UUID primary key; `Slug` unique. Department của dataset nằm trong `Data.departments`, chưa ánh xạ với bảng phòng ban thành viên của ứng dụng.

Seed chạy trong transaction, chỉ thêm UUID chưa tồn tại. Nếu dữ liệu của UUID có sẵn khác dataset, script rollback và báo lỗi thay vì ghi đè. Script đối chiếu JSONB đầy đủ trước commit. Chạy lại với dataset không đổi sẽ không thêm bản trùng.

1.003 câu thiếu đáp án và 8 câu có đáp án cần duyệt được đặt `Status = draft`; 3.059 câu còn lại có `Status = published` (chỉ biểu thị có đáp án trong dataset, không xác nhận tính đúng đắn chuyên môn).

Bảng đã được tích hợp với entity/DbSet EF Core và migration `AddInterviewQuestions`. API đọc tại `/api/v1/interview-questions` và `/api/v1/admin/interview-questions`; xem `docs/features/interview-questions/API.md`. `Data` vẫn giữ toàn bộ `answerVariants`, `sources`, `topics`, `departments` và metadata gốc. Script seed vẫn dùng được trước hoặc sau migration.

## Dữ liệu để import

Import `normalized/questions.json`: 4.070 câu hỏi duy nhất từ 4.187 records trong 57 file nguồn. 114 nhóm trùng được gộp theo nội dung câu hỏi giống hệt nhau sau khi trim và chuẩn hóa Unicode NFC. Không gộp theo độ giống ngữ nghĩa.

Các file JSON ở thư mục này là bản theo chủ đề đã chuẩn hóa; vẫn giữ đủ 4.187 lần xuất hiện để bảo toàn nguồn và đáp án. Không import đồng thời các file này và file hợp nhất.

- `id`: UUID ổn định của câu hỏi; các lần xuất hiện cùng câu có cùng UUID. UUID v5 được tạo lần đầu từ nội dung câu; những lần chạy sau giữ UUID hiện có, kể cả khi sửa nội dung. Xung đột UUID khi gộp được báo lỗi để duyệt.
- `legacyId`: số thứ tự cũ, chỉ duy nhất trong từng file nguồn.
- `slug`: slug nội bộ duy nhất, có UUID làm hậu tố, dùng cho cả free và Pro.
- `sourceSlug`, `sourceUrl`: dữ liệu định danh nguồn, cho phép null. Không suy diễn URL nguồn từ slug nội bộ.
- `departments`: mảng mã nhóm. Sửa `infracstruture` thành `infrastructure`, `carreer` thành `career`; các mã khác được thống nhất về dạng slug.
- `topics`: tên các chủ đề nguồn, chỉ có trong bản hợp nhất. Chủ đề khác với department.
- `sources`: giữ file, legacy ID, định danh nguồn và metadata của từng lần xuất hiện.
- `answerVariants`: giữ nguyên từng đáp án khác nhau và nguồn tương ứng.

## Quy tắc xử lý xung đột

8 nhóm có đáp án khác nhau được gộp về cùng câu hỏi nhưng giữ tất cả phiên bản. Các câu này có `needsReview: true` và `answer: null`; không chọn ngẫu nhiên hay nối các đoạn đáp án thành một câu trả lời. Danh sách nằm trong `normalized/report.json`. Trình import nên để những câu cần duyệt ở trạng thái draft.

`access` và `upgrade` là metadata từ website nguồn, không tự động quyết định chính sách trả phí của ứng dụng. Câu có đáp án rỗng chưa phải câu hỏi có lời giải hoàn chỉnh; trình import nên đánh dấu trạng thái thiếu đáp án. Slug nội bộ không bổ sung nội dung Pro còn thiếu.

## Gán department còn thiếu

| File | Department |
| --- | --- |
| ai.json | ai-ml |
| ba.json | business-analysis |
| backend.json | backend |
| database.json | data-and-messaging |
| devops-cloud.json | infrastructure |
| frontend.json | frontend |

## Tạo và kiểm tra

```sh
node luyenphongvan/normalize.mjs
node luyenphongvan/normalize.mjs --check
```

Lệnh đầu chuẩn hóa các file theo chủ đề và tái tạo bản hợp nhất. Lệnh `--check` chỉ đọc, kiểm tra cấu trúc, department, ID, slug, legacy ID và tính nhất quán giữa bản nguồn và bản hợp nhất. Chạy lệnh kiểm tra sau khi sửa dataset. Việc kiểm tra format không xác nhận tính chính xác chuyên môn của đáp án.

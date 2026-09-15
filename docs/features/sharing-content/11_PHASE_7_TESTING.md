# Phase 7 — Backend Testing

Ngày kiểm thử: 2026-09-15. Phạm vi: backend Sharing và regression backend Sprint 1–3; không bao gồm frontend/E2E.

## Kết quả

- Bổ sung 80 unit test case và 7 integration test case.
- Toàn bộ unit suite: **367 passed, 0 failed, 0 skipped**.
- Toàn bộ integration suite: **77 passed, 0 failed, 0 skipped**; những test hỗ trợ PostgreSQL đã chạy với PostgreSQL 18 thật, các fixture InMemory riêng của sprint cũ giữ nguyên.
- Build qua `dotnet test` thành công; `git diff --check` sạch trong phạm vi test/docs.
- TRX và coverage: `/private/tmp/sharing-phase7.Fs7SAk/results/` (artifact tạm ngoài Git). Unit TRX: `phase7-final_net10.0_20260915145707.trx`; integration TRX: `phase7-integration_net10.0_20260915145825.trx`.
- Database ứng dụng không bị tác động. Các database sinh bởi test đã được xóa; PostgreSQL tạm được dừng sau kiểm thử.

## Phạm vi kiểm thử

| Nhóm                       | Test và nội dung                                                                                                                                                                                                                                                                       |
| -------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Domain                     | `SharingDomainTests`: 30 tổ hợp Content transition, 20 tổ hợp Schedule transition; version không đổi khi transition không hợp lệ; Owner mặc định, review metadata, Published edit, terminal states, thời gian và delivery fields, Link/File và soft delete.                            |
| Validator                  | `SharingValidatorBoundaryTests`, `SharingRulesTests`: độ dài, enum, pagination, collection null/trùng/giới hạn, URL, DST, audience, quyền Contributor.                                                                                                                                 |
| Quyền và workflow          | `SharingEndpointsTests`: Draft riêng tư, Contributor chỉ sửa Rejected, Owner sửa Published, submit/withdraw/review, khóa resource PendingReview, quyền Admin và quản lý tag, self-assignment, PATCH/If-Match và rate limit.                                                            |
| Storage                    | `SharingResourceFailureTests` và `SharingPostgresTests`: lỗi ghi metadata sau upload/replace phải rollback version/audit, dọn file mới, giữ file cũ. Endpoint test kiểm tra download riêng tư, oversize, signature và soft delete. Storage implementation có bộ test riêng ở Sprint 3. |
| Audience và overlap        | Generation/Department đang hoạt động, phép OR giữa hai audience, kết thúc membership mất quyền, ẩn linked Draft, presenter/admin, overlap khác vai trò, thời gian liền kề, Cancelled giải phóng khoảng thời gian.                                                                      |
| Concurrency và persistence | PostgreSQL thật: hai publish lịch chồng nhau chỉ một commit (409 cho request thua); hai sửa cùng Content hoặc Schedule chỉ một commit (412 cho request thua); một audit Update; unique index từ chối Owner thứ hai.                                                                    |
| Regression                 | Chạy toàn bộ test Auth, Profile, Membership, Roadmap, migration/seeder và API cũ. Test nâng cấp từ schema Sprint 3 với graph có sẵn lên Sharing, chạy migration hai lần, giữ user/node/edge và tọa độ.                                                                                 |

## Cách chạy lại

Từ repository root, cấu hình `ROADMAP_TEST_POSTGRES` tới PostgreSQL **kiểm thử riêng**, với quyền tạo database. Test tự tạo database tên ngẫu nhiên và xóa sau khi chạy. Không dùng connection string production.

```sh
dotnet test backend/GdscSharingPlatform.slnx --no-restore -m:1 \
  -p:UseSharedCompilation=false --collect 'XPlat Code Coverage' \
  --logger 'trx;LogFilePrefix=phase7'
```

Nếu chưa restore dependencies, chạy `dotnet restore backend/GdscSharingPlatform.slnx` trước. Nếu không cấu hình PostgreSQL, một số test sẽ **skip** và endpoint tests dùng InMemory; lượt đó không đủ để xác nhận concurrency/rollback/persistence.

## Giới hạn và vấn đề còn theo dõi

- Bộ test pass không đồng nghĩa mọi interleaving hoặc boundary đều đã được kiểm thử.
- Ba finding từ review trước chưa được sửa trong Phase 7: detail/ETag không cùng snapshot khi có cập nhật đồng thời; creator lịch có thể thiếu MeetingUrl sau khi Admin đổi presenter; hơn 100 resource không reorder được. Chưa bổ sung regression tự động cho ba finding này; cần xử lý cùng thay đổi implementation tiếp theo.
- Không thay đổi source nghiệp vụ, migration, cấu hình ứng dụng hoặc các chỉnh sửa frontend/backend đang có của người dùng trong lượt testing này.
- Coverage được xuất theo từng test assembly; không cộng trực tiếp các phần trăm để suy ra coverage tổng.

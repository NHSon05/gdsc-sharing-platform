# Review Phase 4 — Next.js BFF

Ngày review: 2026-09-22. Phạm vi: các thay đổi chưa commit của luồng Google login,
session/cookie, BFF proxy và các thành phần frontend sử dụng session.

## Các vấn đề đã xác nhận và khắc phục

### P1 — Có thể vượt kiểm tra chặn auth bằng đường dẫn mã hóa

Proxy trước đây chỉ kiểm tra chuỗi `/api/auth` trên pathname chưa decode. Ví dụ
`/api/%61uth/login` không khớp điều kiện chặn; backend có thể decode thành endpoint
đăng nhập trả token. Đây là chỗ phá vỡ ranh giới BFF.

Đã sửa: `core/http/proxy-path.ts` chỉ cho phép các nhóm endpoint nghiệp vụ đã biết,
kiểm tra cả đường dẫn sau decode, từ chối separator mã hóa, nested encoding và dot
segments. Proxy không chuyển tiếp Authorization/Cookie do browser tự cung cấp,
không chuyển tiếp Set-Cookie từ backend. `backendFetch` cũng kiểm tra destination origin.

### P1 — Mất session từ API khác không xóa toàn bộ giao diện/cache

Trước đây chỉ hook `/me` xử lý xóa cache, còn interceptor chỉ xóa Zustand. Nếu
refresh thất bại khi gọi API roadmaps/profile, `/me` vẫn có dữ liệu cached và layout
có thể tiếp tục hiển thị nội dung cũ.

Đã sửa: `SessionBoundary` lắng nghe mất session từ mọi request, hủy query và xóa
cache. Layout/RoleGuard kiểm tra trạng thái session, không tiếp tục lấy user từ
query cũ. `/me` dừng tự fetch khi unauthenticated. Session revision ngăn phản hồi
của phiên cũ refresh/xóa phiên mới hoặc khôi phục user sau logout.

### P2 — Single-flight trong một tab chưa xử lý 401 đến muộn hoặc nhiều tab

Đã sửa: giữ single-flight promise trong tab, thêm Web Locks khi browser hỗ trợ,
kiểm tra lại `/me` bên trong lock trước khi rotate. BFF tiếp tục gộp request cùng
refresh token trong một process. 403 của endpoint nghiệp vụ không tự refresh;
401 sau lần retry không lặp lại. Refresh 5xx/network không xóa cookie còn có thể dùng.

### P2 — Mẫu cấu hình HTTP không phù hợp cookie OIDC Secure

Đã sửa `BACKEND_PUBLIC_ORIGIN` trong `.env.example`/README thành
`https://localhost:7160`; địa chỉ này phải khớp backend HTTPS profile và Google
Console. `.env.example` trước đây còn bị `.gitignore` bỏ qua, nay được cho phép
commit. Không thay đổi credential hoặc nội dung `.env` đang dùng.

### P2 — Login làm mất validation errors và chưa kiểm tra profile response

Đã sửa: giữ traceId và validation errors hợp lệ của email/password, không chuyển
tiếp chi tiết lỗi nội bộ. `parseUserProfile` validate và chọn riêng các trường
profile công khai cho login, `/me` và SSR; trường dư không bị chuyển vào browser.
Lỗi backend tạm thời khi login không chủ động xóa session cookie cũ.

### P2 — Thiếu kiểm thử cho các ranh giới bảo mật mới

Đã thêm `frontend/tests/auth-bff.test.mjs` dùng Node test runner, TypeScript có sẵn
và NextRequest/NextResponse thật, mock ở network boundary. Không thêm dependency.
Thêm backend tests cho handoff TTL, verifier, redeem đồng thời, replay, fixed
callback, start thiếu challenge và exchange endpoint.

## Luồng sau chỉnh sửa

1. Browser mở Next `/api/auth/google/start`, nhận cookie verifier HttpOnly.
2. Next redirect tới backend HTTPS, mang theo SHA256 challenge.
3. Google trả về callback OIDC của backend. Middleware kiểm tra state/nonce/PKCE
   và token; application service tìm/tạo tài khoản nội bộ như trước.
4. Backend chuyển về callback Next với handoff code 60 giây, dùng một lần.
5. Next đổi code + verifier qua server channel; ghi token vào cookie HttpOnly,
   xóa cookie tạm và redirect đến đường dẫn nội bộ đã kiểm tra.
6. Giao diện gọi `/me`; API khác đi qua BFF. Chỉ refresh route được rotate cookie,
   không rotate trong middleware hoặc Server Component.

## Kiểm chứng và giới hạn

- Frontend: `pnpm test:auth` đạt 12/12; `pnpm exec tsc --noEmit` và
  `pnpm build` thành công. `pnpm lint` không có lỗi, còn 2 cảnh báo `<img>`
  có sẵn ở `AdminReviewQueue.tsx`; build còn cảnh báo convention middleware cũ.
- Backend: auth integration tests (bao gồm handoff) đạt 29/29; toàn bộ unit
  tests đạt 367/367. Không chạy toàn bộ integration suite phụ thuộc hạ tầng khác.
- Các tests mock provider/network; chưa thay thế lần đăng nhập Google thật trên
  browser hoặc kiểm thử reverse proxy, mobile và nhiều tab trên môi trường deploy.
- Handoff store, attempt store và refresh coordination vẫn process-local. Chỉ
  chạy một instance mỗi dịch vụ; trước khi scale/serverless cần shared atomic
  store/coordination, không chỉ dùng cache thông thường.
- Khi server rotate thành công nhưng response bị mất, client không thể biết chắc
  trạng thái refresh. Cần thiết kế idempotency/session store bền vững nếu yêu cầu
  phục hồi trường hợp này, không tự retry token cũ vô hạn.
- Logout hiện bảo đảm xóa cookie tại browser, nhưng không bảo đảm remote revoke
  nếu backend không truy cập được. Logout-all chỉ báo thành công khi backend xác nhận.
- Rate limiting, redaction query/body tại proxy/log và CORS backend vẫn cần kiểm
  tra trong Phase 5 trước production. Không coi build/test qua là đã hoàn tất hardening.

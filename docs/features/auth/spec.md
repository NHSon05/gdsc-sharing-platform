# Feature Specification: Authentication and Authorization

**Feature Branch**: `001-authentication-authorization`  
**Created**: 2026-08-25  
**Status**: Draft  
**Input**: Xây dựng cơ chế đăng nhập, duy trì phiên, đăng xuất, xem tài khoản hiện tại và phân quyền Admin/Member cho GDSC Sharing Platform.

## User Scenarios & Testing

### User Story 1 — Đăng nhập vào hệ thống (Priority: P1)

Là một thành viên đã có tài khoản, tôi muốn đăng nhập bằng email và mật khẩu để sử dụng các chức năng dành cho người dùng đã xác thực.

**Why this priority**: Đây là điều kiện bắt buộc để người dùng truy cập mọi chức năng cá nhân và chức năng được phân quyền.

**Independent Test**: Có thể kiểm thử độc lập bằng một tài khoản đang hoạt động. Người dùng nhập đúng thông tin và nhận được phiên đăng nhập hợp lệ.

**Acceptance Scenarios**:

1. **Given** người dùng có tài khoản đang hoạt động, **When** nhập đúng email và mật khẩu, **Then** hệ thống xác thực thành công và cung cấp thông tin phiên đăng nhập.
2. **Given** người dùng nhập email không tồn tại, **When** gửi yêu cầu đăng nhập, **Then** hệ thống từ chối mà không tiết lộ email có tồn tại hay không.
3. **Given** người dùng nhập sai mật khẩu, **When** gửi yêu cầu đăng nhập, **Then** hệ thống từ chối với cùng loại thông báo như trường hợp email không tồn tại.
4. **Given** tài khoản không ở trạng thái hoạt động, **When** người dùng đăng nhập, **Then** hệ thống từ chối tạo phiên.
5. **Given** email có khoảng trắng ở đầu hoặc cuối, **When** người dùng đăng nhập bằng thông tin hợp lệ, **Then** hệ thống chuẩn hóa email và xử lý yêu cầu.
6. **Given** email hoặc mật khẩu bị bỏ trống, **When** gửi yêu cầu, **Then** hệ thống trả về lỗi validation cụ thể cho từng trường.
7. **Given** người dùng đăng nhập thành công, **When** xem kết quả, **Then** kết quả không chứa mật khẩu, mật khẩu đã mã hóa hoặc dữ liệu bảo mật nội bộ.

---

### User Story 2 — Truy cập chức năng yêu cầu đăng nhập (Priority: P1)

Là người dùng đã đăng nhập, tôi muốn hệ thống nhận biết danh tính của mình để truy cập các chức năng được bảo vệ mà không phải nhập lại mật khẩu trong mỗi yêu cầu.

**Why this priority**: Phiên đăng nhập chỉ có giá trị khi người dùng có thể sử dụng nó để truy cập tài nguyên được bảo vệ.

**Independent Test**: Đăng nhập, sử dụng thông tin phiên để truy cập một chức năng yêu cầu xác thực và xác nhận hệ thống nhận đúng người dùng.

**Acceptance Scenarios**:

1. **Given** người dùng có phiên hợp lệ, **When** truy cập chức năng yêu cầu đăng nhập, **Then** hệ thống cho phép truy cập.
2. **Given** người dùng không cung cấp thông tin phiên, **When** truy cập chức năng yêu cầu đăng nhập, **Then** hệ thống từ chối với trạng thái chưa xác thực.
3. **Given** thông tin phiên không hợp lệ, **When** truy cập chức năng được bảo vệ, **Then** hệ thống từ chối.
4. **Given** phiên đã hết hạn, **When** truy cập chức năng được bảo vệ, **Then** hệ thống từ chối và không tự động kéo dài phiên.
5. **Given** thông tin phiên đã bị thay đổi, **When** gửi yêu cầu, **Then** hệ thống phát hiện và từ chối.
6. **Given** người dùng đã bị vô hiệu hóa sau khi đăng nhập, **When** hệ thống kiểm tra trạng thái hiện tại của người dùng, **Then** quyền truy cập bị từ chối theo chính sách bảo mật.

---

### User Story 3 — Làm mới phiên đăng nhập (Priority: P1)

Là người dùng đã đăng nhập, tôi muốn duy trì phiên sử dụng mà không phải nhập lại mật khẩu mỗi khi thông tin truy cập ngắn hạn hết hạn.

**Why this priority**: Việc duy trì phiên giúp cân bằng trải nghiệm người dùng và yêu cầu bảo mật.

**Independent Test**: Đăng nhập, sử dụng thông tin làm mới hợp lệ để nhận phiên mới, sau đó xác nhận thông tin làm mới cũ không còn sử dụng được.

**Acceptance Scenarios**:

1. **Given** người dùng có thông tin làm mới phiên hợp lệ, **When** yêu cầu làm mới, **Then** hệ thống cấp thông tin truy cập mới và thông tin làm mới mới.
2. **Given** phiên được làm mới thành công, **When** sử dụng lại thông tin làm mới cũ, **Then** hệ thống từ chối.
3. **Given** thông tin làm mới đã hết hạn, **When** yêu cầu làm mới, **Then** hệ thống từ chối và yêu cầu đăng nhập lại.
4. **Given** thông tin làm mới đã bị thu hồi, **When** yêu cầu làm mới, **Then** hệ thống từ chối.
5. **Given** thông tin làm mới không tồn tại hoặc bị sửa đổi, **When** yêu cầu làm mới, **Then** hệ thống từ chối mà không tiết lộ dữ liệu nội bộ.
6. **Given** tài khoản đã bị vô hiệu hóa, **When** người dùng cố làm mới phiên, **Then** hệ thống từ chối.
7. **Given** thông tin làm mới cũ đã được thay thế nhưng tiếp tục được sử dụng, **When** hệ thống phát hiện hành vi tái sử dụng, **Then** hệ thống thu hồi các phiên liên quan và yêu cầu đăng nhập lại.

---

### User Story 4 — Xem thông tin tài khoản hiện tại (Priority: P1)

Là người dùng đã đăng nhập, tôi muốn xem thông tin tài khoản hiện tại để xác nhận danh tính, phòng ban, trạng thái và quyền của mình.

**Why this priority**: Frontend cần biết người dùng hiện tại là ai để hiển thị đúng giao diện và chức năng.

**Independent Test**: Đăng nhập và yêu cầu thông tin người dùng hiện tại; kết quả phải khớp với tài khoản đã đăng nhập.

**Acceptance Scenarios**:

1. **Given** người dùng có phiên hợp lệ, **When** yêu cầu xem tài khoản hiện tại, **Then** hệ thống trả về đúng thông tin của người dùng đó.
2. **Given** người dùng thuộc một Department, **When** xem tài khoản, **Then** kết quả chứa thông tin Department tương ứng.
3. **Given** người dùng chưa thuộc Department, **When** xem tài khoản, **Then** hệ thống vẫn trả kết quả hợp lệ với Department để trống.
4. **Given** người dùng có nhiều role, **When** xem tài khoản, **Then** hệ thống trả về đầy đủ các role.
5. **Given** người dùng chưa đăng nhập, **When** yêu cầu xem tài khoản hiện tại, **Then** hệ thống từ chối.
6. **Given** người dùng đã đăng nhập, **When** xem tài khoản, **Then** kết quả không chứa thông tin xác thực nhạy cảm.

---

### User Story 5 — Đăng xuất phiên hiện tại (Priority: P1)

Là người dùng đã đăng nhập, tôi muốn đăng xuất khỏi phiên hiện tại để phiên đó không thể tiếp tục được gia hạn.

**Why this priority**: Người dùng phải có khả năng chủ động kết thúc phiên, đặc biệt khi sử dụng thiết bị dùng chung.

**Independent Test**: Đăng nhập, đăng xuất, sau đó thử làm mới phiên bằng thông tin cũ và xác nhận yêu cầu bị từ chối.

**Acceptance Scenarios**:

1. **Given** người dùng có phiên hợp lệ, **When** đăng xuất, **Then** thông tin làm mới của phiên hiện tại bị thu hồi.
2. **Given** người dùng đã đăng xuất, **When** sử dụng lại thông tin làm mới của phiên đó, **Then** hệ thống từ chối.
3. **Given** phiên đã được đăng xuất trước đó, **When** người dùng gửi lại yêu cầu đăng xuất, **Then** hệ thống xử lý an toàn và không tạo lỗi dữ liệu.
4. **Given** người dùng có nhiều phiên trên nhiều thiết bị, **When** đăng xuất một phiên, **Then** các phiên còn lại không bị ảnh hưởng.
5. **Given** người dùng cố đăng xuất bằng thông tin phiên thuộc tài khoản khác, **When** gửi yêu cầu, **Then** hệ thống không thu hồi phiên của tài khoản khác.

---

### User Story 6 — Đăng xuất khỏi tất cả thiết bị (Priority: P2)

Là người dùng đã đăng nhập, tôi muốn kết thúc tất cả phiên của mình để bảo vệ tài khoản khi nghi ngờ bị truy cập trái phép.

**Why this priority**: Đây là cơ chế bảo vệ tài khoản quan trọng nhưng không chặn luồng đăng nhập cơ bản.

**Independent Test**: Tạo ít nhất hai phiên cho cùng một tài khoản, thực hiện đăng xuất tất cả và xác nhận cả hai phiên không thể làm mới.

**Acceptance Scenarios**:

1. **Given** người dùng có nhiều phiên đang hoạt động, **When** chọn đăng xuất tất cả thiết bị, **Then** toàn bộ thông tin làm mới của người dùng bị thu hồi.
2. **Given** người dùng đã đăng xuất tất cả, **When** một phiên cũ yêu cầu làm mới, **Then** hệ thống từ chối.
3. **Given** người dùng khác vẫn có phiên hoạt động, **When** người dùng hiện tại đăng xuất tất cả, **Then** phiên của người dùng khác không bị ảnh hưởng.
4. **Given** người dùng không có phiên làm mới nào khác, **When** đăng xuất tất cả, **Then** yêu cầu vẫn hoàn thành an toàn.

---

### User Story 7 — Truy cập theo vai trò Member (Priority: P2)

Là một Member, tôi muốn truy cập các chức năng dành cho thành viên để sử dụng các tính năng cộng đồng.

**Why this priority**: Phân quyền Member là nền tảng cho Roadmap, Sharing Content và Social Interaction ở các sprint sau.

**Independent Test**: Đăng nhập bằng tài khoản Member và truy cập một chức năng được giới hạn cho thành viên.

**Acceptance Scenarios**:

1. **Given** người dùng có role `Member`, **When** truy cập chức năng dành cho Member, **Then** hệ thống cho phép.
2. **Given** người dùng chưa đăng nhập, **When** truy cập chức năng dành cho Member, **Then** hệ thống trả về trạng thái chưa xác thực.
3. **Given** người dùng đã đăng nhập nhưng không có role phù hợp, **When** truy cập chức năng dành cho Member, **Then** hệ thống trả về trạng thái không đủ quyền.
4. **Given** người dùng có role `Admin`, **When** truy cập chức năng chung dành cho thành viên, **Then** hệ thống cho phép theo chính sách quyền của dự án.

---

### User Story 8 — Truy cập theo vai trò Admin (Priority: P2)

Là một Admin, tôi muốn truy cập các chức năng quản trị để thực hiện nhiệm vụ quản lý hệ thống.

**Why this priority**: Hệ thống đã có tài khoản Admin được seed và cần cơ chế bảo vệ chức năng quản trị trước khi phát triển các module quản lý.

**Independent Test**: So sánh quyền truy cập cùng một chức năng quản trị giữa Admin, Member và người chưa đăng nhập.

**Acceptance Scenarios**:

1. **Given** người dùng có role `Admin`, **When** truy cập chức năng quản trị, **Then** hệ thống cho phép.
2. **Given** người dùng chỉ có role `Member`, **When** truy cập chức năng quản trị, **Then** hệ thống từ chối do không đủ quyền.
3. **Given** người dùng chưa đăng nhập, **When** truy cập chức năng quản trị, **Then** hệ thống từ chối do chưa xác thực.
4. **Given** role của người dùng đã bị thay đổi, **When** một phiên mới được tạo hoặc làm mới, **Then** quyền trong phiên mới phản ánh role hiện tại.

## Edge Cases

- Người dùng nhập email bằng chữ hoa, chữ thường hoặc có khoảng trắng ở đầu/cuối.
- Người dùng nhập mật khẩu có khoảng trắng hợp lệ; hệ thống không được tự ý trim mật khẩu.
- Hai yêu cầu đăng nhập được gửi gần như đồng thời.
- Hai yêu cầu làm mới sử dụng cùng một thông tin làm mới tại cùng thời điểm.
- Thông tin làm mới hết hạn đúng lúc yêu cầu đang được xử lý.
- Người dùng đăng xuất trong khi một yêu cầu làm mới khác đang được xử lý.
- Người dùng bị vô hiệu hóa sau khi đã đăng nhập.
- Người dùng bị soft delete sau khi đã đăng nhập.
- Role của người dùng thay đổi khi họ đang có phiên hoạt động.
- Department của người dùng đã bị xóa hoặc không còn hoạt động.
- Dữ liệu phiên cũ tham chiếu đến người dùng không còn tồn tại.
- Hệ thống nhận thông tin truy cập có cấu trúc không hợp lệ.
- Hệ thống nhận chuỗi làm mới quá dài hoặc rỗng.
- Người dùng gửi thông tin làm mới của tài khoản khác khi đăng xuất.
- Đồng hồ hệ thống có sai lệch nhỏ giữa thời điểm phát hành và xác minh.
- Người dùng đăng xuất một phiên nhưng vẫn còn phiên hợp lệ trên thiết bị khác.
- Hệ thống khởi động khi thiếu cấu hình bảo mật bắt buộc.
- Nhiều lần đăng nhập thất bại liên tiếp kích hoạt quy tắc khóa tài khoản hiện có.

## Requirements

### Functional Requirements

- **FR-001**: Hệ thống MUST cho phép người dùng có tài khoản hợp lệ đăng nhập bằng email và mật khẩu.
- **FR-002**: Hệ thống MUST xác thực email theo cách không phân biệt chữ hoa và chữ thường.
- **FR-003**: Hệ thống MUST giữ nguyên nội dung mật khẩu khi xác thực và không tự động trim mật khẩu.
- **FR-004**: Hệ thống MUST trả cùng một loại thông báo khi email không tồn tại hoặc mật khẩu không chính xác.
- **FR-005**: Hệ thống MUST không cho phép tài khoản không hoạt động, bị đình chỉ hoặc đã bị xóa đăng nhập.
- **FR-006**: Hệ thống MUST tạo phiên đăng nhập khi thông tin xác thực hợp lệ.
- **FR-007**: Hệ thống MUST cung cấp thông tin truy cập ngắn hạn cho phiên đăng nhập thành công.
- **FR-008**: Hệ thống MUST cung cấp thông tin làm mới phiên có thời gian sống dài hơn thông tin truy cập ngắn hạn.
- **FR-009**: Hệ thống MUST không lưu giá trị làm mới phiên ở dạng có thể sử dụng trực tiếp nếu dữ liệu lưu trữ bị lộ.
- **FR-010**: Hệ thống MUST gắn mỗi phiên làm mới với đúng một người dùng.
- **FR-011**: Hệ thống MUST lưu thời điểm tạo và thời điểm hết hạn của phiên làm mới.
- **FR-012**: Hệ thống MUST cho phép một người dùng có nhiều phiên độc lập trên nhiều thiết bị.
- **FR-013**: Hệ thống MUST cho phép người dùng sử dụng thông tin làm mới hợp lệ để nhận phiên mới mà không nhập lại mật khẩu.
- **FR-014**: Hệ thống MUST thay thế thông tin làm mới cũ bằng thông tin làm mới mới sau mỗi lần làm mới thành công.
- **FR-015**: Hệ thống MUST vô hiệu hóa thông tin làm mới cũ ngay sau khi nó được thay thế.
- **FR-016**: Hệ thống MUST từ chối thông tin làm mới đã hết hạn, bị thu hồi, bị thay thế hoặc không tồn tại.
- **FR-017**: Hệ thống MUST phát hiện việc tái sử dụng thông tin làm mới đã được thay thế.
- **FR-018**: Khi phát hiện tái sử dụng thông tin làm mới, hệ thống MUST thu hồi các phiên liên quan và yêu cầu người dùng đăng nhập lại.
- **FR-019**: Hệ thống MUST kiểm tra trạng thái hiện tại của tài khoản trước khi làm mới phiên.
- **FR-020**: Hệ thống MUST cho phép người dùng đăng xuất khỏi phiên hiện tại.
- **FR-021**: Đăng xuất phiên hiện tại MUST chỉ thu hồi phiên thuộc đúng người dùng và đúng thông tin phiên được cung cấp.
- **FR-022**: Yêu cầu đăng xuất lặp lại MUST không tạo bản ghi trùng hoặc lỗi dữ liệu.
- **FR-023**: Hệ thống MUST cho phép người dùng đăng xuất khỏi tất cả thiết bị.
- **FR-024**: Đăng xuất tất cả MUST thu hồi toàn bộ phiên làm mới còn hoạt động của người dùng hiện tại.
- **FR-025**: Đăng xuất tất cả của một người dùng MUST không ảnh hưởng đến phiên của người dùng khác.
- **FR-026**: Hệ thống MUST cho phép người dùng đã đăng nhập xem thông tin tài khoản hiện tại.
- **FR-027**: Thông tin tài khoản hiện tại MUST bao gồm định danh, email, tên hiển thị, trạng thái, Department nếu có và danh sách role.
- **FR-028**: Thông tin tài khoản trả về MUST không chứa mật khẩu, dữ liệu xác thực bí mật hoặc dữ liệu phiên nội bộ.
- **FR-029**: Hệ thống MUST từ chối yêu cầu xem tài khoản hiện tại nếu người dùng chưa được xác thực.
- **FR-030**: Hệ thống MUST nhận diện role `Admin` và `Member`.
- **FR-031**: Hệ thống MUST cho phép Member truy cập chức năng dành cho Member.
- **FR-032**: Hệ thống MUST cho phép Admin truy cập chức năng dành cho Admin.
- **FR-033**: Hệ thống MUST từ chối Member truy cập chức năng chỉ dành cho Admin.
- **FR-034**: Hệ thống MUST phân biệt rõ trường hợp chưa xác thực và trường hợp đã xác thực nhưng không đủ quyền.
- **FR-035**: Hệ thống MUST phản ánh role hiện tại của người dùng khi tạo hoặc làm mới phiên.
- **FR-036**: Hệ thống MUST từ chối thông tin truy cập hết hạn, bị sửa đổi hoặc không được hệ thống tin cậy.
- **FR-037**: Hệ thống MUST không kéo dài thời gian sống của thông tin truy cập ngắn hạn chỉ vì nó được sử dụng.
- **FR-038**: Hệ thống MUST trả lỗi validation cho email, mật khẩu hoặc thông tin làm mới bị thiếu hoặc sai định dạng.
- **FR-039**: Hệ thống MUST sử dụng định dạng lỗi nhất quán và cung cấp mã truy vết cho lỗi authentication và authorization.
- **FR-040**: Hệ thống MUST ghi nhận các sự kiện đăng nhập thành công, đăng nhập thất bại, làm mới, đăng xuất và phát hiện tái sử dụng phiên.
- **FR-041**: Hệ thống MUST không ghi mật khẩu, thông tin truy cập đầy đủ hoặc thông tin làm mới đầy đủ vào log.
- **FR-042**: Hệ thống MUST từ chối khởi động nếu thiếu cấu hình bảo mật bắt buộc.
- **FR-043**: Hệ thống MUST giữ nguyên tài khoản Admin, role và Department đã có từ Sprint 0.
- **FR-044**: Thay đổi dữ liệu Sprint 1 MUST không làm mất hoặc tạo trùng dữ liệu Identity đã tồn tại.
- **FR-045**: Hệ thống MUST cung cấp ít nhất một cách kiểm chứng độc lập quyền Member và quyền Admin trước khi các module nghiệp vụ được phát triển.
- **FR-046**: Hệ thống MUST giới hạn chiều dài dữ liệu authentication đầu vào để tránh payload bất thường.
- **FR-047**: Hệ thống MUST xử lý an toàn khi hai yêu cầu làm mới cùng sử dụng một thông tin phiên.
- **FR-048**: Chỉ một yêu cầu làm mới đồng thời cho cùng một phiên MUST được chấp nhận; yêu cầu còn lại MUST bị từ chối.
- **FR-049**: Hệ thống MUST sử dụng thời gian UTC để xác định thời điểm tạo, hết hạn và thu hồi phiên.
- **FR-050**: Hệ thống MUST cho phép kiểm thử toàn bộ authentication flow mà không thay đổi dữ liệu development hoặc production.

### Security Requirements

- **SR-001**: Dữ liệu bí mật dùng để xác thực phiên MUST được cung cấp từ cấu hình bảo mật bên ngoài source code.
- **SR-002**: Thông tin làm mới phiên MUST có mức ngẫu nhiên đủ để không thể dự đoán trong thực tế.
- **SR-003**: Dữ liệu lưu trữ của thông tin làm mới MUST không thể được dùng trực tiếp để chiếm phiên nếu database bị đọc trái phép.
- **SR-004**: Thông tin truy cập ngắn hạn MUST có thời gian sống ngắn hơn thông tin làm mới.
- **SR-005**: Mỗi thông tin truy cập MUST có định danh duy nhất và thời điểm hết hạn.
- **SR-006**: Hệ thống MUST kiểm tra nguồn phát hành và đối tượng sử dụng của thông tin truy cập.
- **SR-007**: Hệ thống MUST từ chối thông tin truy cập có chữ ký hoặc nội dung không hợp lệ.
- **SR-008**: Hệ thống MUST không tiết lộ tài khoản có tồn tại hay không thông qua lỗi đăng nhập.
- **SR-009**: Hệ thống MUST không trả dữ liệu xác thực nội bộ trong response.
- **SR-010**: Các sự kiện bảo mật MUST có mã truy vết để hỗ trợ điều tra mà không làm lộ secret.
- **SR-011**: Việc thu hồi phiên MUST có hiệu lực đối với lần làm mới tiếp theo.
- **SR-012**: Hành vi tái sử dụng phiên đã bị thay thế MUST được xem là sự kiện bảo mật.

## Key Entities

### User

Đại diện cho thành viên của nền tảng.

Thông tin liên quan đến Sprint 1:

- Định danh người dùng.
- Email.
- Tên hiển thị.
- Trạng thái tài khoản.
- Department.
- Role.
- Phiên bản xác thực hiện tại.
- Các phiên đăng nhập.

### Role

Đại diện cho nhóm quyền của người dùng.

Các role thuộc phạm vi:

- `Admin`
- `Member`

Một người dùng có thể có nhiều role.

### Authentication Session

Đại diện cho một lần đăng nhập trên một thiết bị hoặc trình duyệt.

Thông tin chính:

- Người dùng sở hữu phiên.
- Thời điểm tạo.
- Thời điểm hết hạn.
- Thiết bị hoặc trình duyệt nếu có.
- Địa chỉ mạng nếu có.
- Trạng thái hoạt động hoặc đã thu hồi.
- Lý do thu hồi.
- Phiên thay thế nếu phiên đã được làm mới.

### Access Credential

Thông tin truy cập có thời gian sống ngắn, dùng để chứng minh danh tính và quyền của người dùng trong các yêu cầu được bảo vệ.

Thông tin chính:

- Người dùng.
- Role.
- Thời điểm tạo.
- Thời điểm hết hạn.
- Định danh duy nhất.
- Phiên bản xác thực.

### Renewal Credential

Thông tin có thời gian sống dài hơn, dùng để tạo phiên truy cập mới mà không yêu cầu nhập lại mật khẩu.

Thông tin chính:

- Người dùng sở hữu.
- Thời điểm tạo.
- Thời điểm hết hạn.
- Trạng thái thu hồi.
- Phiên thay thế.
- Thông tin phục vụ phát hiện tái sử dụng.

## Assumptions

- Sprint 0 đã tạo thành công tài khoản Admin mặc định.
- Role `Admin` và `Member` đã tồn tại trước Sprint 1.
- Tài khoản người dùng được tạo trước khi đăng nhập.
- Public registration không thuộc Sprint 1.
- Người dùng đăng nhập bằng email và mật khẩu.
- Một người dùng có thể đăng nhập trên nhiều thiết bị.
- Admin được phép sử dụng các chức năng chung dành cho thành viên.
- Việc thay đổi role có hiệu lực chậm nhất ở lần tạo hoặc làm mới phiên tiếp theo.
- Thông tin truy cập ngắn hạn có thời gian sống mặc định 15 phút.
- Thông tin làm mới phiên có thời gian sống mặc định 7 ngày.
- Đăng xuất một thiết bị không đăng xuất các thiết bị khác.
- Đăng xuất tất cả sẽ thu hồi tất cả phiên làm mới của người dùng.
- Thông tin truy cập ngắn hạn đã phát hành có thể tồn tại đến khi hết hạn; thời gian sống ngắn giới hạn rủi ro.
- Tất cả thời gian bảo mật được tính theo UTC.
- Hệ thống sử dụng chính sách khóa đăng nhập đã được cấu hình trong Sprint 0.
- Các thông báo lỗi hiển thị bằng tiếng Anh nhất quán với API hiện tại; giao diện người dùng có thể dịch ở frontend.
- Sprint 1 chưa yêu cầu giao diện đăng nhập hoàn chỉnh trên frontend; trọng tâm là khả năng authentication và authorization của hệ thống.

## Dependencies

- Sprint 0 đã hoàn thành.
- Có database Identity và Department.
- Có tài khoản Admin được seed.
- Có role `Admin` và `Member`.
- Có cơ chế quản lý configuration và secret.
- Có global exception handling.
- Có Problem Details và `traceId`.
- Có Docker Compose.
- Có Unit Test và Integration Test foundation.
- Có môi trường kiểm thử database cô lập.

## Out of Scope

- Đăng ký tài khoản công khai.
- Đăng nhập qua Google, GitHub hoặc nhà cung cấp bên ngoài.
- Quên mật khẩu.
- Đặt lại mật khẩu.
- Gửi email xác nhận tài khoản.
- Xác thực hai bước.
- Sinh trắc học.
- Quản trị toàn bộ tài khoản người dùng.
- Gán hoặc thu hồi role của người dùng khác.
- Permission động.
- Dashboard.
- Roadmap.
- Sharing Content.
- Sharing Schedule.
- Social Interaction.
- Notification.
- AI Assistant.
- Giao diện frontend hoàn chỉnh cho authentication.
- Hệ thống email production.
- Single Sign-On.
- Quản lý phiên qua trang quản trị.
- Báo cáo bảo mật nâng cao.

## Success Criteria

### Measurable Outcomes

- **SC-001**: 100% người dùng có tài khoản hoạt động và nhập đúng thông tin có thể hoàn thành đăng nhập.
- **SC-002**: Ít nhất 95% yêu cầu đăng nhập hợp lệ hoàn thành trong vòng 2 giây ở môi trường phát triển tiêu chuẩn.
- **SC-003**: 100% trường hợp sai email và sai mật khẩu trả về cùng một loại thông báo, không tiết lộ tài khoản có tồn tại hay không.
- **SC-004**: 100% người dùng chưa đăng nhập bị từ chối khi truy cập chức năng được bảo vệ.
- **SC-005**: 100% Member bị từ chối khi truy cập chức năng chỉ dành cho Admin.
- **SC-006**: 100% Admin có phiên hợp lệ truy cập được chức năng dành cho Admin.
- **SC-007**: 100% yêu cầu làm mới hợp lệ tạo được phiên mới mà không yêu cầu người dùng nhập lại mật khẩu.
- **SC-008**: 100% thông tin làm mới cũ bị vô hiệu hóa ngay sau khi rotation thành công.
- **SC-009**: 100% lần tái sử dụng thông tin làm mới đã bị thay thế được phát hiện và bị từ chối.
- **SC-010**: Sau khi đăng xuất, thông tin làm mới của phiên đó bị từ chối ở lần sử dụng tiếp theo.
- **SC-011**: Sau khi đăng xuất tất cả thiết bị, 100% phiên làm mới hiện có của người dùng bị từ chối.
- **SC-012**: Đăng xuất một phiên không làm gián đoạn các phiên hợp lệ khác của cùng người dùng.
- **SC-013**: 100% response thông tin tài khoản không chứa mật khẩu, dữ liệu xác thực bí mật hoặc dữ liệu phiên nội bộ.
- **SC-014**: 100% lỗi authentication và authorization có mã truy vết.
- **SC-015**: Không có password, thông tin truy cập đầy đủ hoặc thông tin làm mới đầy đủ xuất hiện trong log kiểm thử.
- **SC-016**: 100% kịch bản P1 có thể kiểm thử độc lập và vượt qua acceptance test.
- **SC-017**: Tất cả kiểm thử tự động của Sprint 0 vẫn pass sau khi Sprint 1 hoàn thành.
- **SC-018**: Tất cả kiểm thử authentication, session renewal, logout và role authorization đều pass.
- **SC-019**: Việc nâng cấp dữ liệu Sprint 1 giữ nguyên toàn bộ User, Role, UserRole và Department đã tồn tại.
- **SC-020**: Một lập trình viên mới có thể cấu hình, chạy và kiểm tra authentication flow trong không quá 15 phút bằng tài liệu dự án.

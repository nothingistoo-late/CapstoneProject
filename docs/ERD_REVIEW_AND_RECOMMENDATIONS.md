# Nhận xét ERD QuackOrbit & Đề xuất chỉnh sửa

Tài liệu này đối chiếu ERD bạn gửi với **Overview.md** (QuackOrbit) và **base Clean Architecture** (CapstoneProject), đưa ra nhận xét và đề xuất chỉnh sửa để đồng bộ naming, audit, và chuẩn dự án.

---

## 1. Đối chiếu với Overview.md (QuackOrbit)

### 1.1 Các tính năng trong Overview vs ERD

| Tính năng (Overview) | Entity/ERD tương ứng | Ghi chú |
|----------------------|----------------------|--------|
| Trình chỉnh sửa kéo-thả & Game 2D | Maps, MapsDetail, Submissions, ExecutionsResult | Đủ: map, spec chạy, submission, execution |
| Challenge Mode (chơi đơn) | Maps, Hints, Constraints, UserMapResult, XpTransactions | Đủ: map, gợi ý, kết quả, XP |
| Competitive Mode (2–8 người) | Matches, Rooms, RoomParticipants, UserMatchResult | Đúng: phòng, tham gia, kết quả match |
| UGC Marketplace (tạo thử thách, duyệt, xuất bản) | Maps (CreatedByUserId, IsPublished) | Thiếu: **trạng thái duyệt** (Pending/Approved/Rejected) và **Package/giá** gắn với map nếu bán nội dung |
| Huy hiệu, XP, sao | Achievements, UserAchievements, XpTransactions, UserMapResult (BestStars) | Đủ |
| Gói trả phí / thanh toán | Packages, UserPackage, PaymentRecord, Payment | Cần nối **Payment** với **PaymentRecord** (xem mục 4) |

Kết luận: ERD phủ đủ phần lớn yêu cầu; cần bổ sung/điều chỉnh: trạng thái duyệt map (UGC), liên kết Payment, và làm rõ Map vs MapDetail vs MapsDetail.

---

## 2. Đồng bộ với Base Clean Architecture

Base project dùng:

- **Primary key:** `Guid Id`
- **Audit:** `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`
- **Soft delete:** `IsDeleted`, `DeletedAt`, `DeletedBy` (kế thừa từ `BaseEntity`)
- **Status:** `EntityStatusEnum` (Active, Inactive, Pending, Rejected)
- **Identity:** `AppUser`, `AppRole` (bảng `Users`, `Roles`) — không tạo lại bảng Users/Roles riêng; mở rộng qua entity kế thừa hoặc bảng bổ sung nếu cần.

Đề xuất áp dụng thống nhất cho toàn bộ bảng mới của QuackOrbit:

- Mọi bảng có **PK = Id (Guid)**.
- Bảng là aggregate root hoặc entity chính: kế thừa **BaseEntity** (có đủ audit + soft delete + Status).
- Bảng junction hoặc bảng phụ: ít nhất có **CreatedAt** (và CreatedBy nếu cần trace).
- Trường trạng thái dùng **enum** (lưu DB dạng int), đặt tên rõ (ví dụ `MapStatusEnum`, `SubmissionStatusEnum`).

---

## 3. Các vấn đề chính trong ERD hiện tại

### 3.1 Maps vs MapDetail (singular) vs MapsDetail (plural)

- **MapDetail (singular):** có các cột giống Maps (Title, Description, Difficulty, TimeLimitMs, IsPublished, CreatedAt) → trùng lặp với Maps, dễ gây nhầm lẫn.
- **MapsDetail (plural):** chứa spec thực thi (GridSpec, InitialStateSpec, WinConditionSpec, FailConditionSpec, Status, CreatedAt) → đây mới là “chi tiết kỹ thuật” của map.

Đề xuất:

1. **Bỏ bảng MapDetail (singular)** — không dùng bảng trùng nghĩa với Maps.
2. **Giữ một bảng “chi tiết map”** đặt tên rõ, ví dụ **MapSpec** hoặc **MapVersion** (nếu mỗi map có nhiều phiên bản spec):
   - **MapSpec (hoặc MapVersion):**  
     `Id`, `MapId`, `GridSpec`, `InitialStateSpec`, `WinConditionSpec`, `FailConditionSpec`, `Status`, `Version`, `CreatedAt`, `CreatedBy`.
   - Quan hệ: **Map 1 – N MapSpec** (một map có nhiều bản spec/version).
3. Trả lời câu hỏi *“Khi update các màn của Maps sẽ như thế nào”*:
   - Nếu **một map chỉ có một spec hiện hành:** dùng 1–1 với MapSpec; khi “update màn” = tạo bản MapSpec mới (version++) hoặc update bản hiện tại (tùy nghiệp vụ).
   - Nếu **cần lịch sử version:** MapSpec có Version, Map có thể có `CurrentMapSpecId` (FK) trỏ tới bản đang dùng.

Áp dụng chuẩn base: MapSpec kế thừa BaseEntity (Id, audit, Status, soft delete).

### 3.2 Bảng Payment (đơn lẻ)

- **Payment** có `PaymentId`, `Description`, `PaymentType` nhưng **không được nối** với PaymentRecord.
- Hậu quả: không biết giao dịch thanh toán thuộc loại nào, mô tả thế nào.

Đề xuất:

- **Cách 1 (đơn giản):**  
  - **PaymentRecord** thêm: `PaymentType` (string hoặc enum), `Description` (string, nullable).  
  - Bỏ bảng **Payment** nếu chỉ dùng như lookup đơn giản.
- **Cách 2 (chuẩn hóa):**  
  - **Payment** giữ làm bảng tra cứu: `Id`, `Code`, `Name`, `Description` (ví dụ: VNPay, Momo, Cash).  
  - **PaymentRecord** thêm FK: `PaymentId` → Payment.  
  - Khi tạo PaymentRecord thì gán PaymentId tương ứng.

Chọn một trong hai và áp dụng thống nhất; nếu giữ Payment thì bắt buộc nối PaymentRecord → Payment.

### 3.3 Đặt tên PK và bảng

- **XpTransactions:** PK đang là `LevelId` → dễ hiểu nhầm là “level”.  
  - Đề xuất: đổi PK thành **Id** (Guid), đặt tên bảng rõ ràng (ví dụ **XpTransaction** số ít cho gần với convention).
- **UserPackage:** PK đang là `Id` (chung chung).  
  - Đề xuất: đổi thành **UserPackageId** (Guid) cho thống nhất với các bảng khác.

---

## 4. Đề xuất chỉnh sửa cụ thể (checklist)

### 4.1 Naming & cấu trúc chung

- [ ] Tất cả bảng: **PK = Id (Guid)**.
- [ ] Bảng chính (Map, Match, Room, Package, Achievement, …): kế thừa **BaseEntity** (Id, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted, DeletedAt, DeletedBy, Status).
- [ ] Bảng junction / bảng phụ: có ít nhất **Id**, **CreatedAt**; nếu cần audit đầy đủ thì dùng BaseEntity.
- [ ] **Users / Roles:** dùng Identity sẵn có (AppUser, AppRole). Nếu ERD vẽ “Users”, coi như map tới bảng Identity; không tạo bảng Users mới trùng.

### 4.2 Map & chi tiết map

- [ ] **Bỏ** bảng **MapDetail** (singular) trùng với Maps.
- [ ] **Đổi tên / tách rõ** bảng chi tiết spec (hiện MapsDetail):
  - Tên đề xuất: **MapSpec** hoặc **MapVersion**.
  - Cột: `Id`, `MapId`, `GridSpec`, `InitialStateSpec`, `WinConditionSpec`, `FailConditionSpec`, `Status`, `Version` (optional), `CreatedAt`, `CreatedBy` (và có thể full BaseEntity).
- [ ] **Maps:** thống nhất tên entity là **Map** (số ít); bảng DB có thể là `Maps`.  
  - Thêm **Status** (enum) cho UGC: Draft, PendingReview, Approved, Rejected, Published (hoặc tương đương) để hỗ trợ quy trình duyệt nội dung.

### 4.3 Payment

- [ ] **Hoặc** bỏ bảng Payment và đưa `PaymentType` + `Description` vào **PaymentRecord**.
- [ ] **Hoặc** giữ **Payment** và thêm vào **PaymentRecord** cột **PaymentId** (FK → Payment).

### 4.4 Các bảng còn lại (nhanh)

- [ ] **XpTransactions:** đổi tên PK từ `LevelId` → **Id**; tên bảng có thể giữ **XpTransactions** hoặc **XpTransaction**.
- [ ] **UserPackage:** đổi tên PK từ `Id` → **UserPackageId** (Guid).
- [ ] **Submissions.Language:** xác định kiểu (string/enum), bỏ “???”; ví dụ enum `SubmissionLanguageEnum` (Blockly, …).
- [ ] Các trường **Status** trên mọi bảng: chuẩn hóa thành enum (MapStatusEnum, SubmissionStatusEnum, RoomStatusEnum, …) và lưu int trong DB.

### 4.5 Spec / Value Object (Domain layer)

- Các trường *Spec (GridSpec, WinConditionSpec, AstSpec, RuleSpec, …) trong ERD có thể giữ dạng string/JSON ở DB, nhưng trong **Domain** nên bọc thành **Value Object** (hoặc type riêng) để:
  - Validate format.
  - Parse/serialize thống nhất.
  - Dễ test và tách biệt domain logic khỏi persistence.

---

## 5. Sơ đồ quan hệ sau chỉnh sửa (tóm tắt)

- **Identity:** AppUser (Users), AppRole (Roles) — không vẽ lại; các bảng khác FK vào `UserId` (Guid).
- **Map:** Map (1) – (N) MapSpec (hoặc MapVersion); Map (1) – (N) Hint, Constraint; Map (N) – (N) Tag qua MapTag, (N) – (N) Concept qua MapConcept.
- **Challenge / XP:** Map, UserMapResult, XpTransactions (PK = Id); nếu cần có thể thêm MapSpecId vào XpTransactions nếu gắn XP theo version.
- **Competitive:** Match (1) – (N) Room, (1) – (N) UserMatchResult; Room (1) – (N) RoomParticipant; Submission → UserMatchResult.
- **Payment:** PaymentRecord (có PaymentId → Payment hoặc có sẵn PaymentType + Description trong bảng).

---

## 6. Kết luận

- ERD hiện tại đã phủ đủ các luồng chính của QuackOrbit theo Overview (challenge, competitive, UGC, XP, achievement, package).
- Để hợp với base Clean Architecture và tránh nhầm lẫn sau này:
  1. Thống nhất **Id (Guid)**, **BaseEntity**, **EntityStatusEnum** và audit cho mọi bảng mới.
  2. **Gộp/bỏ MapDetail (singular)**, giữ một bảng spec rõ tên (MapSpec/MapVersion) và quy tắc “update màn”.
  3. **Nối Payment với PaymentRecord** hoặc bỏ Payment và mở rộng PaymentRecord.
  4. Sửa tên PK **XpTransactions** (Id), **UserPackage** (UserPackageId), và làm rõ **Submissions.Language** cùng các enum Status.

Sau khi chỉnh ERD theo các mục trên, bạn có thể chuyển sang bước thiết kế Domain entities (kế thừa BaseEntity/IEntityLike), Value Object cho các Spec, và Repository/Application services phù hợp với từng aggregate.

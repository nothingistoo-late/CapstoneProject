# QuackOrbit – Hướng dẫn API & Kiểu dữ liệu

Tài liệu mô tả chi tiết các API, thứ tự gọi, request/response và các kiểu dữ liệu (enums, DTOs) cho toàn bộ 6 module.

---

## Cấu trúc Role-based Controllers

API được tách theo **vai trò** để dễ bảo trì và tránh gộp chung một controller:

| Nhóm | Base path | Mô tả |
|------|-----------|--------|
| **Learner** | `api/learner/*` | Người học: auth, challenges (catalog + UGC), gameplay, marketplace (xem + mua), community (rate, report), competitive, chat |
| **CMS** | `api/cms/*` | Admin/Moderator: auth, users, challenges (duyệt map + CRUD tags/concepts), **level-maps** (catalog + JSON level), marketplace (CRUD gói + báo cáo thanh toán), community (danh sách + xử lý báo cáo) |

- **Learner:** `api/learner/auth`, `api/learner/challenges`, `api/learner/gameplay`, `api/learner/marketplace`, `api/learner/community`, `api/learner/competitive`, `api/learner/chat`
- **CMS:** `api/cms/auth`, `api/cms/users`, `api/cms/challenges`, `api/cms/level-maps`, `api/cms/marketplace`, `api/cms/community`

---

## Mục lục

1. [Chuẩn API chung](#1-chuẩn-api-chung)
2. [Các kiểu dữ liệu dùng chung](#2-các-kiểu-dữ-liệu-dùng-chung)
3. [Module 1: Xác thực & Người dùng (Learner / CMS)](#3-module-1-xác-thực--người-dùng-learner--cms)
4. [Module 2: Quản lý Thử thách (Challenge)](#4-module-2-quản-lý-thử-thách-challenge)
5. [Module 2.4: Level Maps (CMS) – Catalog & JSON level](#44-level-maps-cms--catalog--json-level)
6. [Module 3: Gameplay & Tiến trình](#5-module-3-gameplay--tiến-trình)
7. [Module 4: Thi đấu (Competitive)](#6-module-4-thi-đấu-competitive)
8. [Module 5: Marketplace](#7-module-5-marketplace)
9. [Module 6: Community & Báo cáo](#8-module-6-community--báo-cáo)
10. [CMS Users & Chat (tóm tắt)](#9-cms-users--chat-tóm-tắt)

---

## 1. Chuẩn API chung

### Base URL & Header

- **Base URL:** `https://your-api-host/api` (ví dụ: `https://localhost:7001/api`).
- **Content-Type:** `application/json` cho body.
- **Xác thực:** Các endpoint yêu cầu đăng nhập dùng JWT Bearer:
  ```http
  Authorization: Bearer <AccessToken>
  ```

### Định dạng response thống nhất

Mọi API (trừ một số CMS trả trực tiếp `PaginationResult`) đều bọc trong **Result** hoặc **Result&lt;T&gt;**:

**Result (không data):**
```json
{
  "isSuccess": true,
  "message": "Success"
}
```

**Result&lt;T&gt; (có data):**
```json
{
  "isSuccess": true,
  "message": "Success",
  "data": { ... }
}
```

**Khi lỗi:**
```json
{
  "isSuccess": false,
  "message": "Mô tả lỗi",
  "errorCode": "NotFound",
  "errors": ["Chi tiết 1", "Chi tiết 2"]
}
```

- **isSuccess:** `true` | `false`
- **message:** Thông báo ngắn.
- **data:** Chỉ có khi `Result<T>` và thành công.
- **errorCode:** Mã lỗi (xem `ErrorCodeEnum`).
- **errors:** Mảng chi tiết (validation, v.v.).

**Ánh xạ HTTP status:** Backend dùng `ResultExtensions.GetHttpStatusCode()` để trả đúng mã HTTP (401 Unauthorized, 403 Forbidden, 404 NotFound, 400 Bad Request, 422, 500, v.v.) tương ứng `errorCode`.

### Phân trang

Các API danh sách trả **PaginationResult&lt;T&gt;** (thường nằm trong `Result.Data`):

| Thuộc tính    | Kiểu   | Mô tả                          |
|---------------|--------|---------------------------------|
| currentPage   | int    | Trang hiện tại                  |
| pageSize      | int    | Số phần tử mỗi trang            |
| totalItems    | int    | Tổng số bản ghi                 |
| totalPages    | int    | Tổng số trang                   |
| hasPrevious   | bool   | Có trang trước                  |
| hasNext       | bool   | Có trang sau                    |
| items         | T[]    | Danh sách phần tử trang hiện tại |

---

## 2. Các kiểu dữ liệu dùng chung

### 2.1 Enums (Domain)

**RoleEnum** – Vai trò người dùng:
| Giá trị   | Mô tả           |
|-----------|------------------|
| Admin     | Quản trị hệ thống |
| Learner   | Người học        |
| Moderator | Kiểm duyệt nội dung UGC |

**EntityStatusEnum** – Trạng thái entity (User, Package, v.v.):
| Giá trị  | Mô tả    |
|----------|----------|
| Inactive | 0 – Vô hiệu hóa |
| Active   | 1 – Hoạt động   |
| Pending  | 2 – Chờ xử lý   |
| Rejected | 3 – Từ chối     |

**MapStatusEnum** – Vòng đời thử thách (map):
| Giá trị      | Mô tả |
|--------------|--------|
| Draft        | 0 – Nháp (chỉ author xem) |
| PendingReview| 1 – Đã gửi, chờ duyệt     |
| Approved     | 2 – Đã duyệt, chưa xuất bản |
| Rejected     | 3 – Bị từ chối            |
| Published    | 4 – Đã xuất bản lên catalog |

**ReportStatusEnum** – Trạng thái báo cáo:
| Giá trị  | Mô tả        |
|----------|--------------|
| Pending  | 0 – Chờ xử lý |
| Reviewed | 1 – Đã xem    |
| Resolved | 2 – Đã xử lý  |
| Dismissed| 3 – Đã bỏ qua |

### 2.2 ErrorCodeEnum (Application)

Dùng trong `Result.ErrorCode` để client xử lý lỗi:

- **1xxx – Auth:** Unauthorized, Forbidden, InvalidCredentials, TokenExpired, InvalidToken  
- **2xxx – Validation:** ValidationFailed, InvalidInput, DuplicateEntry, InvalidOperation, TooManyRequests  
- **3xxx:** NotFound  
- **4xxx – Business:** BusinessRuleViolation, InsufficientPermissions, ResourceConflict  
- **5xxx – Server:** InternalError, DatabaseError, ExternalServiceError  
- **6xxx – File:** FileUploadFailed, FileNotFound, StorageError, InvalidFileType, FileSizeTooLarge  
- **7xxx–8xxx:** FeatureDisabled, InvalidResponse, EmailSendFailed, …

---

## 3. Module 1: Xác thực & Người dùng (Learner / CMS)

**Learner:** `api/learner/auth/*`  
**CMS:** `api/cms/auth/*`

### 3.1 Thứ tự gọi API (Learner)

#### Luồng đăng ký (Register → Verify OTP → có thể Login)

1. **Đăng ký (gửi OTP)**  
   `POST /api/learner/auth/register` (Learner)

   **Body – RegisterRequest:**
   | Field           | Type   | Bắt buộc | Mô tả |
   |-----------------|--------|----------|--------|
   | email           | string | ✓        | Email đăng ký |
   | password        | string | ✓        | Mật khẩu (min 6 ký tự) |
   | confirmPassword | string | ✓        | Xác nhận mật khẩu |
   | firstName       | string | ✓        | Tên |
   | lastName        | string | ✓        | Họ |
   | phoneNumber     | string |          | SĐT (nếu có) |
   | learnerCode     | string |          | Mã học viên (tùy chọn) |
   | gender          | int?   |          | Giới tính (enum) |
   | dateOfBirth     | date?  |          | Ngày sinh |
   | otpSentChannel  | int?   |          | Gửi OTP qua Email (mặc định) hoặc SMS |

   **Response:** `Result` – success khi OTP đã gửi (qua email/SMS). User chưa active.

2. **Xác thực OTP (hoàn tất đăng ký)**  
   `POST /api/learner/auth/verify-otp`

   **Body – VerifyOtpRequest:**
   | Field          | Type   | Mô tả |
   |----------------|--------|--------|
   | contact        | string | Email hoặc SĐT (đúng với đăng ký) |
   | otp            | string | Mã OTP nhận được |
   | otpSentChannel | int    | NotificationChannelEnum: Email / SMS |
   | otpType        | int    | OtpTypeEnum: Registration (đăng ký) / ResetPassword |

   **Response:** `Result` – thành công thì tài khoản active, có thể đăng nhập.

3. **Đăng nhập**  
   `POST /api/learner/auth/login`

   **Body – LoginRequest:**
   | Field     | Type | Mô tả |
   |-----------|------|--------|
   | email     | string | Email đã đăng ký |
   | password  | string | Mật khẩu |
   | grantType | int   | GrantTypeEnum.Password |

   **Response:** `Result<AuthResponse>`  
   - **AuthResponse:** `accessToken`, `expiresAt`, `roles` (mảng string, ví dụ `["Learner"]`).

4. **Refresh token**  
   `POST /api/learner/auth/refresh-token`  
   Header: `Authorization: Bearer <AccessToken hiện tại>`  
   **Response:** `Result<AuthResponse>` – token mới.

5. **Lấy profile**  
   `GET /api/learner/auth/profile`  
   **Response:** `Result<ProfileResponse>` (userId, email, firstName, lastName, phoneNumber, avatarPath, …).

6. **Cập nhật profile**  
   `PUT /api/learner/auth/profile`  
   Body: UpdateProfileRequest (firstName, lastName, phoneNumber, dateOfBirth, bio, …); có thể kèm file avatar (multipart).

7. **Đổi mật khẩu**  
   `POST /api/learner/auth/change-password`  
   Body: currentPassword, newPassword, confirmPassword.

8. **Đăng xuất**  
   `POST /api/learner/auth/logout`  
   Header: Bearer token. Server invalidation refresh token (nếu có).

#### Luồng khác

- **Google đăng nhập:** `POST /api/learner/auth/google` – Body: `{ "idToken": "<Google IdToken>" }`. Trả `Result<AuthResponse>`, role mặc định Learner.
- **Quên mật khẩu:** `POST /api/learner/auth/reset-password` – Gửi OTP; sau đó dùng OTP trong verify-otp với `otpType: ResetPassword` và flow đặt lại mật khẩu (nếu có endpoint riêng).
- **Quick Login:** `POST /api/learner/auth/quick-login` – Body: `{ "quickCode": "DEMO123" }`. Có thể tắt bằng cấu hình `QuickLogin:Enabled: false` trong appsettings.

### 3.2 CMS Auth

- **Login:** `POST /api/cms/auth/login` – Email + password (Admin/Moderator).  
- **Profile:** `GET /api/cms/auth/profile`, `PUT /api/cms/auth/profile`.  
- **Refresh:** `POST /api/cms/auth/refresh-token`.  
- **Logout:** `POST /api/cms/auth/logout`.

Request/response tương tự Learner (AuthResponse, Result). Role trả trong `roles` (Admin, Moderator).

---

## 4. Module 2: Quản lý Thử thách (Challenge)

**Learner (catalog + UGC):** `api/learner/challenges` (maps, tags, concepts read-only)  
**CMS (duyệt + CRUD tags/concepts):** `api/cms/challenges` (maps moderation, tags/concepts CRUD)

### 4.1 Kiểu dữ liệu

**CreateMapRequest** – Tạo/sửa map (spec, hints, constraints, tag/concept):
| Field              | Type   | Mô tả |
|--------------------|--------|--------|
| title              | string | Bắt buộc, max 200 |
| description        | string | |
| difficulty         | int    | 1–5 |
| timeLimitMs        | int    | > 0 (ms) |
| price              | decimal? | Map trả phí (null = free) |
| gridSpec           | string | Spec lưới chơi |
| initialStateSpec   | string | Trạng thái ban đầu |
| winConditionSpec   | string | Điều kiện thắng |
| failConditionSpec  | string | Điều kiện thua |
| hints              | List&lt;HintItemDto&gt; | OrderNo, Content |
| constraints        | List&lt;ConstraintItemDto&gt; | Type, Payload |
| tagIds             | List&lt;Guid&gt; | Id tag |
| conceptIds         | List&lt;Guid&gt; | Id concept |

**UpdateMapRequest** – Giống trên, các field có thể null (chỉ cập nhật field gửi lên); thêm `editorialContent`, `unlockEditorialAfterStars` (0–3).

**MapListItemDto** (trong danh sách): Id, Title, Description, Difficulty, TimeLimitMs, IsPublished, MapStatus, Price, CreatedByUserId, CreatedAt, TagNames, ConceptNames.

**MapDetailDto** (chi tiết): Giống list + EditorialContent, UnlockEditorialAfterStars, ActiveSpec (MapSpecDto), Hints, Constraints. Editorial chỉ trả khi user đủ sao nếu gọi với `includeEditorialForUser=true`.

### 4.2 Thứ tự gọi API (Maps)

#### Người học (Learner) – Chơi thử thách

1. **Lấy danh sách map (catalog)**  
   `GET /api/learner/challenges?pageNumber=1&pageSize=20&publishedOnly=true&...`

   Query (GetMapsQuery): pageNumber, pageSize, difficulty, conceptId, tagId, **publishedOnly** (true = chỉ Published), mapStatus, search, createdByUserId, sortBy (CreatedAt|Title|Difficulty|TimeLimitMs), sortAscending.

   **Response:** `Result<PaginationResult<MapListItemDto>>`.

2. **Xem chi tiết map**  
   `GET /api/learner/challenges/{id}?includeEditorialForUser=true`  
   Trả MapDetailDto; editorial chỉ có nếu user đạt đủ sao (UnlockEditorialAfterStars).

3. (Nếu map trả phí) Mua map: xem [Module 5 – Purchase Map](#75-mua-map-mua-thử-thách-trả-phí).

#### Tác giả / UGC (Learner) – Tạo và gửi duyệt

1. **Lấy tags/concepts (cho form tạo map)**  
   `GET /api/learner/challenges/tags?search=`  
   `GET /api/learner/challenges/concepts?search=`  
   Trả `Result<List<TagDto>>`, `Result<List<ConceptDto>>`.

2. **Tạo map (nháp)**  
   `POST /api/learner/challenges`  
   Body: **CreateMapRequest**.  
   **Response:** `Result<Guid>` – Id map vừa tạo (status = Draft).  
   **Role:** Learner, Admin, Moderator.

3. **Cập nhật map (nháp)**  
   `PUT /api/learner/challenges/{id}`  
   Body: **UpdateMapRequest**. Chỉ author hoặc Admin/Moderator.

4. **Gửi duyệt**  
   `POST /api/learner/challenges/{id}/submit`  
   Chuyển map sang **PendingReview**. Role: Learner (author).

#### Admin/Moderator (CMS) – Duyệt & xuất bản

5. **Duyệt map**  
   `POST /api/cms/challenges/maps/{id}/approve`  
   Query (optional): `reviewNote=...`. Chuyển PendingReview → **Approved**.

6. **Từ chối map**  
   `POST /api/cms/challenges/maps/{id}/reject`  
   Query (optional): `rejectReason=...`. Chuyển → **Rejected**.

7. **Xuất bản map**  
   `POST /api/cms/challenges/maps/{id}/publish`  
   Chỉ map **Approved** → **Published** (hiện trên catalog).

8. **Batch duyệt/từ chối/xuất bản**  
   - `POST /api/cms/challenges/maps/batch/approve` – Body: `{ "mapIds": [...], "reviewNote": "..." }`  
   - `POST /api/cms/challenges/maps/batch/reject` – Body: `{ "mapIds": [...], "rejectReason": "..." }`  
   - `POST /api/cms/challenges/maps/batch/publish` – Body: `{ "mapIds": [...] }`  
   Response: Result với DTO chứa successCount, failedCount, notFoundIds, invalidStatusIds (nếu có).

9. **Xóa map (soft delete)**  
   `DELETE /api/learner/challenges/{id}` (author) hoặc `DELETE /api/cms/challenges/maps/{id}` (Admin/Moderator).

### 4.3 Tags & Concepts (CMS – Admin/Moderator)

- **Tags:**  
  `GET /api/cms/challenges/tags?search=`  
  `POST /api/cms/challenges/tags` – Body: `{ "name": "..." }` → `Result<Guid>`  
  `PUT /api/cms/challenges/tags/{id}` – Body: `{ "name": "..." }`  
  `DELETE /api/cms/challenges/tags/{id}`

- **Concepts:**  
  `GET /api/cms/challenges/concepts?search=`  
  `POST /api/cms/challenges/concepts` – Body: `{ "name": "...", "description": null }`  
  `PUT /api/cms/challenges/concepts/{id}`  
  `DELETE /api/cms/challenges/concepts/{id}`

### 4.4 Level Maps (CMS) – Catalog & JSON level

**Base path:** `api/cms/level-maps`  
**Mục đích:** Lưu và quản lý dữ liệu level (JSON từ level editor): thông tin catalog (name, type, difficulty) tách riêng với nội dung JSON đầy đủ (layers, startPosition, goalPosition, metadata…). Dùng cho CMS import/export level, đồng bộ catalog từ FE.

**Entity (Domain):**

- **LevelCatalog:** Id (Guid), Name, Type, Difficulty (kế thừa BaseEntity: audit, soft delete, status). Không có ExternalId, File.
- **LevelDetail:** Id, LevelCatalogId (FK), JsonContent (string – raw JSON). Quan hệ 1-1 với LevelCatalog (cascade delete).

**DTOs (Application.Commons.DTOs.Maps):**

| DTO | Mô tả |
|-----|--------|
| **MapsFilter** | Pagination + search, sortBy (name \| createdAt \| updatedAt), isAscending, status (EntityStatusEnum?) |
| **MapsListItemDto** | id, name, type, difficulty, createdAt (danh sách catalog) |
| **MapsResponseDto** | id, name, type, difficulty, jsonContent (string?, từ LevelDetail), createdAt, updatedAt |
| **CreateMapsRequest** | **level** (object, bắt buộc): JSON đầy đủ level; **name**, **type**, **difficulty** (optional): override catalog |
| **UpdateMapsRequest** | name?, type?, difficulty?, jsonContent? (string – ghi đè JSON chi tiết) |
| **BatchCreateMapsRequest** | **levels** (array of object) và/hoặc **jsonContents** (array of string) |
| **BatchUpsertCatalogRequest** | **levels**: array of { id, file, name, type, difficulty } (upsert theo name) |
| **BatchDeleteMapsRequest** | **ids**: List&lt;Guid&gt; |
| **BatchCreateMapsResultDto** | successCount, failedCount, createdIds, errors |
| **BatchUpsertCatalogResultDto** | (theo implementation) |
| **BatchDeleteMapsResultDto** | successCount, notFoundCount, notFoundIds |

**Thứ tự gọi API (Level Maps – CMS):**

| Method | Path | Mô tả |
|--------|------|--------|
| GET | `/api/cms/level-maps` | Danh sách catalog có phân trang. Query: page, pageSize, search, sortBy, isAscending, status. **Response:** PaginationResult&lt;MapsListItemDto&gt; (thường trả trực tiếp trong Result). |
| GET | `/api/cms/level-maps/{id}` | Chi tiết một level (catalog + jsonContent). **Response:** Result&lt;MapsResponseDto&gt;. |
| POST | `/api/cms/level-maps` | Tạo một level: body **CreateMapsRequest** (level object + name/type/difficulty optional). Tạo LevelCatalog + LevelDetail (1-1). **Response:** Result&lt;MapsResponseDto&gt; (201). |
| PUT | `/api/cms/level-maps/{id}` | Cập nhật catalog và/hoặc jsonContent. Body **UpdateMapsRequest**. Nếu gửi jsonContent thì ghi đè LevelDetail.JsonContent. |
| DELETE | `/api/cms/level-maps/{id}` | Soft-delete level (IsDeleted trên LevelCatalog). |
| POST | `/api/cms/level-maps/batch/create` | Batch tạo nhiều level. Body **BatchCreateMapsRequest** (levels hoặc jsonContents). **Response:** Result&lt;BatchCreateMapsResultDto&gt; (successCount, failedCount, createdIds, errors). |
| POST | `/api/cms/level-maps/batch/upsert-catalog` | Đồng bộ catalog từ FE. Body **BatchUpsertCatalogRequest**. Upsert theo **name** (tạo mới hoặc cập nhật type/difficulty); không tạo/sửa LevelDetail.JsonContent. |
| POST | `/api/cms/level-maps/batch/delete` | Soft-delete nhiều level theo danh sách Id. Body **BatchDeleteMapsRequest**. **Response:** Result&lt;BatchDeleteMapsResultDto&gt;. |

**Ghi chú:** Tất cả endpoint Level Maps yêu cầu role Admin hoặc Moderator. Swagger có mô tả chi tiết (remarks) cho từng API trong `LevelMapsController`.

---

## 5. Module 3: Gameplay & Tiến trình

**Base path (Learner):** `api/learner/gameplay`

### 5.1 Validate solution (nộp bài)

`POST /api/learner/gameplay/validate`  
Header: Bearer (Learner).

**Body – ValidateSolutionRequest:**
| Field       | Type   | Mô tả |
|-------------|--------|--------|
| mapId       | Guid   | Id map đang chơi |
| language    | string | Mặc định "Blockly" |
| astSpec     | string?| Chuỗi AST (hoặc bytecodeSpec) |
| bytecodeSpec| string?| Chuỗi bytecode (ít nhất một trong hai phải có) |

**Response:** `Result<ValidateSolutionResultDto>` – thường chứa trạng thái (Accepted/Rejected), điểm/sao, thông báo. Backend tạo Submission, ExecutionsResult, cập nhật UserMapResult và cộng XP (10 + stars*5).

**Thứ tự gọi:** Sau khi learner chọn map (GET map by id), chơi xong → gửi validate → nhận kết quả; có thể gọi thêm GetHints nếu cần gợi ý.

### 5.2 Gợi ý (hints)

`GET /api/learner/gameplay/maps/{mapId}/hints`  
**Response:** `Result<List<HintLevelDto>>` – danh sách hint theo cấp (OrderNo, Content). Dùng sau khi có mapId (từ catalog hoặc chi tiết map).

### 5.3 Dashboard tiến trình

`GET /api/learner/gameplay/dashboard`  
Header: Bearer (Learner).

**Response:** `Result<ProgressDashboardDto>` – totalXp, mapsCompleted, totalStars, badges, conceptsPracticed, recentActivities. Gọi bất kỳ lúc nào sau khi đăng nhập để hiển thị trang “Tiến trình của tôi”.

---

## 6. Module 4: Thi đấu (Competitive)

**Base path REST (Learner):** `api/learner/competitive`  
**SignalR Hub:** `/hubs/competitive`

### 6.1 Thứ tự gọi (REST)

1. **Tạo trận đấu**  
   `POST /api/learner/competitive/matches`  
   Body: `{ "mapId": "<guid>", "rulesSpec": null }`  
   **Response:** `Result<Guid>` – matchId.

2. **Tạo phòng**  
   `POST /api/learner/competitive/matches/{matchId}/rooms?maxPlayers=8`  
   **Response:** `Result<CreateRoomResultDto>` – chứa **roomCode** (string) để người chơi join.

3. **Vào phòng**  
   `POST /api/learner/competitive/rooms/join`  
   Body: `{ "roomCode": "..." }`  
   **Response:** `Result<JoinRoomResultDto>` – thông tin phòng; client dùng roomCode để kết nối SignalR.

4. **SignalR – JoinRoom(roomCode), LeaveRoom(roomCode), SubmitSolution(roomCode, astSpec, bytecodeSpec)**  
   Server có thể broadcast ranking (BroadcastRanking). Thứ tự: join room qua REST → connect Hub → JoinRoom(roomCode) → chơi và SubmitSolution khi hoàn thành.

---

## 7. Module 5: Marketplace

**Learner (xem + mua):** `api/learner/marketplace`  
**CMS (quản lý gói + báo cáo):** `api/cms/marketplace`

### 7.1 Gói tính năng (Packages)

**CreatePackageRequest:** name, durationDays, limit (optional), price, featuresSpec (optional).  
**UpdatePackageRequest:** name?, durationDays?, limit?, price?, featuresSpec?, isActive?.

- **Danh sách gói (Learner):**  
  `GET /api/learner/marketplace/packages?pageNumber=1&pageSize=20&isActive=true&search=`  
  **Response:** `Result<PaginationResult<PackageDto>>`.

- **Chi tiết gói (Learner):**  
  `GET /api/learner/marketplace/packages/{id}`  
  **Response:** `Result<PackageDto>`.

- **Tạo gói (CMS – Admin):**  
  `POST /api/cms/marketplace/packages`  
  Body: CreatePackageRequest → `Result<Guid>`.

- **Sửa gói (CMS):**  
  `PUT /api/cms/marketplace/packages/{id}`  
  Body: UpdatePackageRequest.

- **Xóa gói (CMS, soft delete):**  
  `DELETE /api/cms/marketplace/packages/{id}`.

- **Batch bật/tắt (CMS):**  
  `POST /api/cms/marketplace/packages/batch/status`  
  Body: `{ "packageIds": [...], "isActive": true }`  
  **Response:** DTO với successCount, failedCount, notFoundIds.

### 7.2 Mua gói (Learner)

`POST /api/learner/marketplace/packages/{id}/purchase`  
Query (optional): `paymentMethodId=`.  
**Response:** `Result<Guid>`. Header: Bearer (Learner).

### 7.3 Mua map (thử thách trả phí – Learner)

`POST /api/learner/marketplace/maps/{mapId}/purchase`  
Query (optional): `paymentMethodId=`.  
**Response:** `Result<Guid>`. Chỉ map có price > 0; nếu map free API trả lỗi InvalidOperation.

### 7.4 Báo cáo thanh toán (CMS – Admin)

`GET /api/cms/marketplace/reports/payments?from=&to=&groupBy=Day|Month|Year`  
**Response:** `Result<PaymentReportDto>`.

---

## 8. Module 6: Community & Báo cáo

**Learner (rate, report):** `api/learner/community`  
**CMS (quản lý báo cáo):** `api/cms/community`

### 8.1 Đánh giá map (Learner)

`POST /api/community/maps/{mapId}/rate`  
Body: `{ "rating": 1-5, "comment": "..." }`  
**Response:** `Result`. Gọi sau khi chơi map (sau validate hoặc khi xem chi tiết map).

### 8.2 Báo cáo map (Learner)

`POST /api/community/maps/{mapId}/report`  
Body: `{ "reason": "string", "details": "..." }`  
**Response:** `Result<Guid>` – reportId. Dùng khi người dùng bấm “Báo cáo nội dung”.

### 8.3 Quản lý báo cáo (Admin/Moderator)

- **Danh sách báo cáo:**  
  `GET /api/community/reports?status=&mapId=&userId=&dateFrom=&dateTo=&pageNumber=&pageSize=`  
  **Response:** Paginated reports (filter theo ReportStatusEnum, mapId, userId, khoảng ngày).

- **Xử lý một báo cáo:**  
  - `POST /api/community/reports/{reportId}/resolve` – Body (optional): `{ "reviewNote": "..." }`  
  - `POST /api/community/reports/{reportId}/dismiss` – Body (optional): `{ "reviewNote": "..." }`

- **Batch:**  
  - `POST /api/cms/community/reports/batch/resolve` – Body: `{ "reportIds": [...], "reviewNote": "..." }`  
  - `POST /api/cms/community/reports/batch/dismiss` – Body: `{ "reportIds": [...], "reviewNote": "..." }`  
  Response DTO: successCount, failedCount, notFoundIds.

---

## 9. CMS Users & Chat (tóm tắt)

### 9.1 CMS Users – `api/cms/users`

- **Danh sách:** `GET /api/cms/users` – query: filter (search, email, phoneNumber, role, status, joiningFrom, joiningTo, sortBy), pageNumber, pageSize.  
  **Response:** `PaginationResult<UserListItem>` (có thể không bọc Result tùy implementation).
- **Chi tiết:** `GET /api/cms/users/{id}`.
- **Tạo user:** `POST /api/cms/users` – multipart (CreateUserRequest + avatar file).
- **Sửa user:** `PUT /api/cms/users/{id}`.
- **Xóa (soft):** `DELETE /api/cms/users/{id}`.
- **Batch đổi trạng thái:** `POST /api/cms/users/batch/status` – Body: `{ "userIds": [...], "status": 0|1 }` (EntityStatusEnum).
- **QuickLogin cleanup:** `POST /api/cms/users/quicklogin/cleanup?daysInactive=7` – Admin (dọn user quick-login không hoạt động).

### 9.2 Chat (Learner) – `api/learner/chat`

- **Tạo hội thoại riêng:** `POST /api/learner/chat/conversations/private` – Body chứa userId đối phương.
- **Tạo nhóm tạm:** `POST /api/learner/chat/conversations/temporary-group` – Body: name.
- **Đóng hội thoại nhóm:** `POST /api/learner/chat/conversations/{conversationId}/close`.
- **Danh sách hội thoại:** `GET /api/learner/chat/conversations?pageNumber=1&pageSize=20&searchTerm=`.
- **Tin nhắn:** `GET /api/learner/chat/conversations/{conversationId}/messages?...`, `POST .../messages`, `PUT /api/learner/chat/messages/{messageId}`, `DELETE /api/learner/chat/messages/{messageId}`.
- **Danh sách user (tìm bạn chat):** `GET /api/learner/chat/users?pageNumber=1&pageSize=100&searchTerm=`.

Tất cả Chat yêu cầu Bearer token. SignalR ChatHub dùng cho real-time (riêng với Competitive Hub).

---

## Cấu trúc thư mục (tham khảo)

```
Application/
  Commons/
    DTOs/Challenge: CreateMapRequest, UpdateMapRequest, MapListItemDto, MapDetailDto, BatchMapRequests, HintItemDto, ConstraintItemDto
    DTOs/Maps: CreateMapsRequest, UpdateMapsRequest, MapsFilter, MapsListItemDto, MapsResponseDto, BatchCreateMapsRequest, BatchUpsertCatalogRequest, BatchDeleteMapsRequest, LevelCatalogItemDto, BatchCreateMapsResultDto, BatchUpsertCatalogResultDto, BatchDeleteMapsResultDto
    DTOs/Marketplace: PackageDto, CreatePackageRequest, UpdatePackageRequest, PackageFilter
    DTOs/Auth: RegisterRequest, LoginRequest, VerifyOtpRequest, AuthResponse, ProfileResponse
    Models: Result, Result<T>, PaginationResult<T>
    Enums: ErrorCodeEnum
  Features/
    Challenge/Commands: CreateMap, UpdateMap, DeleteMap, SubmitMapForReview, ApproveMap, RejectMap, PublishMap, BatchApprove/Reject/Publish, CreateTag, UpdateTag, DeleteTag, CreateConcept, UpdateConcept, DeleteConcept
    Challenge/Queries: GetMaps, GetMapById, GetTags, GetConcepts
    Maps/Commands: CreateMaps, UpdateMaps, DeleteMaps, BatchCreateMaps, BatchUpsertCatalog, BatchDeleteMaps
    Maps/Queries: GetPagedMaps, GetMapsById
    Auth/Commands: Login, Register, VerifyOtp, QuickLogin, GoogleLogin, RefreshToken, Logout, UpdateProfile, ChangePassword, ResetPassword
    Gameplay/Commands: ValidateSolution
    Gameplay/Queries: GetHintsForMap, GetProgressDashboard
    Competitive/Commands: CreateMatch, CreateRoom, JoinRoom
    Marketplace/Commands: CreatePackage, UpdatePackage, DeletePackage, PurchasePackage, PurchaseMap, BatchUpdatePackageStatus
    Marketplace/Queries: GetPackages, GetPackageById, GetPaymentReport
    Community/Commands: RateMap, ReportMap, ResolveReport, DismissReport, BatchResolveReports, BatchDismissReports
    Community/Queries: GetReports
    User/Commands: CreateUser, UpdateUser, DeleteUser, BatchUpdateUserStatus
    User/Queries: GetPagedUsers, GetUserById

API/Controllers/
  Learner: AuthController, ChallengeController, GameplayController, MarketplaceController, CommunityController, CompetitiveController, ChatController
  Cms: AuthController, UserController, ChallengeController, LevelMapsController, MarketplaceController, CommunityController
```

---

## Mở rộng

- **Thêm Command/Query:** Tạo thư mục trong `Features/<Module>`, thêm Handler + Validator (FluentValidation); MediatR quét assembly.
- **Thêm endpoint:** Controller tương ứng, inject IMediator, gọi `Send(command/query)`, dùng `AuthorizeRoles` theo RBAC.
- **SignalR:** Hub kế thừa `Hub`, đăng ký trong Startup; client kết nối tới `/hubs/<tên>`.

Sau khi chạy migration và cập nhật database, có thể kiểm thử lần lượt: Auth (register → verify-otp → login) → Challenge (get maps, create map, submit, approve, publish) → Gameplay (validate, dashboard) → Marketplace (packages, purchase) → Community (rate, report, resolve).

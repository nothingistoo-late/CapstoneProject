# Danh Sách Chi Tiết Các Tính Năng Có Sẵn

## 🎯 Tổng Quan
Dự án base được xây dựng với .NET 8, Clean Architecture, CQRS (MediatR), tích hợp đầy đủ các chức năng cho hệ thống QuackOrbit: xác thực, quản lý user, thử thách (maps), gameplay, lobby, competitive, marketplace, OrbitCoin, community, chat. Controllers mỏng; logic nghiệp vụ nằm trong Command/Query Handlers.

---

## 🔐 1. AUTHENTICATION & AUTHORIZATION

### 1.1. CMS Authentication (`/api/cms/auth`)
**Controller:** `Cms/AuthController.cs`

| Endpoint | Method | Mô Tả | Authorization |
|----------|--------|-------|---------------|
| `/api/cms/auth/login` | POST | Đăng nhập CMS (Admin/Moderator) | Public (với filter) |
| `/api/cms/auth/logout` | POST | Đăng xuất CMS | Admin, Moderator |
| `/api/cms/auth/profile` | GET | Lấy profile user đang đăng nhập | Admin, Moderator |
| `/api/cms/auth/profile` | PUT | Cập nhật profile (upload avatar) | Admin, Moderator |
| `/api/cms/auth/refresh-token` | POST | Refresh access token | Admin, Moderator |

### 1.2. Learner Authentication (`/api/learner/auth`)
**Controller:** `Learner/AuthController.cs`

| Endpoint | Method | Mô Tả | Authorization |
|----------|--------|-------|---------------|
| `/api/learner/auth/login` | POST | Đăng nhập (email/password) | Public |
| `/api/learner/auth/quick-login` | POST | Đăng nhập nhanh (quickCode demo) | Public |
| `/api/learner/auth/google` | POST | Đăng nhập Google (idToken) | Public |
| `/api/learner/auth/logout` | POST | Đăng xuất | Learner |
| `/api/learner/auth/register` | POST | Đăng ký (gửi OTP, multipart/form-data) | Public |
| `/api/learner/auth/verify-otp` | POST | Xác thực OTP (đăng ký / reset password) | Public |
| `/api/learner/auth/reset-password` | POST | Yêu cầu reset password (gửi OTP) | Public |
| `/api/learner/auth/change-password` | POST | Đổi mật khẩu (cần password hiện tại) | Learner |
| `/api/learner/auth/profile` | GET | Lấy profile | Learner |
| `/api/learner/auth/profile` | PUT | Cập nhật profile (upload avatar) | Learner |
| `/api/learner/auth/refresh-token` | POST | Refresh access token | Learner |

### 1.3. Authentication Features
- ✅ **JWT Bearer Authentication**
  - Access Token với thời gian hết hạn configurable
  - Refresh Token lưu trong database
  - Auto refresh token mechanism
  
- ✅ **ASP.NET Core Identity**
  - Custom AppUser và AppRole entities
  - Role-based authorization
  - Password hashing và validation
  
- ✅ **Google OAuth** (Cấu hình sẵn, chưa implement endpoint)
  - GoogleSettings trong appsettings
  
- ✅ **OTP Verification System**
  - OTP gửi qua Email hoặc SMS (hiện tại chỉ Email)
  - Rate limiting cho OTP requests
  - OTP expiration và attempt limits
  - Cache-based OTP storage

### 1.4. Authorization Attributes
- ✅ `[AuthorizeRoles]` - Chỉ định roles được phép truy cập
- ✅ `[ServiceFilter(typeof(AdminRoleAccessFilter))]` - Filter cho CMS endpoints
- ✅ `[SkipModelValidation]` - Bỏ qua validation cho form-data

---

## 👥 2. USER MANAGEMENT

### 2.1. CMS User Management (`/api/cms/users`)
**Controller:** `Cms/UserController.cs`
**Authorization:** Admin only

| Endpoint | Method | Mô Tả | Request Body |
|----------|--------|-------|--------------|
| `/api/cms/users` | GET | Lấy danh sách users có phân trang, filter, sort | Query params: filter |
| `/api/cms/users/{id}` | GET | Lấy thông tin chi tiết user theo ID | - |
| `/api/cms/users` | POST | Tạo user mới (hỗ trợ upload avatar) | Form-data: CreateUserRequest + avatarFile |
| `/api/cms/users/{id}` | PUT | Cập nhật thông tin user (hỗ trợ upload avatar) | Form-data: UpdateUserRequest + avatarFile |
| `/api/cms/users/{id}` | DELETE | Xóa user (soft delete) | - |

### 2.2. User Features (CMS)
- ✅ **User CRUD Operations**
  - Create user với role assignment (Admin only)
  - Update user (Admin only)
  - Delete user – soft delete (Admin only)
  - Get user by ID, Get paginated users (filter, sort)
  
- ✅ **Batch & Cleanup**
  - Batch update user status (Active/Inactive)
  - QuickLogin cleanup: deactivate inactive QuickLogin users (Hangfire + manual trigger)

- ✅ **User Profile**
  - FirstName, LastName, Email, PhoneNumber
  - Avatar upload và management
  - Status tracking (Active, Inactive, Pending, Rejected)
  - Audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
  
- ✅ **User Roles**
  - Admin
  - Moderator
  - Learner
  - Multiple role support (future)

### 2.3. Các module API khác (tóm tắt)
- **Maps (Thử thách):** `api/learner/maps` (catalog, tạo/sửa/xóa map, upload JSON, tags), `api/cms/maps` (duyệt, approve/reject/publish, tags CRUD).
- **Game Lobby:** `api/learner/lobby` (rooms, join, start/end, submit solution, set map); real-time qua SignalR `/hubs/gamelobby`.
- **OrbitCoin:** `api/learner/orbitcoin` (balance, transactions, deposit, confirm), `api/cms/orbitcoin` (credit); webhook PayOS `api/webhooks/payos`.
- **Marketplace:** `api/learner/marketplace` (packages, purchase package/map), `api/cms/marketplace` (CRUD packages, payment report).
- **Community:** `api/learner/community` (rate, report), `api/cms/community` (reports, resolve/dismiss).
- **Competitive:** `api/learner/competitive` (matches, rooms, join); SignalR `/hubs/competitive`.
- **Chat:** `api/learner/chat` (conversations, messages); SignalR `/hubs/chat`.

---

## 📧 3. EMAIL SERVICE

### 3.1. Email Features
- ✅ **SMTP Email Service**
  - Gửi email thông qua SMTP
  - Hỗ trợ HTML content
  - Email templates cho OTP
  - Configurable email settings (SMTP server, port, SSL)
  
- ✅ **Email Templates**
  - OTP verification email template
  - Customizable với NotificationTemplateHelper
  - Support email và app name configuration

---

## 📁 4. FILE MANAGEMENT

### 4.1. File Service (`LocalFileService`)
**Interface:** `IFileService`

| Method | Mô Tả |
|--------|-------|
| `UploadFileAsync(IFormFile, fileName, subDirectory)` | Upload file từ IFormFile |
| `UploadFileAsync(Stream, fileName, contentType, subDirectory)` | Upload file từ Stream |
| `DeleteFileAsync(filePath)` | Xóa file |
| `GetFileUrl(filePath)` | Lấy public URL của file |
| `GetFileContentAsync(filePath)` | Lấy nội dung file (bytes + content type) |

### 4.2. File Features
- ✅ **Local File Storage**
  - Upload files vào thư mục `wwwroot/uploads/`
  - Hỗ trợ subdirectories (avatars, lesson-documents, etc.)
  - File path management
  - File deletion với fire-and-forget pattern
  
- ✅ **File Upload Support**
  - Avatar upload cho users
  - File validation (size, type)
  - Unique filename generation

---

## 🔑 5. OTP (ONE-TIME PASSWORD) SYSTEM

### 5.1. OTP Cache Service (`OtpCacheService`)
**Interface:** `IOtpCacheService`

| Method | Mô Tả |
|--------|-------|
| `GenerateAndStoreOtp(contact, type, userData, channel)` | Tạo và lưu OTP |
| `GetOtpData(contact, type)` | Lấy OTP data |
| `VerifyOtp(contact, otpCode, type, channel)` | Xác thực OTP |
| `RemoveOtp(contact, type)` | Xóa OTP |
| `GetRemainingAttempts(contact, type)` | Lấy số lần thử còn lại |
| `IsOtpExpired(contact, type)` | Kiểm tra OTP đã hết hạn |
| `GetRemainingTime(contact, type)` | Lấy thời gian còn lại |
| `CleanUpExpriredOtp()` | Dọn dẹp OTP hết hạn |
| `GetActiveCacheCount()` | Lấy số lượng OTP đang active |
| `ClearAllCache()` | Xóa tất cả cache |
| `ClearRateLimitTracker(contact)` | Xóa rate limit tracker |
| `GetRateLimitStatus(contact)` | Lấy trạng thái rate limit |

### 5.2. OTP Features
- ✅ **OTP Generation & Validation**
  - 6-digit OTP code
  - Configurable expiration time (mặc định 5 phút)
  - Maximum attempts (mặc định 3 lần)
  - Rate limiting với cooldown period
  
- ✅ **OTP Types**
  - Registration OTP
  - Password Reset OTP
  
- ✅ **OTP Channels**
  - Email (đã implement)
  - SMS (interface sẵn, chưa implement)
  
- ✅ **Rate Limiting**
  - Cooldown period giữa các requests
  - Block duration khi vượt quá số lần request
  - Configurable per OTP type

---

## 🗄️ 6. DATABASE & DATA ACCESS

### 6.1. Database Context
- ✅ **Application DbContext**
  - ASP.NET Core Identity integration
  - AppUser và AppRole entities
  - Custom table naming
  - BaseEntity configuration
  
- ✅ **Dual Database Support**
  - Main database (Identity + Business entities)
  - Outer database (Hangfire jobs)
  
- ✅ **Migrations**
  - Automatic migrations trong Development
  - Manual migration commands
  - Migration factory pattern

### 6.2. Repository Pattern
- ✅ **GenericRepository<T>**
  - CRUD operations
  - Query building với predicates
  - Include support cho navigation properties
  - Soft delete support
  
- ✅ **UnitOfWork Pattern**
  - Transaction management
  - SaveChangesAsync
  - Repository access

### 6.3. Query Builders
- ✅ **UserQueryBuilder**
  - Build predicates từ UserFilter
  - Search functionality (FirstName, LastName, Email, PhoneNumber)
  - Date range filtering
  - Sorting support

---

## 🛡️ 7. MIDDLEWARES

### 7.1. Middleware Pipeline
| Middleware | Thứ Tự | Mô Tả |
|-----------|--------|-------|
| **CORS** | 1 | Cross-Origin Resource Sharing configuration |
| **Static Files** | 2 | Phục vụ static files (Swagger CSS, uploads) |
| **HTTPS Redirection** | 3 | Redirect HTTP sang HTTPS (Production only) |
| **Global Exception Handling** | 4 | Xử lý exceptions toàn cục, logging errors |
| **JWT Middleware** | 5 | Validate JWT tokens, extract user claims |
| **Validation** | 6 | Validate request models với FluentValidation |
| **Authentication** | 7 | ASP.NET Core Authentication |
| **Authorization** | 8 | ASP.NET Core Authorization |

### 7.2. Exception Handling
- ✅ **GlobalExceptionHandlingMiddleware**
  - Catch và log tất cả unhandled exceptions
  - Return standardized error responses
  - Error logging với file logger

### 7.3. Validation
- ✅ **ValidationMiddleware**
  - Tích hợp FluentValidation
  - Auto validation cho requests
  - Standardized validation error responses

---

## 🔧 8. CONFIGURATIONS

### 8.1. API Configurations
- ✅ **Swagger Configuration**
  - Custom Swagger UI với CSS styling
  - API grouping và tagging
  - JWT authentication trong Swagger
  - Development only
  
- ✅ **CORS Configuration**
  - Configurable allowed origins
  - Development ports support (3000, 5173, 4200)
  - Credentials support
  
- ✅ **JWT Configuration**
  - Configurable key, issuer, audience
  - Token expiration settings
  - Refresh token expiration
  
- ✅ **Logging Configuration**
  - File logging với rotation
  - Log levels configuration
  - Structured logging

### 8.2. Infrastructure Configurations
- ✅ **Hangfire Configuration**
  - Background jobs management
  - Dashboard monitoring
  - Retry policies
  - Queue configuration
  - Development: no auth required

---

## 🎨 9. BEHAVIORS (MediatR Pipeline)

### 9.1. MediatR Behaviors
| Behavior | Mô Tả |
|----------|-------|
| **ValidationBehavior** | Tự động validate requests với FluentValidation |
| **AuthorizationBehavior** | Kiểm tra authorization trước khi execute handler |
| **PerformanceBehavior** | Logging performance metrics (response time) |

---

## 🛠️ 10. HELPERS & UTILITIES

### 10.1. DateTime Helper (`DateTimeHelper`)
| Method | Mô Tả |
|--------|-------|
| `GetVietNamTime()` | Lấy thời gian hiện tại theo múi giờ Việt Nam |
| `GetVietNamTime(DateTime utc)` | Convert UTC DateTime sang giờ Việt Nam |
| `GetUtcTime(DateTime vietnam)` | Convert giờ Việt Nam sang UTC |
| `GetVietNamTimeNullable()` | Phiên bản nullable (trả về null nếu lỗi) |
| `GetVietNamTimeZone()` | Lấy TimeZoneInfo của Việt Nam |

**Features:**
- ✅ Hỗ trợ cả Windows và Linux (timezone IDs khác nhau)
- ✅ Auto fallback nếu timezone không tìm thấy

### 10.2. Password Crypto Helper (`PasswordCryptoHelper`)
| Method | Mô Tả |
|--------|-------|
| `Encrypt(plainText, key)` | Mã hóa password với AES |
| `Decrypt(cipherText, key)` | Giải mã password |

### 10.3. Notification Template Helper (`NotificationTemplateHelper`)
- ✅ Build OTP email templates
- ✅ Customizable template data
- ✅ HTML content generation

### 10.4. Mapping Helper (`MappingHelper`)
- ✅ AutoMapper configuration helpers
- ✅ BaseEntity fields ignore
- ✅ Identity fields ignore

---

## 🔄 11. CQRS PATTERN

### 11.1. Commands (Write Operations)
- ✅ **Auth Commands:** Login, Logout, Register, VerifyOtp, ResetPassword, ChangePassword, UpdateProfile, RefreshToken, QuickLogin, GoogleLogin
- ✅ **User Commands:** CreateUser, UpdateUser, DeleteUser, BatchUpdateUserStatus
- ✅ **Maps Commands:** CreateMap, UpdateMap, DeleteMap, SubmitMapForReview, CreateMapFromJsonFile; CMS: ApproveMap, RejectMap, PublishMap, BatchApprove/Reject/Publish, CreateTag, UpdateTag, DeleteTag
- ✅ **Lobby Commands:** CreateLobbyRoom, JoinLobbyRoom, LeaveLobbyRoom, StartLobbyGame, EndLobbyGame, ToggleLobbyReady, SetLobbyRoomMap, SubmitLobbySolution
- ✅ **Competitive Commands:** CreateMatch, CreateRoom, JoinRoom
- ✅ **Gameplay Commands:** ValidateSolution
- ✅ **Marketplace Commands:** CreatePackage, UpdatePackage, DeletePackage, PurchasePackage, PurchaseMapWithOrbitCoin, BatchUpdatePackageStatus
- ✅ **Community Commands:** RateMap, ReportMap, ResolveReport, DismissReport, BatchResolveReports, BatchDismissReports
- ✅ **OrbitCoin Commands:** CreateDepositOrder, ConfirmDeposit, CreditOrbitCoin (CMS), HandlePayOSWebhook

### 11.2. Queries (Read Operations)
- ✅ **Auth:** GetProfileQuery
- ✅ **User:** GetPagedUsersQuery, GetUserByIdQuery
- ✅ **Maps:** GetMapsQuery, GetMapByIdQuery, GetTagsQuery; MapExistsQuery
- ✅ **Lobby:** GetLobbyRoomsQuery, GetLobbyRoomQuery
- ✅ **Gameplay:** GetHintsForMapQuery, GetProgressDashboardQuery
- ✅ **Marketplace:** GetPackagesQuery, GetPackageByIdQuery, GetPaymentReportQuery (CMS)
- ✅ **Community:** GetReportsQuery (CMS)
- ✅ **OrbitCoin:** GetOrbitCoinBalanceQuery, GetOrbitCoinTransactionHistoryQuery

### 11.3. Validation
- ✅ FluentValidation cho tất cả Commands
- ✅ Request validation với validators riêng biệt

---

## 👤 12. CURRENT USER SERVICE

### 12.1. ICurrentUserService
| Property/Method | Mô Tả |
|----------------|-------|
| `UserId` | User ID từ JWT claims |
| `IsAuthenticated` | Kiểm tra user đã authenticated chưa |
| `Roles` | Roles từ JWT claims (quick access) |
| `IsUserValidAsync()` | Validate user existence và status |
| `GetCurrentRolesAsync()` | Lấy roles hiện tại từ database |
| `ValidateUserWithRolesAsync()` | Validate và lấy roles trong 1 call |
| `ValidateUserWithRolesAndEntityAsync()` | Validate và lấy roles + user entity |
| `GetCurrentUserAsync()` | Lấy user entity hiện tại từ database (cached) |

**Features:**
- ✅ Request-scoped caching
- ✅ Database validation (active status, refresh token valid)
- ✅ Role validation từ database

---

## 🔐 13. IDENTITY SERVICE

### 13.1. IIdentityService
| Method | Mô Tả |
|--------|-------|
| `AuthenticateAsync(LoginRequest)` | Xác thực user login |
| `CreateUserAsync(user, password)` | Tạo user mới với Identity |
| `AddUserToRoleAsync(user, role)` | Thêm user vào role |
| `GetUserRolesAsync(user)` | Lấy roles của user |
| `GetUserByIdAsync(userId)` | Lấy user theo ID |
| `GetUserByFirstOrDefaultAsync(predicate)` | Tìm user với predicate |
| `IsEmailDuplicateAsync(user, email)` | Kiểm tra email trùng |
| `IsPhoneNumberDuplicateAsync(user, phone)` | Kiểm tra phone trùng |
| `UpdateUserAsync(user)` | Cập nhật user |
| `RemoveUserRolesAsync(user, role)` | Xóa role khỏi user |
| `ResetUserPasswordAsync(predicate, token, password)` | Reset password |
| `GeneratePasswordResetToken(user)` | Tạo password reset token |
| `ChangePasswordAsync(user, current, new)` | Đổi password |
| `GetUserByIdIncludeProfileAsync(userId)` | Lấy user với profile |

---

## 🎯 14. RESULT PATTERN

### 14.1. Result<T> & Result Classes
- ✅ **Standardized Response Pattern**
  - `Result<T>` - Generic result với data
  - `Result` - Non-generic result
  - Success/Failure states
  - Error codes
  - HTTP status code mapping

### 14.2. PaginationResult<T>
- ✅ Paginated responses
- ✅ PageNumber, PageSize, TotalCount, TotalPages
- ✅ Data collection

---

## 📝 15. VALIDATION

### 15.1. Validators
- ✅ **Auth Validators:**
  - `LoginCommandValidator`
  - `RegisterCommandValidator`
  - `VerifyOtpCommandValidator`
  - `ResetPasswordCommandValidator`
  - `ChangePasswordCommandValidator`
  - `UpdateProfileRequestValidator`
  
- ✅ **User Validators:**
  - `CreateUserRequestValidator`
  - `UpdateUserRequestValidator`

### 15.2. Validation Extensions
- ✅ `ValidEmail()` - Email validation
- ✅ `ValidPassword(minLength)` - Password validation
- ✅ `ValidPhoneNumber()` - Phone number validation
- ✅ `ValidPersonName(fieldName, maxLength)` - Name validation
- ✅ `ValidFile(maxSize, allowedExtensions)` - File validation

---

## 🌐 16. DTOs (DATA TRANSFER OBJECTS)

DTOs tập trung tại **Application/Commons/DTOs/** theo từng domain.

### 16.1. Auth DTOs (`Commons/DTOs/Auth`)
- ✅ `LoginRequest`, `AuthResponse`, `RegisterRequest`, `VerifyOtpRequest`, `ResetPasswordRequest`, `ChangePasswordRequest`, `UpdateProfileRequest`, `ProfileResponse`, `QuickLoginRequest`, `GoogleLoginRequest`

### 16.2. User DTOs (`Commons/DTOs/User`)
- ✅ `CreateUserRequest`, `UpdateUserRequest`, `UserResponse`, `UserListItem`, `UserFilter`, `BatchUpdateUserStatusRequest`

### 16.3. Maps DTOs (`Commons/DTOs/Maps`)
- ✅ `CreateMapRequest`, `UpdateMapRequest`, `MapListItemDto`, `MapDetailDto`, `TagDto`, `CreateTagRequest`, `UpdateTagRequest`, `CreateMapFromJsonFileInput`; Level Maps: `MapsFilter`, `MapsListItemDto`, `MapsResponseDto`, `CreateMapsRequest`, `UpdateMapsRequest`, batch DTOs

### 16.4. Lobby DTOs (`Commons/DTOs/Lobby`)
- ✅ `CreateLobbyRoomRequest`, `CreateLobbyRoomResponse`, `JoinLobbyRoomRequest`, `JoinLobbyRoomResponse`, `LobbyRoomDetailResponse`, `LobbyRoomListItemDto`, `LobbyPlayerDto`, `SetRoomMapRequest`, `StartGameResponse`, `SubmitGameResponse`, `PlayerGameResult`, `PlayerRankingDto`

### 16.5. Gameplay DTOs (`Commons/DTOs/Gameplay`)
- ✅ `ValidateSolutionRequest`, `ValidateSolutionResultDto`, `HintLevelDto`, `ProgressDashboardDto`, `SubmissionSubmitRequest`

### 16.6. Competitive DTOs (`Commons/DTOs/Competitive`)
- ✅ `CreateMatchRequest`, `JoinRoomRequest`, `JoinRoomResultDto` (CreateRoomResultDto trong Features)

### 16.7. Marketplace DTOs (`Commons/DTOs/Marketplace`)
- ✅ `PackageDto`, `PackageFilter`, `CreatePackageRequest`, `UpdatePackageRequest`, `PaymentReportDto`, `BatchUpdatePackageStatusRequest`

### 16.8. Community DTOs (`Commons/DTOs/Community`)
- ✅ `RateMapRequest`, `ReportMapRequest`, `BatchReportsRequest`, `BatchReportResultDto`

### 16.9. OrbitCoin DTOs (`Commons/DTOs/OrbitCoin`)
- ✅ `CreateDepositOrderRequest`, `CreateDepositOrderResult`, `OrbitCoinBalanceDto`, `CreditOrbitCoinRequest`

---

## 📊 17. ENUMS

### 17.1. Domain Enums
- ✅ `RoleEnum`: Admin, Moderator, Learner
- ✅ `EntityStatusEnum`: Inactive, Active, Pending, Rejected
- ✅ `GenderEnum`: Female, Male, Other
- ✅ `GrantTypeEnum`: Password, Google

### 17.2. Application Enums
- ✅ `OtpTypeEnum`: Registration, PasswordReset
- ✅ `NotificationChannelEnum`: Email, SMS, Firebase
- ✅ `ErrorCodeEnum`: Các error codes chuẩn
- ✅ `NotificationTemplateEnums`: Template types

---

## 🔄 18. MAPPING (AutoMapper)

### 18.1. Mapping Profiles
- ✅ `AuthMappingProfile`: RegisterRequest, UpdateProfileRequest → Entities
- ✅ `UserMappingProfile`: User entities ↔ DTOs

### 18.2. Mapping Resolvers
- ✅ `AvatarUrlResolver`: Resolve avatar URL từ path

---

## 📦 19. ENTITIES

### 19.1. Domain Entities
- ✅ `AppUser`: User entity với ASP.NET Core Identity
  - FirstName, LastName
  - Email, PhoneNumber
  - AvatarPath
  - Status tracking
  - Refresh token management
  - Audit fields
  
- ✅ `AppRole`: Role entity với ASP.NET Core Identity
  - Description
  - Status tracking
  - Audit fields

### 19.2. Base Classes
- ✅ `BaseEntity`: Base class với soft delete và audit tracking
- ✅ `IEntityLike`: Interface cho filtering và sorting

---

## 🔒 20. SECURITY FEATURES

### 20.1. Security Implementations
- ✅ **Password Security**
  - ASP.NET Core Identity password hashing
  - AES encryption cho password trong requests
  - Password validation rules
  
- ✅ **Token Security**
  - JWT access tokens
  - Refresh tokens stored in database
  - Token expiration và refresh mechanism
  
- ✅ **OTP Security**
  - Rate limiting
  - Attempt limits
  - Expiration time
  - Secure OTP generation

---

## 🚀 21. BACKGROUND JOBS (Hangfire)

### 21.1. Hangfire Features
- ✅ **Hangfire Integration**
  - Dashboard monitoring (`/hangfire`)
  - Background job execution
  - Retry policies
  - Queue management
  
- ✅ **Configuration**
  - Separate database cho Hangfire
  - Configurable worker count
  - Queue configuration
  - Retry settings

---

## 📚 22. DOCUMENTATION

### 22.1. Swagger Documentation
- ✅ Auto-generated API documentation
- ✅ Custom Swagger UI styling
- ✅ API grouping và tagging
- ✅ Request/Response examples
- ✅ Authorization testing trong Swagger

---

## 🔍 23. EXTENSIONS

### 23.1. Application Extensions
- ✅ Automatic database migrations trong Development
- ✅ Data seeding (roles, admin user)
- ✅ Hangfire initialization

### 23.2. Entity Extensions
- ✅ `InitializeEntity(userId)` - Khởi tạo audit fields
- ✅ `UpdateEntity(userId)` - Cập nhật audit fields
- ✅ `SoftDelete(userId)` - Soft delete entity

---

## 📋 24. RESPONSE FORMATS

### 24.1. Standardized Responses
- ✅ `Result<T>` format:
  ```json
  {
    "isSuccess": true,
    "data": {...},
    "message": "...",
    "errors": [...],
    "errorCode": "..."
  }
  ```

- ✅ `PaginationResult<T>` format:
  ```json
  {
    "items": [...],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 100,
    "totalPages": 10
  }
  ```

---

## 🎯 25. TÍNH NĂNG BỔ SUNG

### 25.1. File Storage
- ✅ Local file storage trong `wwwroot/uploads/`
- ✅ Avatar upload và management
- ✅ File URL generation

### 25.2. Logging
- ✅ File-based logging với rotation
- ✅ Structured logging
- ✅ Error logging trong GlobalExceptionHandlingMiddleware

### 25.3. Health Checks
- ✅ Health check endpoint (`/health`)

---

## 📊 TÓM TẮT TÍNH NĂNG

### ✅ Hoàn Toàn Implemented
- [x] Authentication (Login, Logout, Register, QuickLogin, Google)
- [x] Authorization (Role-based: Admin, Moderator, Learner)
- [x] OTP Verification System
- [x] User Management (CRUD, batch status, QuickLogin cleanup)
- [x] Profile Management
- [x] File Upload (Avatar)
- [x] Email Service
- [x] JWT Token Management
- [x] Password Management (Change, Reset)
- [x] Current User Service
- [x] Maps / Challenges (catalog, UGC, submit, approve, reject, publish, tags, upload JSON)
- [x] Game Lobby (rooms, join, start/end game, submit solution, SignalR)
- [x] Gameplay (validate solution, hints, progress dashboard)
- [x] Competitive (matches, rooms, join, SignalR)
- [x] Marketplace (packages, purchase package/map với OrbitCoin)
- [x] OrbitCoin (balance, deposit, PayOS, confirm, CMS credit)
- [x] Community (rate map, report map, CMS resolve/dismiss reports)
- [x] Chat (private/temporary group, messages, SignalR)
- [x] Database Operations
- [x] Validation System
- [x] Exception Handling
- [x] Logging
- [x] Hangfire Integration
- [x] CORS Configuration
- [x] Swagger Documentation (chi tiết cho API)

### 🔧 Cần Cấu Hình
- [ ] Email Settings (SMTP)
- [ ] JWT Keys
- [ ] Database Connection Strings
- [ ] Google OAuth (nếu cần)
- [ ] Hangfire Database

### 🚧 Interface Sẵn, Chưa Implement
- [ ] SMS Service (NotificationChannelEnum.SMS)
- [ ] Firebase Service (NotificationChannelEnum.Firebase)
- [ ] Google OAuth endpoint

---

## 📁 CẤU TRÚC THƯ MỤC

```
src/
├── CapstoneProject.API/             # Web API Layer
│   ├── Controllers/
│   │   ├── Learner/               # Auth, Map, GameLobby, Gameplay, Marketplace, Community, Competitive, Chat, OrbitCoin
│   │   ├── Cms/                   # Auth, User, Map, Marketplace, Community, OrbitCoin
│   │   └── PayOSWebhookController # api/webhooks/payos
│   ├── Hubs/                      # GameLobbyHub, CompetitiveHub, ChatHub
│   ├── Middlewares/
│   ├── Configurations/
│   ├── Attributes/
│   └── Models/                    # API-specific (vd CreateMapFromJsonFileRequest với IFormFile)
│
├── CapstoneProject.Application/     # Application Layer
│   ├── Features/                   # CQRS: Commands & Queries
│   │   ├── Auth/
│   │   ├── User/
│   │   ├── Maps/
│   │   ├── Lobby/
│   │   ├── Gameplay/
│   │   ├── Competitive/
│   │   ├── Marketplace/
│   │   ├── Community/
│   │   ├── OrbitCoin/
│   │   └── Chat/
│   ├── Commons/
│   │   ├── DTOs/                  # Auth, User, Maps, Lobby, Gameplay, Competitive, Marketplace, Community, OrbitCoin
│   │   ├── Interfaces/
│   │   ├── Behaviors/
│   │   ├── Validators/
│   │   └── ...
│
├── CapstoneProject.Domain/          # Domain Layer
│   ├── Entities/
│   ├── Enums/
│   └── Common/
│
└── CapstoneProject.Infrastructure/   # Infrastructure Layer
    ├── Services/
    ├── Repositories/
    ├── Context/
    └── ...
```

---

## 🎉 KẾT LUẬN

Dự án base này cung cấp đầy đủ các chức năng cơ bản cho một hệ thống quản lý người dùng:
- ✅ Authentication & Authorization hoàn chỉnh
- ✅ User Management đầy đủ
- ✅ OTP System
- ✅ File Upload
- ✅ Email Service
- ✅ Clean Architecture với CQRS
- ✅ Validation & Error Handling
- ✅ Logging & Monitoring
- ✅ Background Jobs Support

**Sẵn sàng để mở rộng và phát triển các tính năng business cụ thể!**


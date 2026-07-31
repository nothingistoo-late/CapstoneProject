# .NET 8 Clean Architecture Base Project

Dự án base được xây dựng với .NET 8, tuân theo kiến trúc Clean Architecture với pattern CQRS. Tích hợp ASP.NET Core Identity và JWT, hỗ trợ seeding dữ liệu khởi tạo (roles, admin) và tự động áp dụng migrations trong môi trường Development.

## Kiến trúc

- **src/ChemistrySubjectBe.API**: Web API (.NET 8)
  - Cấu hình DI, middlewares, Swagger, CORS
  - Tự động Migrate/Seed trong môi trường Development
  - Hỗ trợ Hangfire cho background jobs
- **src/ChemistrySubjectBe.Application**: Application layer
  - Interfaces, Models, Query Builders, Features với CQRS pattern
  - Business logic và validation
- **src/ChemistrySubjectBe.Domain**: Domain layer
  - Entities, Enums, Base types (BaseEntity, IEntityLike)
  - Domain models và business rules
- **src/ChemistrySubjectBe.Infrastructure**: Infrastructure layer
  - EF Core DbContext + DI, Identity stores, SQL Server provider
  - Repositories với GenericRepository và UnitOfWork pattern
  - External services và integrations

## Yêu cầu hệ thống

- .NET 8 SDK
- SQL Server (local hoặc container)
- dotnet-ef CLI

Cài đặt dotnet-ef (nếu cần):
```bash
dotnet tool install --global dotnet-ef
```

## Cấu hình

**QUAN TRỌNG**: Copy file example settings và điều chỉnh theo môi trường của bạn:
```bash
cd src/ChemistrySubjectBe.API
copy appsettings.example.json appsettings.json
```

⚠️ **Lưu ý bảo mật**: 
- File `appsettings.json` KHÔNG được commit lên Git (đã được thêm vào .gitignore)
- File `appsettings.example.json` PHẢI được giữ lại làm template
- Luôn thay đổi các thông tin nhạy cảm trước khi chạy trong môi trường thực tế

### Các cấu hình quan trọng:

- **ConnectionStrings**:
  - `DefaultConnection`: Kết nối database chính
  - `OuterDbConnection`: Kết nối database cho Hangfire
- **Jwt**: Cấu hình JWT authentication
- **AdminUser**: Thông tin admin mặc định (Email, DefaultPassword)
- **DataSeeding**: `EnableSeeding` = true để seed roles + admin
- **Cors**: `AllowedOrigins` cho frontend URLs
- **EmailSettings**: Cấu hình SMTP cho gửi email
- **GoogleSettings**: OAuth Google authentication
- **Hangfire**: Cấu hình background jobs

## EF Core Migrations

Tạo migration mới (ví dụ: InitialBusiness):
```bash
dotnet ef migrations add InitialBusiness --project src/ChemistrySubjectBe.Infrastructure --startup-project src/ChemistrySubjectBe.API
```

Áp dụng migrations thủ công (tùy chọn):
```bash
dotnet ef database update --project src/ChemistrySubjectBe.Infrastructure --startup-project src/ChemistrySubjectBe.API
```

**Lưu ý**: Trong môi trường Development, API sẽ tự động áp dụng pending migrations khi khởi động (xem Program.cs).

## Chạy dự án

Chạy API:
```bash
cd src/ChemistrySubjectBe.API
dotnet run
```

Chế độ watch (rebuild khi có thay đổi):
```bash
cd src/ChemistrySubjectBe.API
dotnet watch run
```

Khi chạy trong môi trường Development, API sẽ:
- Thử kết nối lại database
- Áp dụng pending migrations
- Seed dữ liệu (roles từ RoleEnum, admin từ appsettings)
- Khởi động Hangfire dashboard

## Tính năng chính

### Authentication & Authorization
- ASP.NET Core Identity với custom AppUser/AppRole
- JWT Bearer authentication
- Google OAuth integration
- Role-based authorization
- OTP verification cho registration/password reset

### Background Jobs
- Hangfire integration với separate database
- Email sending jobs
- Configurable retry policies
- Dashboard monitoring

### Database Features
- Soft delete và audit tracking cho BaseEntity
- Identity entities implement IEntityLike để tái sử dụng filters/sorting
- DbContext pooling được bật
- Dual database support (main + hangfire)

## Các lệnh hữu ích

Lệnh tạo migrations (đã tạo sẵn 2 migration bên dưới)
//ChemistrySubjectDbContext (db chính)
dotnet ef migrations add InitialBusiness --project ChemistrySubjectBe.Infrastructure --startup-project ChemistrySubjectBe.API --context ChemistrySubjectDbContext

//ChemistrySubjectOuterDbContext (db cho hangfire)
dotnet ef migrations add InitialOuter --project ChemistrySubjectBe.Infrastructure --startup-project ChemistrySubjectBe.API --context ChemistrySubjectOuterDbContext

Liệt kê migrations:
```bash
dotnet ef migrations list --project src/ChemistrySubjectBe.Infrastructure --startup-project src/ChemistrySubjectBe.API
```

Xóa migration cuối (chưa áp dụng):
```bash
dotnet ef migrations remove --project src/ChemistrySubjectBe.Infrastructure --startup-project src/ChemistrySubjectBe.API
```

Tạo/cập nhật database thủ công:
```bash
dotnet ef database update --project src/ChemistrySubjectBe.Infrastructure --startup-project src/ChemistrySubjectBe.API
```

Build solution:
```bash
dotnet build ChemistrySubjectBe.sln
```

Restore packages:
```bash
dotnet restore ChemistrySubjectBe.sln
```

## Endpoints chính

- **Swagger UI**: `/swagger` (Development only)
- **Hangfire Dashboard**: `/hangfire` (Development: no auth required)
- **Health Check**: `/health`
- **API Base**: `/api`

## Xử lý sự cố

### SQL Server
- Kiểm tra `ConnectionStrings:DefaultConnection` và quyền user
- Với Docker, publish port 1433 và cân nhắc `TrustServerCertificate=True` khi dùng self-signed certificates

### Email Configuration
- Đảm bảo `EmailSettings` được cấu hình đúng
- Với Gmail, sử dụng App Password thay vì password thường
- Kiểm tra `EnableSsl` = true cho Gmail

### JWT Authentication
- Đảm bảo `Jwt:Key` đủ dài và bảo mật
- Kiểm tra `Jwt:Issuer` và `Jwt:Audience` khớp với client

### Google OAuth
- Cấu hình `GoogleSettings:ClientId` và `GoogleSettings:ClientSecret`
- Đảm bảo `RedirectUri` khớp với Google Console

### Hangfire
- Kiểm tra `OuterDbConnection` nếu `UseOuterDatabase` = true
- Dashboard có thể truy cập tại `/hangfire` trong Development

## Development Notes
Live Link: https://quackorbit.vercel.app/
Link github FE: https://github.com/vukt2004/SEP_FE

- File log được tạo trong thư mục `Logs/`
- CORS được cấu hình cho development ports (3000, 5173, 4200)
- Rate limiting được áp dụng cho OTP endpoints
- Password encryption sử dụng AES với key từ config

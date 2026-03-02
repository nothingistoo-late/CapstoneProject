# Tổng Quan Kiến Trúc Hệ Thống - ChemistrySubject Backend

## 📋 Mục Lục
1. [Kiến Trúc Tổng Quan](#kiến-trúc-tổng-quan)
2. [Vai Trò Từng Tầng (Layer)](#vai-trò-từng-tầng-layer)
3. [Cấu Trúc Thư Mục Chi Tiết](#cấu-trúc-thư-mục-chi-tiết)
4. [Các Class Quan Trọng Nhất](#các-class-quan-trọng-nhất)
5. [Core Flow - Luồng Xử Lý Request](#core-flow---luồng-xử-lý-request)

---

## 🏗️ Kiến Trúc Tổng Quan

Hệ thống được xây dựng theo **Clean Architecture** với pattern **CQRS** (Command Query Responsibility Segregation), sử dụng **MediatR** cho việc xử lý commands/queries và **Unit of Work** pattern cho data access.

```
┌─────────────────────────────────────────────────────────┐
│                    API Layer                             │
│  (Controllers, Middlewares, Attributes, Config)         │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│              Application Layer                           │
│  (Features/CQRS, DTOs, Validators, Behaviors)          │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                Domain Layer                              │
│  (Entities, Enums, Base Types)                          │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│           Infrastructure Layer                           │
│  (DbContext, Repositories, Services, EF Core)           │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                   Database                               │
│  (SQL Server - Main DB + Outer DB for Hangfire)         │
└─────────────────────────────────────────────────────────┘
```

---

## 🎯 Vai Trò Từng Tầng (Layer)

### 1. **ChemistrySubjectBe.API** - Presentation Layer

**Vai trò**: Entry point của hệ thống, xử lý HTTP requests/responses

**Trách nhiệm chính**:
- Nhận HTTP requests từ client
- Routing và controller logic
- Authentication & Authorization (JWT, Role-based)
- Exception handling global
- API documentation (Swagger)
- Middleware pipeline configuration

**Cấu trúc thư mục**:
```
API/
├── Controllers/          # API endpoints (Cms/, Student/)
├── Middlewares/          # Global exception, JWT, Validation
├── Attributes/           # Custom attributes (AuthorizeRoles)
├── Configurations/       # Swagger, CORS, JWT, Service config
├── Extensions/           # Application startup extensions
├── Injection/            # API-specific DI registration
└── Program.cs            # Entry point - rất minimal
```

---

### 2. **ChemistrySubjectBe.Application** - Application Layer

**Vai trò**: Business logic và orchestration layer

**Trách nhiệm chính**:
- Xử lý business logic thông qua Commands/Queries (CQRS)
- Validation với FluentValidation
- Mapping DTOs ↔ Entities (AutoMapper)
- Pipeline behaviors (Validation, Authorization, Performance)
- Business rules và domain logic orchestration

**Cấu trúc thư mục**:
```
Application/
├── Features/             # CQRS features theo module
│   ├── Auth/            # Authentication features
│   ├── Practice/        # Practice session features
│   ├── Exam/            # Exam features
│   └── ...              # Mỗi feature có Commands/Queries
├── Commons/
│   ├── DTOs/            # Data Transfer Objects (Request/Response)
│   ├── Validators/      # FluentValidation validators
│   ├── Behaviors/       # MediatR pipeline behaviors
│   ├── Mappings/        # AutoMapper profiles
│   ├── Interfaces/      # Application interfaces
│   └── Models/          # Result, Pagination models
└── ApplicationDependencyInjection.cs
```

---

### 3. **ChemistrySubjectBe.Domain** - Domain Layer

**Vai trò**: Core business entities và domain rules

**Trách nhiệm chính**:
- Định nghĩa Entities (pure domain models)
- Enums cho business values
- Base classes (BaseEntity, IEntityLike)
- Domain business rules (no dependencies)

**Cấu trúc thư mục**:
```
Domain/
├── Entities/            # Domain entities (AppUser, Question, Exam...)
├── Enums/               # Business enums (RoleEnum, StatusEnum...)
└── Common/              # BaseEntity, IEntityLike
```

**Đặc điểm**:
- ✅ **Không có dependencies** vào các layer khác
- ✅ Chứa pure business logic
- ✅ Entities kế thừa `BaseEntity` (audit fields, soft delete)

---

### 4. **ChemistrySubjectBe.Infrastructure** - Infrastructure Layer

**Vai trò**: Data access và external services

**Trách nhiệm chính**:
- EF Core DbContext configuration
- Repository pattern implementation (GenericRepository)
- Unit of Work pattern
- Identity & JWT services
- External services (Email, File Storage, OTP Cache)
- Database migrations

**Cấu trúc thư mục**:
```
Infrastructure/
├── Context/             # DbContext (ChemistrySubjectDbContext)
├── Repositories/        # GenericRepository, UnitOfWork
├── Services/            # Identity, JWT, Email, File services
├── Migrations/          # EF Core migrations
├── BackgroundServices/  # Hangfire jobs
└── InfrastructureDependencyInjection.cs
```

---

## 📁 Cấu Trúc Thư Mục Chi Tiết

### API Layer Structure

```
src/ChemistrySubjectBe.API/
├── Controllers/
│   ├── Cms/                    # CMS admin controllers
│   │   ├── AuthController.cs
│   │   ├── LessonController.cs
│   │   ├── QuestionController.cs
│   │   └── ...
│   └── Student/                # Student controllers
│       ├── AuthController.cs
│       ├── PracticeController.cs
│       ├── StudentAttemptsController.cs
│       └── ...
├── Middlewares/
│   ├── GlobalExceptionHandlingMiddleware.cs   # Global exception handler
│   ├── JwtMiddleware.cs                       # JWT authentication
│   └── ValidationMiddleware.cs
├── Attributes/
│   ├── AuthorizeRolesAttribute.cs             # Role-based authorization
│   └── RoleAccessFilter.cs
├── Configurations/
│   ├── ServiceConfiguration.cs                # Main service registration
│   ├── SwaggerConfiguration.cs
│   ├── CorsConfiguration.cs
│   └── JwtConfiguration.cs
└── Program.cs                                  # Entry point (minimal)
```

### Application Layer Structure

```
src/ChemistrySubjectBe.Application/
├── Features/                                   # CQRS Features
│   ├── Practice/
│   │   ├── Commands/
│   │   │   ├── SubmitPractice/
│   │   │   │   ├── SubmitPracticeCommand.cs           # Command (input)
│   │   │   │   ├── SubmitPracticeCommandHandler.cs    # Handler (logic)
│   │   │   │   └── SubmitPracticeCommandValidator.cs  # Validation
│   │   │   └── StartPractice/...
│   │   └── Queries/
│   │       └── GetPracticeResult/...
│   ├── Auth/                                   # Authentication features
│   ├── Exam/                                   # Exam management
│   └── ...
├── Commons/
│   ├── DTOs/                                   # Data Transfer Objects
│   │   ├── Practice/
│   │   │   ├── SubmitPracticeRequest.cs        # Request DTO
│   │   │   ├── PracticeResultResponse.cs       # Response DTO
│   │   │   └── PracticeAnswerRequest.cs
│   │   └── ...
│   ├── Validators/                             # FluentValidation
│   ├── Behaviors/                              # MediatR pipeline
│   │   ├── ValidationBehavior.cs               # Auto-validation
│   │   ├── AuthorizationBehavior.cs            # Auto-authorization
│   │   └── PerformanceBehavior.cs              # Performance logging
│   ├── Mappings/                               # AutoMapper profiles
│   └── Interfaces/                             # Application interfaces
│       ├── IUnitOfWork.cs
│       ├── IGenericRepository.cs
│       └── ICurrentUserService.cs
```

---

## 🔑 Các Class Quan Trọng Nhất

### 1. **BaseEntity** (Domain Layer)

```csharp
// src/ChemistrySubjectBe.Domain/Common/BaseEntity.cs
public class BaseEntity : IEntityLike
{
    public Guid Id { get; set; }
    public DateTime? CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;        // Soft delete
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public EntityStatusEnum Status { get; set; }
}
```

**Chức năng**:
- Base class cho tất cả entities
- Cung cấp audit fields (CreatedAt, UpdatedAt, CreatedBy...)
- Hỗ trợ soft delete (IsDeleted)
- Track entity status

**Sử dụng**: Tất cả domain entities kế thừa từ `BaseEntity`

---

### 2. **ChemistrySubjectDbContext** (Infrastructure Layer)

```csharp
// src/ChemistrySubjectBe.Infrastructure/Context/ChemistrySubjectDbContext.cs
public class ChemistrySubjectDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    // DbSets cho tất cả entities
    public DbSet<Question> Questions { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<PracticeSession> PracticeSessions { get; set; }
    // ...
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Entity configurations, relationships, indexes
    }
}
```

**Chức năng**:
- EF Core DbContext cho database operations
- Kế thừa `IdentityDbContext` cho authentication
- Cấu hình entity relationships, indexes
- DbSet definitions cho tất cả entities

---

### 3. **IGenericRepository<T>** (Application Interface)

```csharp
// src/ChemistrySubjectBe.Application/Commons/Interfaces/IGenericRepository.cs
public interface IGenericRepository<T> where T : class, IEntityLike
{
    Task AddAsync(T entity);
    Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, ...);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>>? predicate, ...);
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(...);
    void Update(T entity);
    void Delete(T entity);
    // ...
}
```

**Chức năng**:
- Generic repository interface với các CRUD operations cơ bản
- Hỗ trợ filtering, paging, includes
- Abstraction cho data access layer

**Implementation**: `GenericRepository<T>` trong Infrastructure layer

---

### 4. **IUnitOfWork** (Application Interface)

```csharp
// src/ChemistrySubjectBe.Application/Commons/Interfaces/IUnitOfWork.cs
public interface IUnitOfWork
{
    IGenericRepository<T> Repository<T>() where T : class, IEntityLike;
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

**Chức năng**:
- Quản lý repositories và database transactions
- Đảm bảo atomic operations (tất cả success hoặc rollback)
- Factory pattern để tạo repositories

**Implementation**: `UnitOfWork` trong Infrastructure layer

**Sử dụng trong Handler**:
```csharp
await _unitOfWork.BeginTransactionAsync(cancellationToken);
try
{
    // Multiple repository operations
    await _unitOfWork.Repository<Entity1>().AddAsync(...);
    _unitOfWork.Repository<Entity2>().Update(...);
    await _unitOfWork.CommitTransactionAsync(cancellationToken);
}
catch
{
    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
    throw;
}
```

---

### 5. **SubmitPracticeCommand** (CQRS Command)

```csharp
// src/ChemistrySubjectBe.Application/Features/Practice/Commands/SubmitPractice/SubmitPracticeCommand.cs
public record SubmitPracticeCommand(SubmitPracticeRequest Request) 
    : IRequest<Result<PracticeResultResponse>>;
```

**Chức năng**:
- Command object chứa input data cho operation
- Implement `IRequest<TResponse>` từ MediatR
- Immutable (record type)

---

### 6. **SubmitPracticeCommandHandler** (CQRS Handler)

```csharp
// src/ChemistrySubjectBe.Application/Features/Practice/Commands/SubmitPractice/SubmitPracticeCommandHandler.cs
public class SubmitPracticeCommandHandler 
    : IRequestHandler<SubmitPracticeCommand, Result<PracticeResultResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    
    public async Task<Result<PracticeResultResponse>> Handle(
        SubmitPracticeCommand command, 
        CancellationToken cancellationToken)
    {
        // 1. Validate user
        // 2. Get entities từ database
        // 3. Business logic processing
        // 4. Save changes (transaction)
        // 5. Return result
    }
}
```

**Chức năng**:
- Xử lý business logic cho command
- Sử dụng UnitOfWork để access repositories
- Transaction management
- Return Result<T> với success/error handling

---

### 7. **ValidationBehavior** (MediatR Pipeline)

```csharp
// src/ChemistrySubjectBe.Application/Commons/Behaviors/ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // 1. Find validators for request
        // 2. Validate request
        // 3. Throw exception if validation fails
        // 4. Continue to next handler if valid
        return await next();
    }
}
```

**Chức năng**:
- Tự động validate requests trước khi xử lý
- Sử dụng FluentValidation validators
- Chạy trong MediatR pipeline (trước handler)

---

### 8. **GlobalExceptionHandlingMiddleware** (API Middleware)

```csharp
// src/ChemistrySubjectBe.API/Middlewares/GlobalExceptionHandlingMiddleware.cs
public class GlobalExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Classify exception (Validation, Database, Unauthorized...)
            // Log exception
            // Return formatted error response
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

**Chức năng**:
- Bắt tất cả exceptions trong application
- Phân loại exceptions (Validation, Database, Unauthorized...)
- Trả về formatted error response với status code phù hợp
- Log exceptions (chỉ internal errors)

---

### 9. **PracticeController** (API Controller)

```csharp
// src/ChemistrySubjectBe.API/Controllers/Student/PracticeController.cs
[ApiController]
[AuthorizeRoles(nameof(RoleEnum.Student))]
[Route("api/student/practice")]
public class PracticeController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitPractice([FromBody] SubmitPracticeRequest request)
    {
        var command = new SubmitPracticeCommand(request);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
```

**Chức năng**:
- Nhận HTTP requests
- Map request thành Command
- Gửi Command qua MediatR
- Trả về HTTP response

---

### 10. **Program.cs** (Entry Point)

```csharp
// src/ChemistrySubjectBe.API/Program.cs
var builder = WebApplication.CreateBuilder(args)
    .ConfigureServices();  // Register all services

var app = builder.Build()
    .ConfigurePipeline();  // Configure middlewares

await app.ConfigureApplicationAsync();  // Migrations, seeding
app.Run();
```

**Chức năng**:
- Entry point của application
- Rất minimal - logic được tách vào extension methods
- Register services và configure pipeline

---

## 🔄 Core Flow - Luồng Xử Lý Request

### Ví dụ: Submit Practice Session

#### Step 1: HTTP Request đến API
```
POST /api/student/practice/submit
Authorization: Bearer <jwt_token>
Body: {
  "practiceSessionId": "...",
  "answers": [...]
}
```

#### Step 2: Middleware Pipeline

```
Request
  ↓
┌─────────────────────────────────┐
│ GlobalExceptionHandling         │  ← Catch tất cả exceptions
└─────────────────────────────────┘
  ↓
┌─────────────────────────────────┐
│ JwtMiddleware                   │  ← Validate JWT token
└─────────────────────────────────┘
  ↓
┌─────────────────────────────────┐
│ ValidationMiddleware            │  ← Model validation
└─────────────────────────────────┘
  ↓
┌─────────────────────────────────┐
│ Authentication                  │  ← ASP.NET Core Identity
└─────────────────────────────────┘
  ↓
┌─────────────────────────────────┐
│ Authorization                   │  ← Role-based (Student)
└─────────────────────────────────┘
  ↓
Controller
```

#### Step 3: Controller nhận Request

```csharp
// PracticeController.SubmitPractice()
[HttpPost("submit")]
public async Task<IActionResult> SubmitPractice([FromBody] SubmitPracticeRequest request)
{
    // Map HTTP request thành Command
    var command = new SubmitPracticeCommand(request);
    
    // Gửi Command qua MediatR
    var result = await _mediator.Send(command);
    
    // Trả về HTTP response
    return StatusCode(result.GetHttpStatusCode(), result);
}
```

#### Step 4: MediatR Pipeline Behaviors

```
SubmitPracticeCommand
  ↓
┌─────────────────────────────────┐
│ ValidationBehavior              │  ← Validate request với FluentValidation
│  - Tìm SubmitPracticeValidator  │
│  - Validate SubmitPracticeRequest│
│  - Throw nếu invalid            │
└─────────────────────────────────┘
  ↓
┌─────────────────────────────────┐
│ AuthorizationBehavior           │  ← Check permissions (nếu có)
└─────────────────────────────────┘
  ↓
┌─────────────────────────────────┐
│ PerformanceBehavior             │  ← Log performance metrics
└─────────────────────────────────┘
  ↓
Handler
```

#### Step 5: Command Handler xử lý Business Logic

```csharp
// SubmitPracticeCommandHandler.Handle()
public async Task<Result<PracticeResultResponse>> Handle(
    SubmitPracticeCommand command, 
    CancellationToken cancellationToken)
{
    // 1. VALIDATE USER
    var (isValid, userId) = await _currentUserService.IsUserValidAsync();
    if (!isValid) throw new UnauthorizedAccessException();
    
    // 2. GET ENTITIES FROM DATABASE
    var practiceSession = await _unitOfWork
        .Repository<PracticeSession>()
        .GetFirstOrDefaultAsync(
            ps => ps.Id == request.PracticeSessionId,
            ps => ps.Lesson!,
            ps => ps.StudentUser!
        );
    
    if (practiceSession == null)
        throw new KeyNotFoundException("Practice session not found");
    
    // 3. BUSINESS LOGIC
    var questions = await _unitOfWork
        .Repository<Question>()
        .FindAsync(q => questionIds.Contains(q.Id), q => q.Options);
    
    // 4. TRANSACTION - Save changes atomically
    await _unitOfWork.BeginTransactionAsync(cancellationToken);
    try
    {
        // Create practice answers
        foreach (var answerRequest in request.Answers)
        {
            var practiceAnswer = new PracticeAnswer { ... };
            await _unitOfWork.Repository<PracticeAnswer>().AddAsync(practiceAnswer);
        }
        
        // Update practice session
        practiceSession.IsCompleted = true;
        _unitOfWork.Repository<PracticeSession>().Update(practiceSession);
        
        // COMMIT TRANSACTION
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
        
        // 5. RETURN RESULT
        return Result<PracticeResultResponse>.Success(response);
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
        throw;
    }
}
```

#### Step 6: Repository Layer (GenericRepository)

```csharp
// GenericRepository<T> implementation
public async Task<T> GetFirstOrDefaultAsync(
    Expression<Func<T, bool>> predicate, 
    params Expression<Func<T, object>>[] includes)
{
    var query = _dbSet.AsQueryable();
    
    // Apply includes (eager loading)
    if (includes?.Any() == true)
    {
        query = includes.Aggregate(query, (current, include) => current.Include(include));
    }
    
    // Apply filter
    return await query.FirstOrDefaultAsync(predicate);
}
```

#### Step 7: EF Core DbContext thực hiện Database Query

```csharp
// EF Core translates LINQ to SQL
SELECT * FROM PracticeSessions ps
LEFT JOIN Lessons l ON ps.LessonId = l.Id
LEFT JOIN Users u ON ps.StudentUserId = u.Id
WHERE ps.Id = @practiceSessionId
```

#### Step 8: Database Response

```
Database trả về data
  ↓
EF Core map vào entities
  ↓
Repository trả về entities
  ↓
Handler xử lý business logic
  ↓
Handler trả về Result<PracticeResultResponse>
  ↓
MediatR trả về Result cho Controller
  ↓
Controller trả về HTTP Response
  ↓
Middleware xử lý response
  ↓
Client nhận response
```

---

## 🔗 Dependency Flow

```
API Layer
  ↓ depends on
Application Layer (Interfaces: IUnitOfWork, ICurrentUserService...)
  ↓ depends on
Domain Layer (Entities, Enums)
  ↑ implements
Infrastructure Layer (UnitOfWork, Services, Repositories, DbContext)
```

**Lưu ý quan trọng**:
- ✅ API chỉ phụ thuộc vào Application layer (interfaces)
- ✅ Application không phụ thuộc vào Infrastructure (chỉ interfaces)
- ✅ Domain layer không phụ thuộc vào gì cả (pure)
- ✅ Infrastructure implement các interfaces từ Application

---

## 📊 Data Flow Diagram

```
┌──────────────┐
│   Client     │
└──────┬───────┘
       │ HTTP Request
       ▼
┌──────────────────────────────────┐
│  PracticeController              │
│  - Nhận HTTP request             │
│  - Tạo Command                   │
│  - Gửi qua MediatR               │
└──────┬───────────────────────────┘
       │ MediatR.Send(command)
       ▼
┌──────────────────────────────────┐
│  MediatR Pipeline                │
│  1. ValidationBehavior           │
│  2. AuthorizationBehavior        │
│  3. PerformanceBehavior          │
└──────┬───────────────────────────┘
       │
       ▼
┌──────────────────────────────────┐
│  SubmitPracticeCommandHandler    │
│  - Business logic                │
│  - Sử dụng IUnitOfWork           │
└──────┬───────────────────────────┘
       │
       ▼
┌──────────────────────────────────┐
│  IUnitOfWork                     │
│  - Repository<PracticeSession>   │
│  - Repository<Question>          │
│  - Transaction management        │
└──────┬───────────────────────────┘
       │
       ▼
┌──────────────────────────────────┐
│  GenericRepository<T>            │
│  - CRUD operations               │
│  - Query building                │
└──────┬───────────────────────────┘
       │
       ▼
┌──────────────────────────────────┐
│  ChemistrySubjectDbContext       │
│  - EF Core DbContext             │
│  - LINQ to SQL translation       │
└──────┬───────────────────────────┘
       │ SQL Query
       ▼
┌──────────────────────────────────┐
│  SQL Server Database             │
└──────────────────────────────────┘
       │
       │ Data
       ▼
┌──────────────────────────────────┐
│  Entities (PracticeSession...)   │
│  ↓                               │
│  Result<PracticeResultResponse>  │
│  ↓                               │
│  HTTP Response                   │
└──────────────────────────────────┘
```

---

## 🎯 Tóm Tắt

### Luồng xử lý request hoàn chỉnh:

1. **HTTP Request** → Client gửi request đến API endpoint
2. **Middleware Pipeline** → Exception handling, JWT, Validation, Auth
3. **Controller** → Nhận request, tạo Command, gửi qua MediatR
4. **MediatR Behaviors** → Validation, Authorization, Performance logging
5. **Command Handler** → Business logic, sử dụng UnitOfWork
6. **UnitOfWork** → Quản lý repositories và transactions
7. **Repository** → Generic CRUD operations
8. **DbContext** → EF Core, translate LINQ → SQL
9. **Database** → SQL Server thực hiện query
10. **Response** → Data → Entities → Result → HTTP Response → Client

### Pattern sử dụng:

- ✅ **Clean Architecture** - Tách biệt layers rõ ràng
- ✅ **CQRS** - Commands cho write, Queries cho read
- ✅ **MediatR** - Mediator pattern cho Commands/Queries
- ✅ **Repository Pattern** - Abstraction cho data access
- ✅ **Unit of Work** - Transaction management
- ✅ **Pipeline Behaviors** - Cross-cutting concerns (validation, auth, logging)
- ✅ **Dependency Injection** - Loose coupling

---

*Document được tạo để giải thích chi tiết kiến trúc và flow của hệ thống ChemistrySubject Backend.*




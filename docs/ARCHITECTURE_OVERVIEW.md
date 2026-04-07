# Tong Quan Kien Truc He Thong - CapstoneProject Backend (QuackOrbit)

## Muc Luc
1. Kien truc tong quan
2. Vai tro tung tang
3. Cau truc thu muc
4. Luong xu ly request
5. Quy uoc response message

---

## Kien Truc Tong Quan

He thong duoc xay dung theo Clean Architecture + CQRS (MediatR), tach 4 tang chinh:

- API: HTTP endpoint, middleware, auth, swagger, SignalR hubs
- Application: command/query handlers, business orchestration, validation
- Domain: entities, enums, domain primitives
- Infrastructure: EF Core, repositories, identity services, external services

```text
Client (Web/Mobile)
        |
        v
API Layer (Controllers, Middlewares, Hubs)
        |
        v
Application Layer (CQRS Handlers + Behaviors)
        |
        v
Domain Layer (Entities + Rules)
        |
        v
Infrastructure Layer (DbContext, Repositories, Services)
        |
        v
PostgreSQL / SQL Server (theo cau hinh)
```

---

## Vai Tro Tung Tang

### 1. CapstoneProject.API

Trach nhiem:
- Dinh nghia endpoint cho 2 nhom route chinh: `api/cms/*`, `api/learner/*`
- Xac thuc va phan quyen bang JWT + role filters
- Chuan hoa loi dau vao va loi he thong thong qua middleware
- Cung cap realtime channels qua SignalR:
  - `/hubs/chat`
  - `/hubs/gamelobby`
  - `/hubs/competitive`

Thu muc quan trong:
- `Controllers/Cms`, `Controllers/Learner`
- `Middlewares`
- `Attributes`
- `Hubs`
- `Configurations`

### 2. CapstoneProject.Application

Trach nhiem:
- Xu ly nghiep vu qua CQRS handlers (`Features/*/Commands`, `Features/*/Queries`)
- Validation va cross-cutting behaviors
- Dinh nghia `Result` contract tra ve cho API
- To chuc theo module nghiep vu:
  - Auth, User, Maps, Gameplay, Lobby, Competitive, Marketplace, OrbitCoin, Community, Chat, Complaint, LearningPath, XP

Thu muc quan trong:
- `Features/*`
- `Commons/DTOs`
- `Commons/Interfaces`
- `Commons/Models/Result.cs`
- `Commons/Behaviors`

### 3. CapstoneProject.Domain

Trach nhiem:
- Chua entity va enum nghiep vu cot loi
- Khong phu thuoc vao API/Application/Infrastructure
- Dat cac kieu du lieu va trang thai chung cho business model

Thu muc quan trong:
- `Entities`
- `Enums`
- `Common`

### 4. CapstoneProject.Infrastructure

Trach nhiem:
- Trien khai data access voi EF Core
- Cung cap repository/unit-of-work va service phu tro
- Identity, auth support services, payment/support services, migrations

Thu muc quan trong:
- `Context`
- `Repositories`
- `Services`
- `Migrations`
- `Configurations`

---

## Cau Truc Thu Muc (Rut Gon)

```text
src/
  CapstoneProject.API/
    Controllers/
      Cms/
      Learner/
    Hubs/
    Middlewares/
    Attributes/
    Configurations/
    Program.cs

  CapstoneProject.Application/
    Features/
    Commons/
      DTOs/
      Interfaces/
      Models/
      Behaviors/

  CapstoneProject.Domain/
    Entities/
    Enums/
    Common/

  CapstoneProject.Infrastructure/
    Context/
    Repositories/
    Services/
    Migrations/
```

---

## Core Flow - Luong Xu Ly Request

1. Client goi endpoint REST hoac su kien SignalR.
2. API Controller/HUB nhan input, map request vao Command/Query.
3. MediatR dispatch den handler tuong ung trong Application.
4. Handler tuong tac voi repository/service de xu ly nghiep vu.
5. Ket qua tra ve theo `Result` (success/failure + message ro rang).
6. API tra HTTP response thong nhat cho frontend.

Cross-cutting:
- Validation middleware + pipeline behaviors
- Role access filters (`AuthorizeRolesAttribute`, `RoleAccessFilter`)
- Global exception handling

---

## Quy Uoc Response Message

- Tat ca `Result.Success(...)` phai truyen message cu the (khong dung message mac dinh chung chung).
- Message duoc uu tien tieng Viet de thong nhat voi tai lieu API response.
- Tai lieu thong diep duoc tong hop theo module tai:
  - `docs/API_RESPONSE_MESSAGES_BY_MODULE_VI.md`
- Cot `API Endpoint` trong tai lieu can uu tien endpoint/hub/global descriptor thay vi duong dan file code.

---

## Ghi Chu Van Hanh

- Neu bo sung endpoint moi, can cap nhat dong bo:
  - Controller/HUB + Swagger summary
  - Handler message (success/failure)
  - `docs/API_RESPONSE_MESSAGES_BY_MODULE_VI.md`
  - `docs/FEATURES_LIST.md` (neu la tinh nang moi)

# Tổ chức project theo ChemistrySubjectBe

Tài liệu mô tả cách tổ chức code BaseBECleanArchitecture (Capstone) đã được chỉnh lại để thống nhất với project mẫu **ChemistrySubjectBe**.

---

## 1. Application layer

### 1.1. Commons

- **DTOs** (`Application/Commons/DTOs/`): Tất cả request/response DTO theo từng domain.
  - **Lobby**: `CreateLobbyRoomRequest`, `CreateLobbyRoomResponse`, `JoinLobbyRoomRequest`, `JoinLobbyRoomResponse`, `SetRoomMapRequest`, `StartGameResponse`, `LobbyRoomDetailResponse`, `LobbyPlayerDto`, `LobbyRoomListItemDto`, `PlayerGameResult`, `PlayerRankingDto`, `SubmitGameResponse`.
- **OrbitCoin**: `OrbitCoinBalanceDto`, `CreateDepositOrderRequest`, `CreateDepositOrderResult`, `CreditOrbitCoinRequest`.
- **Gameplay**: `SubmissionSubmitRequest`, `ValidateSolutionResultDto`, `ValidateSolutionRequest`, `ProgressDashboardDto`.
- **Maps**: `CreateTagRequest`, `UpdateTagRequest` (+ các DTO Maps khác đã có). Request tạo map từ file JSON (`CreateMapFromJsonFileRequest`) nằm trong **API/Models** vì có `IFormFile`.
- **Community**: `RateMapRequest`, `ReportMapRequest`, `BatchReportsRequest`, `BatchReportResultDto`.
- **Competitive**: `CreateMatchRequest`, `JoinRoomRequest`, `JoinRoomResultDto`.
- **User**, **Auth**, **Marketplace**, **Chat**: các DTO tương ứng (đã có sẵn trong Commons).
- **Validators** (`Commons/Validators/`): Validator dùng chung cho request (vd: `CreateUserRequestValidator`, `UpdateProfileRequestValidator`).
- **Behaviors**: `ValidationBehavior`, `AuthorizationBehavior`, `PerformanceBehavior`, `UnhandledExceptionBehavior`.
- **Interfaces**: `IRoomManager`, `IUnitOfWork`, `IOrbitCoinService`, v.v.

### 1.2. Features

- Mỗi feature: **Commands** và **Queries** (thư mục số nhiều, thống nhất).
- Mỗi Command/Query: một thư mục riêng chứa:
  - `XxxCommand.cs` / `XxxQuery.cs`
  - `XxxCommandHandler.cs` / `XxxQueryHandler.cs`
  - (Tùy chọn) `XxxCommandValidator.cs` khi validate theo command.
- **Models** trong Features chỉ dùng cho **domain/in-memory models** (không phải DTO API), ví dụ:
  - **Lobby**: `LobbyRoom`, `LobbyPlayer`, `GameInstance`, `LobbyGameState` (dùng bởi `IRoomManager`/`RoomManager`).
- **Không** đặt request/response DTO trong Features; đặt trong **Commons/DTOs/{Domain}**.

---

## 2. Đã thực hiện (alignment với ChemistrySubjectBe)

1. **Lobby**
   - Tạo `Application/Commons/DTOs/Lobby/` và chuyển toàn bộ DTO lobby từ Controller + `Features/Lobby/Models` vào đây.
   - Xóa DTO trùng trong `Features/Lobby/Models` (chỉ giữ domain models: `LobbyRoom`, `LobbyPlayer`, `GameInstance`, `LobbyGameState`).
   - Cập nhật `IRoomManager`, `RoomManager`, `SubmitLobbySolutionCommand`/Handler, `GameLobbyController` dùng DTO từ `Commons.DTOs.Lobby`.

2. **OrbitCoin**
   - Chuyển `OrbitCoinBalanceDto` từ `Features/OrbitCoin/Queries/GetOrbitCoinBalance/` sang `Commons/DTOs/OrbitCoin/`.
   - Cập nhật Query, Handler và `LearnerOrbitCoinController` tham chiếu DTO mới.

3. **Controller**
   - Tất cả controller: không khai báo DTO/request/response inline; dùng DTO từ `Application.Commons.DTOs.{Domain}` hoặc `API.Models` (cho request có `IFormFile`).
   - **OrbitCoin**: `CreateDepositOrderRequest`, `CreditOrbitCoinRequest`, `CreateDepositOrderResult` từ Commons.DTOs.OrbitCoin.
   - **Maps (CMS/Learner)**: `CreateTagRequest`, `UpdateTagRequest` từ Commons.DTOs.Maps; `CreateMapFromJsonFileRequest` từ API.Models.
   - **Community**: `RateMapRequest`, `ReportMapRequest` (Learner); `BatchReportsRequest`, `BatchReportResultDto` (CMS) từ Commons.DTOs.Community.
   - **Competitive**: `CreateMatchRequest`, `JoinRoomRequest` từ Commons.DTOs.Competitive.

---

## 3. API & Controllers

- **Học viên**: `api/learner/*` — controllers trong `Controllers/Learner/` (vd: `GameLobbyController`, `LearnerOrbitCoinController`, `LearnerMapController`, …).
- **CMS**: `api/cms/*` — controllers trong `Controllers/Cms/`.
- **Webhook**: `api/webhooks/payos` — controller ở thư mục gốc Controllers.
- Controller mỏng: nhận request → tạo Command/Query → `_mediator.Send` → trả `StatusCode(result.GetHttpStatusCode(), result)`.

---

## 4. Refactor controller → Command/Query (đã làm)

- **GameLobbyController**: Toàn bộ logic đã chuyển sang Application:
  - `GetLobbyRoomsQuery`, `CreateLobbyRoomCommand`, `JoinLobbyRoomCommand`, `GetLobbyRoomQuery`, `StartLobbyGameCommand`, `EndLobbyGameCommand`, `LeaveLobbyRoomCommand`, `ToggleLobbyReadyCommand`, `SetLobbyRoomMapCommand`. Controller chỉ còn `_mediator.Send()` + `StatusCode(result.GetHttpStatusCode(), result)` (và broadcast SignalR cho SubmitSolution).
- **Learner/Cms MapController**: Endpoint tạo map từ file JSON dùng `CreateMapFromJsonFileCommand` (input: `CreateMapFromJsonFileInput`). Parse JSON/hints/tagIds nằm trong Handler. Controller chỉ đọc file → string, build input, Send.
- **Auth (Learner/Cms), Marketplace, Gameplay, OrbitCoin, Community, Competitive**: Đã dùng Command/Query từ trước; controller mỏng.

## 5. Gợi ý tiếp theo (khi mở rộng)

- Khi thêm feature mới: đặt request/response DTO trong `Commons/DTOs/{TênDomain}`.
- Validator cho request: có thể đặt trong `Commons/Validators` hoặc kèm Command trong Features (pattern giống ChemistrySubjectBe).
- Giữ nhất quán thư mục **Commands** / **Queries** (số nhiều) trong từng feature.

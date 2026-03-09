# Game Lobby & Room Management System

In-memory multiplayer lobby and room management (Gunny/GunBound style) using **ASP.NET Core**, **SignalR**, and **C#**. **All IDs are Guid** (RoomId, HostId, PlayerId).

---

## Luồng từ đầu tới cuối (tóm tắt)

1. **Vào lobby**
   - Client đăng nhập (JWT), gọi REST `GET /api/learner/lobby/rooms` hoặc kết nối SignalR `/hubs/gamelobby` → nhận **LobbyRoomList** (danh sách phòng: RoomId, RoomCode, HostId, số người, Status, IsLocked, SelectedMapId).

2. **Tạo phòng**
   - Host: REST `POST .../rooms` (body: maxPlayers, selectedMapId?) hoặc Hub `CreateRoom(maxPlayers, selectedMapId?)`. Server kiểm tra map tồn tại (nếu có mapId) → tạo room in-memory, trả **RoomId** + **RoomCode**. Host tự động join phòng.

3. **Vào phòng**
   - Người chơi: REST `POST .../rooms/join` (body: **roomId** hoặc **roomCode** — chỉ cần một) hoặc Hub `JoinRoom(roomId)` / `JoinRoomByCode(roomCode)`. Server validate phòng Waiting, chưa đủ người, đúng code nếu lock → join, broadcast **RoomUpdated**. Nếu join bằng REST thì sau đó cần connect SignalR và gọi `JoinRoom(roomId)` để nhận real-time.

4. **Trong phòng (Waiting)**
   - Host có thể: **SetRoomMap** (chọn map, server check map tồn tại), **SetRoomLocked**, **KickPlayer**.
   - Mọi người: **ToggleReady**. Khi đủ ≥2 người và **tất cả ready**, host gọi **StartGame**.

5. **Start game**
   - Host: REST `POST .../rooms/{roomId}/start` hoặc Hub `StartGame(roomId)`. Server kiểm tra map (nếu phòng đã chọn map), tạo **GameInstance** (MapId, TurnOrder, GameState), đặt room **Status = Playing**, broadcast **GameStarted** (MapId, TurnOrder, GameState). Client load map theo MapId, hiển thị lượt chơi.

6. **Chơi & nộp bài**
   - Từng người chơi gửi bài: REST `POST .../rooms/{roomId}/submit` (body: astSpec, bytecodeSpec, language) hoặc Hub `SubmitSolution(roomId, astSpec, bytecodeSpec, language)`.
   - Server: lấy MapId từ room/game → gọi **ValidateSolutionCommand** (kiểm tra ast+bytecode, tạo **Submission** entity, lưu DB; tính score, status) → **RecordSubmission** vào GameInstance.PlayerResults.
   - Response: **SubmissionId**, **Status** (string "Accepted"|"WrongAnswer"), **Score**, Stars, StepsUsed, BlocksUsed. Khi **tất cả** trong game đã submit → server tính **ranking** (xếp hạng theo điểm), trả trong response và broadcast **RankingUpdated** (PlayerId, Score, Rank, Status) cho cả phòng.

7. **Kết thúc ván**
   - Bất kỳ ai trong phòng: REST `POST .../rooms/{roomId}/end` hoặc Hub `EndGame(roomId)` → server xóa GameInstance, room **Status = Waiting**, mọi người **unready**. Broadcast **GameEnded**. **Phòng vẫn tồn tại** — có thể chọn map khác, ready, Start lại. Chỉ khi **tất cả leave** thì phòng mới bị xóa.

8. **Leave**
   - REST `POST .../rooms/{roomId}/leave` hoặc Hub `LeaveRoom(roomId)`. Nếu host leave lúc Waiting → host migration. Nếu không còn ai → xóa phòng.

---

## Overview

- **Lobby**: Players connect to the lobby and see a list of rooms.
- **Rooms**: Create, join by ID or code, wait for players, toggle ready, start game.
- **In-memory**: No database; rooms and game instances live in process (suitable for matchmaking and short-lived sessions).

## Architecture

| Layer | Components |
|-------|------------|
| **Application** | **Commons/DTOs/Lobby**: request/response DTOs (`CreateLobbyRoomRequest`, `JoinLobbyRoomRequest`, `LobbyRoomDetailResponse`, `SubmitGameResponse`, `PlayerRankingDto`, …). **Features/Lobby/Models**: in-memory domain models `LobbyRoom`, `LobbyPlayer`, `GameInstance`, `LobbyGameState`. **Commons/Interfaces**: `IRoomManager`. Domain: `RoomStatusEnum` (Waiting, Playing); all IDs are **Guid**. |
| **Infrastructure** | `RoomManager` (singleton, thread-safe `ConcurrentDictionary`) |
| **API** | `GameLobbyController` (REST), `GameLobbyHub` (SignalR) |

## REST API (api/learner/lobby)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/learner/lobby/rooms` | List all lobby rooms |
| POST | `/api/learner/lobby/rooms` | Create room (body: `{ "maxPlayers": 8 }`), returns `RoomId` (Guid), `RoomCode` |
| POST | `/api/learner/lobby/rooms/join` | Join by **RoomId** or **RoomCode** — chỉ cần gửi **một trong hai** (e.g. `{ "roomCode": "ABC123" }` hoặc `{ "roomId": "guid" }`). |
| GET | `/api/learner/lobby/rooms/{roomId}` | Get room detail by Guid |
| POST | `/api/learner/lobby/rooms/{roomId}/start` | Start game (host only; ≥2 players, all ready). Returns session: MapId, TurnOrder, GameState. |
| POST | `/api/learner/lobby/rooms/{roomId}/end` | End game (any player in room). Room back to Waiting, all unready. |
| POST | `/api/learner/lobby/rooms/{roomId}/leave` | Leave room. |
| POST | `/api/learner/lobby/rooms/{roomId}/ready` | Toggle ready. |
| POST | `/api/learner/lobby/rooms/{roomId}/map` | Set selected map (host only). Body: `{ "mapId": "guid" }`. |
| POST | `/api/learner/lobby/rooms/{roomId}/submit` | Submit solution for current game. Body: `{ "astSpec", "bytecodeSpec", "language" }`. Room must be Playing and have a map. Returns score, status, submissionId; when all players have submitted, response includes `rankingIfAllSubmitted` (and server broadcasts `RankingUpdated` via SignalR). |

Auth: Bearer token (Learner, Admin, Moderator). Response format: `Result<T>` (isSuccess, message, data, errorCode).

## SignalR Hub

- **URL**: `https://<host>/hubs/gamelobby`
- **Auth**: `[Authorize]` — JWT required (query `access_token` for WebSocket).
- **IDs**: Client sends Guid for roomId/playerId (e.g. JSON string or Guid).

## SignalR Client Methods (server invokes from client)

| Method | Parameters | Description |
|--------|------------|--------------|
| `CreateRoom` | `maxPlayers` (optional, default 8) | Create room; caller becomes host. |
| `JoinRoom` | `roomId`, `roomCode` (optional, required if room locked) | Join by room ID (e.g. from lobby list). |
| `JoinRoomByCode` | `roomCode` | Join by 6-character room code only. |
| `LeaveRoom` | `roomId` | Leave room; host migration if host leaves (waiting only). |
| `ToggleReady` | `roomId` | Toggle ready state in the room. |
| `StartGame` | `roomId` | Start match (host only; ≥2 players, all ready). |
| `KickPlayer` | `roomId`, `targetPlayerId` | Kick player (host only, waiting only). |
| `SetRoomLocked` | `roomId`, `isLocked` | Lock/unlock room (host only). |
| `SetSelectedMap` | `roomId`, `mapId` (optional) | Set map for room (host only). |
| `SubmitSolution` | `roomId`, `astSpec`, `bytecodeSpec`, `language` (optional) | Submit solution for current game. Server validates with room map, records score; when all have submitted, server broadcasts `RankingUpdated` to the room. |
| `EndGame` | `roomId` | End current game (any player in room). |
| `GetLobbyRooms` | — | Request current lobby room list. |

## Server → Client Events

| Event | When |
|-------|------|
| `LobbyRoomList` | On connect; when any room is created/updated/removed. Payload: list of `{ RoomId, RoomCode, HostId, CurrentPlayerCount, MaxPlayers, Status, IsLocked }`. |
| `RoomCreated` | After `CreateRoom` success. Payload: full room DTO. |
| `JoinedRoom` | After `JoinRoom` / `JoinRoomByCode` success. Payload: full room DTO. |
| `LeftRoom` | After `LeaveRoom`. Payload: `roomId`. |
| `RoomUpdated` | Room state changed (player joined/left, ready toggled, lock, kick). Payload: full room DTO. |
| `GameStarted` | After `StartGame` success. Payload: `{ RoomId, RoomCode, MapId, Players, TurnOrder, GameState: { CurrentTurnIndex, CurrentPlayerId, RoundNumber }, StartedAt }`. Client: load map by MapId, show turn order, set current player. |
| `GameEnded` | After `EndGame`. Payload: `{ RoomId }`. Room is back to Waiting; client can show lobby again. |
| `SubmissionResult` | After `SubmitSolution` (Hub). Payload: `{ Success, Score?, Status?, SubmissionId?, Message? }`. |
| `RankingUpdated` | When all players in the game have submitted (Hub and REST). Payload: list of `{ PlayerId, Score, Rank, Status }` ordered by score desc, then by SubmittedAt. Client can show ranking then call `EndGame` to return to lobby. |
| `KickedFromRoom` | When host kicks you. Payload: `{ RoomId }`. |
| `Error` | Validation or operation failure. Payload: error message string. |

## Alignment with Domain (no duplication)

- **Status**: Lobby uses Domain `RoomStatusEnum` (Waiting, Playing only). No separate `LobbyRoomStatus`.
- **Room**: Domain `Room` has `Code`, `RoomStatus`, `MaxPlayers`; lobby `LobbyRoom` uses `RoomCode` (= Code), `Status` (= RoomStatusEnum), same semantics.
- **Player**: Domain `RoomParticipant` has `IsOwner`, `IsReady`, `UserId`; lobby `LobbyPlayer` uses `IsHost` (= IsOwner), `IsReady`, `PlayerId` (= UserId string for SignalR). Lobby adds `ConnectionId` for realtime only.

## Room Model

- **RoomId**: **Guid** (unique).
- **RoomCode**: 6-character string (e.g. `AB12CD`) for quick join; same concept as Domain `Room.Code`.
- **HostId**, **Players**, **MaxPlayers**, **Status** (`RoomStatusEnum`: `Waiting` | `Playing`), **IsLocked** (lobby-only).
- Only **Waiting** rooms are joinable; **Playing** rooms do not accept new players.
- **Map tồn tại:** Khi tạo phòng có `selectedMapId`, khi đặt map cho phòng (`SetRoomMap` / `SetSelectedMap`), hoặc khi **Start game** (phòng đã chọn map), server kiểm tra map tồn tại và chưa bị xóa (DB). Nếu không tồn tại → REST trả 404, SignalR gửi event `Error`.

## Start Game Rules

- Room status must be **Waiting**.
- Caller must be **host**.
- At least **2 players**.
- **All players** must be **ready**.

When started: `Room.Status = Playing`, a `GameInstance` is created with `RoomId`, `RoomCode`, `MapId` (from room’s SelectedMapId), `Players`, `TurnOrder`, and initial `GameState` (`LobbyGameState`: `CurrentTurnIndex = 0`, `CurrentPlayerId = TurnOrder[0]`, `RoundNumber = 1`). The hub broadcasts `GameStarted` to the room.

**Client khi nhận GameStarted:** load map theo `MapId` (gọi API map hoặc cache), hiển thị thứ tự lượt `TurnOrder`, set lượt hiện tại theo `GameState.CurrentPlayerId`, bắt đầu vòng chơi (turn-based thì chỉ `CurrentPlayerId` mới gửi action; sau mỗi turn có thể gọi API/hub để server cập nhật `GameState` và broadcast `TurnChanged` nếu bạn thêm sau).

## Submit solution & ranking

- **Khi đang chơi (room Status = Playing):** Mỗi người chơi gửi bài giải qua **REST** `POST .../rooms/{roomId}/submit` (body: `astSpec`, `bytecodeSpec`, `language`) hoặc **SignalR** `SubmitSolution(roomId, astSpec, bytecodeSpec, language)`.
- **Server:** Lấy **MapId** từ room/game (bắt buộc room đã chọn map). Gọi logic validate bài giải giống single-player (`ValidateSolutionCommand`), lưu kết quả (score, status, submissionId) vào `GameInstance.PlayerResults` qua `RecordSubmission`.
- **Đánh giá (placeholder):** Hiện tại chỉ kiểm tra có đủ `astSpec` + `bytecodeSpec` (mỗi cái ≥ 2 ký tự), không chạy engine thật. Accepted → có điểm (100 trừ theo steps); thiếu input hoặc rác → **WrongAnswer**, 0 điểm. Khi tích hợp engine/block interpreter thật thì thay logic trong `ValidateSolutionCommandHandler`.
- **Response status dạng chữ:** Trong response (và `RankingUpdated`), **Status** luôn là **string** (vd. `"Accepted"`, `"WrongAnswer"`), không trả số enum để client dễ hiểu.
- **Khi tất cả người chơi trong game đã submit:** Server tính **xếp hạng** (theo điểm giảm dần, cùng điểm thì theo thời gian submit), trả về trong response (REST) và broadcast event **`RankingUpdated`** (payload: danh sách `PlayerId`, `Score`, `Rank`, `Status`) cho cả phòng qua SignalR.
- **Client:** Hiển thị bảng xếp hạng khi nhận `RankingUpdated` (hoặc từ `rankingIfAllSubmitted` trong response submit). Sau khi xem xong có thể gọi **EndGame** để kết thúc game và quay lại chờ phòng.

## Khi kết thúc game (End Game)

- Bất kỳ người chơi nào trong phòng gọi **EndGame** (REST `POST .../rooms/{roomId}/end` hoặc SignalR `EndGame(roomId)`).
- Server: xóa `GameInstance`, đặt `Room.Status = Waiting`, set tất cả player `IsReady = false`.
- **Phòng vẫn tồn tại:** Room không bị xóa; mọi người vẫn ở trong phòng, có thể chọn map khác, ready lại rồi Start game tiếp. Chỉ khi **tất cả đã leave** (phòng trống) thì room mới bị remove khỏi lobby.
- Broadcast `GameEnded` cho phòng và cập nhật `LobbyRoomList`.
- Client: nhận `GameEnded` → quay lại màn hình chờ trong phòng (có thể chọn map lại, ready, start lại).

## Host Migration

If the host leaves while the room is **Waiting**, the next player (by `PlayerId` order) becomes the new host; `RoomUpdated` is broadcast.

## Locked Rooms

When **IsLocked** is true, the room still appears in the lobby, but joining is only allowed with the correct **RoomCode** (via `JoinRoom(roomId, roomCode)` or `JoinRoomByCode(roomCode)`).

## Thread Safety

- `RoomManager` uses `ConcurrentDictionary` for rooms, game instances, and room-code lookup.
- Single shared `RoomManager` instance (singleton); safe for concurrent SignalR connections.

## API đổi status map (không thuộc lobby)

- **Learner (tác giả map):** `POST api/learner/maps/{id}/submit-review` — Submit map để duyệt (Draft → PendingReview). Controller: `Learner/MapController`.
- **CMS (Admin/Moderator):** `POST api/cms/maps/{id}/approve`, `POST api/cms/maps/{id}/publish` — Duyệt (PendingReview → Approved) và Publish (Approved → Published). Batch: `BatchApproveMaps`, `BatchPublishMaps`. Controller: `Cms/MapController`.

## Optional Next Steps

- Persist finished games or high scores via existing Application/Infrastructure layers.
- Add reconnection handling (e.g. map connection id to user/room and rejoin).
- Extend `GameInstance.GameState` for turn-based gameplay (e.g. GunBound rounds).

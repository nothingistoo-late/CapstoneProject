using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Lobby.Models;

namespace CapstoneProject.Application.Features.Lobby.Commands.JoinLobbyRoom;

public class JoinLobbyRoomCommandHandler : IRequestHandler<JoinLobbyRoomCommand, Result<JoinLobbyRoomResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoomManager _roomManager;

    public JoinLobbyRoomCommandHandler(ICurrentUserService currentUserService, IRoomManager roomManager)
    {
        _currentUserService = currentUserService;
        _roomManager = roomManager;
    }

    public async Task<Result<JoinLobbyRoomResponse>> Handle(JoinLobbyRoomCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<JoinLobbyRoomResponse>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var request = command.Request;
        LobbyRoom? room;
        if (request.RoomId.HasValue && request.RoomId != Guid.Empty)
        {
            room = _roomManager.GetRoomById(request.RoomId.Value);
            if (room == null)
                return Result<JoinLobbyRoomResponse>.Failure("Không tìm thấy phòng.", ErrorCodeEnum.NotFound);
        }
        else if (!string.IsNullOrWhiteSpace(request.RoomCode))
        {
            room = _roomManager.GetRoomByCode(request.RoomCode.Trim());
            if (room == null)
                return Result<JoinLobbyRoomResponse>.Failure("Không tìm thấy phòng.", ErrorCodeEnum.NotFound);
        }
        else
            return Result<JoinLobbyRoomResponse>.Failure("Cung cấp RoomId hoặc RoomCode.", ErrorCodeEnum.ValidationFailed);

        var (success, errorMessage, updatedRoom) = _roomManager.JoinRoom(room.RoomId, userIdNullable.Value, "", request.RoomCode);
        if (!success || updatedRoom == null)
        {
            var code = errorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true ? ErrorCodeEnum.NotFound : ErrorCodeEnum.ValidationFailed;
            return Result<JoinLobbyRoomResponse>.Failure(errorMessage ?? "Không thể tham gia.", code);
        }

        return Result<JoinLobbyRoomResponse>.Success(new JoinLobbyRoomResponse
        {
            RoomId = updatedRoom.RoomId,
            RoomCode = updatedRoom.RoomCode,
            CurrentPlayerCount = updatedRoom.PlayerCount,
            MaxPlayers = updatedRoom.MaxPlayers
        }, "Đã tham gia phòng.");
    }
}

using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Application.Commons.Interfaces;
using MediatR;

namespace CapstoneProject.Application.Features.Lobby.Commands.SetLobbyRoomLock;

public class SetLobbyRoomLockCommandHandler : IRequestHandler<SetLobbyRoomLockCommand, Result<LobbyRoomDetailResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoomManager _roomManager;

    public SetLobbyRoomLockCommandHandler(ICurrentUserService currentUserService, IRoomManager roomManager)
    {
        _currentUserService = currentUserService;
        _roomManager = roomManager;
    }

    public async Task<Result<LobbyRoomDetailResponse>> Handle(SetLobbyRoomLockCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<LobbyRoomDetailResponse>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var (success, errorMessage, room) = _roomManager.SetRoomLocked(command.RoomId, userIdNullable.Value, command.IsLocked);
        if (!success || room == null)
        {
            var code = errorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                ? ErrorCodeEnum.NotFound
                : ErrorCodeEnum.ValidationFailed;
            return Result<LobbyRoomDetailResponse>.Failure(errorMessage ?? "Không thể cập nhật trạng thái riêng tư của phòng.", code);
        }

        var response = new LobbyRoomDetailResponse
        {
            RoomId = room.RoomId,
            RoomCode = room.RoomCode,
            HostId = room.HostId,
            CurrentPlayerCount = room.PlayerCount,
            MaxPlayers = room.MaxPlayers,
            Status = room.Status,
            IsLocked = room.IsLocked,
            SelectedGameId = room.SelectedGameId,
            Players = room.Players.Values.Select(p => new LobbyPlayerDto
            {
                PlayerId = p.PlayerId,
                IsReady = p.IsReady,
                IsHost = p.IsHost
            }).ToList()
        };

        return Result<LobbyRoomDetailResponse>.Success(
            response,
            command.IsLocked ? "Phòng đã được đặt thành riêng tư." : "Phòng đã được mở công khai.");
    }
}

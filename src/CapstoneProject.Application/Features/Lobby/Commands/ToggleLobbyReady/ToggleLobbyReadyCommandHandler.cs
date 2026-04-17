using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Lobby.Models;

namespace CapstoneProject.Application.Features.Lobby.Commands.ToggleLobbyReady;

public class ToggleLobbyReadyCommandHandler : IRequestHandler<ToggleLobbyReadyCommand, Result<LobbyRoomDetailResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoomManager _roomManager;

    public ToggleLobbyReadyCommandHandler(ICurrentUserService currentUserService, IRoomManager roomManager)
    {
        _currentUserService = currentUserService;
        _roomManager = roomManager;
    }

    public async Task<Result<LobbyRoomDetailResponse>> Handle(ToggleLobbyReadyCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<LobbyRoomDetailResponse>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var (success, errorMessage, room) = _roomManager.ToggleReady(command.RoomId, userIdNullable.Value);
        if (!success || room == null)
        {
            var code = errorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true ? ErrorCodeEnum.NotFound : ErrorCodeEnum.ValidationFailed;
            return Result<LobbyRoomDetailResponse>.Failure(errorMessage ?? "Không thể chuyển đổi sẵn sàng.", code);
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
            Players = room.Players.Values.Select(p => new LobbyPlayerDto { PlayerId = p.PlayerId, IsReady = p.IsReady, IsHost = p.IsHost }).ToList()
        };
        return Result<LobbyRoomDetailResponse>.Success(response, "Đã cập nhật trạng thái sẵn sàng.");
    }
}

using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Lobby.Models;

namespace CapstoneProject.Application.Features.Lobby.Commands.EndLobbyGame;

public class EndLobbyGameCommandHandler : IRequestHandler<EndLobbyGameCommand, Result<LobbyRoomDetailResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoomManager _roomManager;

    public EndLobbyGameCommandHandler(ICurrentUserService currentUserService, IRoomManager roomManager)
    {
        _currentUserService = currentUserService;
        _roomManager = roomManager;
    }

    public async Task<Result<LobbyRoomDetailResponse>> Handle(EndLobbyGameCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<LobbyRoomDetailResponse>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var (success, errorMessage, room) = _roomManager.EndGame(command.RoomId, userIdNullable.Value);
        if (!success || room == null)
        {
            var code = errorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true ? ErrorCodeEnum.NotFound : ErrorCodeEnum.ValidationFailed;
            return Result<LobbyRoomDetailResponse>.Failure(errorMessage ?? "Could not end game.", code);
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
            SelectedMapId = room.SelectedMapId,
            Players = room.Players.Values.Select(p => new LobbyPlayerDto { PlayerId = p.PlayerId, IsReady = p.IsReady, IsHost = p.IsHost }).ToList()
        };
        return Result<LobbyRoomDetailResponse>.Success(response, "Game ended. Room is waiting for next start.");
    }
}

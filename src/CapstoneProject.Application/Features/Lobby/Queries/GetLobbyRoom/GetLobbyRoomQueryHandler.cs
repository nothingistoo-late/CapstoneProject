using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Lobby.Models;

namespace CapstoneProject.Application.Features.Lobby.Queries.GetLobbyRoom;

public class GetLobbyRoomQueryHandler : IRequestHandler<GetLobbyRoomQuery, Result<LobbyRoomDetailResponse>>
{
    private readonly IRoomManager _roomManager;

    public GetLobbyRoomQueryHandler(IRoomManager roomManager) => _roomManager = roomManager;

    public Task<Result<LobbyRoomDetailResponse>> Handle(GetLobbyRoomQuery request, CancellationToken cancellationToken)
    {
        var room = _roomManager.GetRoomById(request.RoomId);
        if (room == null)
            return Task.FromResult(Result<LobbyRoomDetailResponse>.Failure("Không tìm thấy phòng.", ErrorCodeEnum.NotFound));

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
        return Task.FromResult(Result<LobbyRoomDetailResponse>.Success(response));
    }
}

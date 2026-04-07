using MediatR;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Application.Commons.Interfaces;

namespace CapstoneProject.Application.Features.Lobby.Queries.GetLobbyRooms;

public class GetLobbyRoomsQueryHandler : IRequestHandler<GetLobbyRoomsQuery, Result<IReadOnlyList<LobbyRoomListItemDto>>>
{
    private readonly IRoomManager _roomManager;

    public GetLobbyRoomsQueryHandler(IRoomManager roomManager) => _roomManager = roomManager;

    public Task<Result<IReadOnlyList<LobbyRoomListItemDto>>> Handle(GetLobbyRoomsQuery request, CancellationToken cancellationToken)
    {
        var rooms = _roomManager.GetLobbyRooms();
        return Task.FromResult(Result<IReadOnlyList<LobbyRoomListItemDto>>.Success(rooms, "Đã lấy danh sách phòng chờ."));
    }
}


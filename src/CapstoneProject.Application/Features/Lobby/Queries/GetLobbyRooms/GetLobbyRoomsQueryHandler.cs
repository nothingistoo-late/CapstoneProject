using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Lobby.Queries.GetLobbyRooms;

public class GetLobbyRoomsQueryHandler : IRequestHandler<GetLobbyRoomsQuery, Result<IReadOnlyList<LobbyRoomListItemDto>>>
{
    private readonly IRoomManager _roomManager;
    private readonly IUnitOfWork _unitOfWork;

    public GetLobbyRoomsQueryHandler(IRoomManager roomManager, IUnitOfWork unitOfWork)
    {
        _roomManager = roomManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<LobbyRoomListItemDto>>> Handle(GetLobbyRoomsQuery request, CancellationToken cancellationToken)
    {
        var rooms = _roomManager.GetLobbyRooms().ToList();
        if (rooms.Count == 0)
            return Result<IReadOnlyList<LobbyRoomListItemDto>>.Success(rooms, "Đã lấy danh sách phòng chờ.");

        var hostIds = rooms.Select(r => r.HostId).Distinct().ToList();
        var gameIds = rooms.Where(r => r.SelectedGameId.HasValue).Select(r => r.SelectedGameId!.Value).Distinct().ToList();

        var userDict = await _unitOfWork.Repository<AppUser>().GetQueryable()
            .Where(u => hostIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.UserName })
            .ToDictionaryAsync(
                u => u.Id,
                u =>
                {
                    var fullName = $"{u.FirstName} {u.LastName}".Trim();
                    return string.IsNullOrWhiteSpace(fullName)
                        ? (string.IsNullOrWhiteSpace(u.UserName) ? u.Id.ToString() : u.UserName!)
                        : fullName;
                },
                cancellationToken);

        var gameDict = await _unitOfWork.Repository<Game>().GetQueryable()
            .Where(g => gameIds.Contains(g.Id))
            .Select(g => new { g.Id, g.Title })
            .ToDictionaryAsync(g => g.Id, g => g.Title, cancellationToken);

        foreach (var room in rooms)
        {
            if (userDict.TryGetValue(room.HostId, out var hostName))
                room.HostName = hostName;
            if (room.SelectedGameId.HasValue && gameDict.TryGetValue(room.SelectedGameId.Value, out var gameTitle))
                room.SelectedGameTitle = gameTitle;
        }

        return Result<IReadOnlyList<LobbyRoomListItemDto>>.Success(rooms, "Đã lấy danh sách phòng chờ.");
    }
}


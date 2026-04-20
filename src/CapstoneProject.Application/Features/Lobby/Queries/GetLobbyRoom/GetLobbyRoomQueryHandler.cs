using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Lobby.Models;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Lobby.Queries.GetLobbyRoom;

public class GetLobbyRoomQueryHandler : IRequestHandler<GetLobbyRoomQuery, Result<LobbyRoomDetailResponse>>
{
    private readonly IRoomManager _roomManager;
    private readonly IUnitOfWork _unitOfWork;

    public GetLobbyRoomQueryHandler(IRoomManager roomManager, IUnitOfWork unitOfWork)
    {
        _roomManager = roomManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LobbyRoomDetailResponse>> Handle(GetLobbyRoomQuery request, CancellationToken cancellationToken)
    {
        var room = _roomManager.GetRoomById(request.RoomId);
        if (room == null)
            return Result<LobbyRoomDetailResponse>.Failure("Không tìm thấy phòng.", ErrorCodeEnum.NotFound);

        var playerIds = room.Players.Keys.ToList();
        var playerNames = await _unitOfWork.Repository<AppUser>().GetQueryable()
            .Where(u => playerIds.Contains(u.Id))
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
                PlayerName = playerNames.TryGetValue(p.PlayerId, out var name) ? name : p.PlayerId.ToString(),
                IsReady = p.IsReady,
                IsHost = p.IsHost
            }).ToList()
        };
        return Result<LobbyRoomDetailResponse>.Success(response, "Đã lấy thông tin phòng.");
    }
}

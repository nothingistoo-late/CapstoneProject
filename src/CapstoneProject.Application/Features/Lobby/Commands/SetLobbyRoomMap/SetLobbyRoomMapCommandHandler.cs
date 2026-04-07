using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Lobby.Models;
using CapstoneProject.Application.Features.Maps.Queries.MapExists;

namespace CapstoneProject.Application.Features.Lobby.Commands.SetLobbyRoomMap;

public class SetLobbyRoomMapCommandHandler : IRequestHandler<SetLobbyRoomMapCommand, Result<LobbyRoomDetailResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoomManager _roomManager;
    private readonly IMediator _mediator;

    public SetLobbyRoomMapCommandHandler(ICurrentUserService currentUserService, IRoomManager roomManager, IMediator mediator)
    {
        _currentUserService = currentUserService;
        _roomManager = roomManager;
        _mediator = mediator;
    }

    public async Task<Result<LobbyRoomDetailResponse>> Handle(SetLobbyRoomMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<LobbyRoomDetailResponse>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        if (command.Request.MapId.HasValue && command.Request.MapId.Value != Guid.Empty)
        {
            var mapExists = await _mediator.Send(new MapExistsQuery(command.Request.MapId.Value), cancellationToken);
            if (!mapExists.IsSuccess || mapExists.Data != true)
                return Result<LobbyRoomDetailResponse>.Failure(mapExists.Message ?? "Bản đồ không được tìm thấy hoặc đã bị xóa.", ErrorCodeEnum.NotFound);
        }

        var (success, errorMessage, room) = _roomManager.SetRoomMap(command.RoomId, userIdNullable.Value, command.Request.MapId);
        if (!success || room == null)
        {
            var code = errorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true ? ErrorCodeEnum.NotFound : ErrorCodeEnum.ValidationFailed;
            return Result<LobbyRoomDetailResponse>.Failure(errorMessage ?? "Không thể thiết lập bản đồ.", code);
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
        return Result<LobbyRoomDetailResponse>.Success(response, "Bản đồ được cập nhật.");
    }
}

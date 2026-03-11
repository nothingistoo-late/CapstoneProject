using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Lobby.Models;
using CapstoneProject.Application.Features.Maps.Queries.MapExists;

namespace CapstoneProject.Application.Features.Lobby.Commands.CreateLobbyRoom;

public class CreateLobbyRoomCommandHandler : IRequestHandler<CreateLobbyRoomCommand, Result<CreateLobbyRoomResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoomManager _roomManager;
    private readonly IMediator _mediator;

    public CreateLobbyRoomCommandHandler(ICurrentUserService currentUserService, IRoomManager roomManager, IMediator mediator)
    {
        _currentUserService = currentUserService;
        _roomManager = roomManager;
        _mediator = mediator;
    }

    public async Task<Result<CreateLobbyRoomResponse>> Handle(CreateLobbyRoomCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<CreateLobbyRoomResponse>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var maxPlayers = command.Request?.MaxPlayers ?? 8;
        var mapId = command.Request?.SelectedMapId;
        if (mapId.HasValue && mapId.Value != Guid.Empty)
        {
            var mapExists = await _mediator.Send(new MapExistsQuery(mapId.Value), cancellationToken);
            if (!mapExists.IsSuccess || mapExists.Data != true)
                return Result<CreateLobbyRoomResponse>.Failure(mapExists.Message ?? "Map not found or has been deleted.", ErrorCodeEnum.NotFound);
        }

        var userId = userIdNullable.Value;
        var existingRoom = _roomManager.GetRoomContainingPlayer(userId);
        if (existingRoom != null)
        {
            var currentRoomInfo = new CreateLobbyRoomResponse
            {
                RoomId = existingRoom.RoomId,
                RoomCode = existingRoom.RoomCode,
                MaxPlayers = existingRoom.MaxPlayers,
                SelectedMapId = existingRoom.SelectedMapId
            };
            return Result<CreateLobbyRoomResponse>.Failure(
                "Không thể tạo phòng. Bạn đã ở trong một phòng rồi. Vui lòng rời phòng hiện tại trước khi tạo phòng mới.",
                ErrorCodeEnum.ValidationFailed,
                currentRoomInfo);
        }

        var room = _roomManager.CreateRoom(userId, "", maxPlayers, mapId);
        if (room == null)
            return Result<CreateLobbyRoomResponse>.Failure("Failed to create room.", ErrorCodeEnum.InvalidOperation);

        return Result<CreateLobbyRoomResponse>.Success(new CreateLobbyRoomResponse
        {
            RoomId = room.RoomId,
            RoomCode = room.RoomCode,
            MaxPlayers = room.MaxPlayers,
            SelectedMapId = room.SelectedMapId
        }, "Room created.");
    }
}

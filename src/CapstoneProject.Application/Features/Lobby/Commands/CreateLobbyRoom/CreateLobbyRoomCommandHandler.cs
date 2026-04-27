using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Lobby.Models;
using CapstoneProject.Application.Features.Games.Queries.MapExists;

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
            return Result<CreateLobbyRoomResponse>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var maxPlayers = command.Request?.MaxPlayers ?? 8;
        var gameId = command.Request?.SelectedGameId;
        if (gameId.HasValue && gameId.Value != Guid.Empty)
        {
            var mapExists = await _mediator.Send(new MapExistsQuery(gameId.Value), cancellationToken);
            if (!mapExists.IsSuccess || mapExists.Data != true)
                return Result<CreateLobbyRoomResponse>.Failure(mapExists.Message ?? "Trò chơi không được tìm thấy hoặc đã bị xóa.", ErrorCodeEnum.NotFound);
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
                SelectedGameId = existingRoom.SelectedGameId
            };
            return Result<CreateLobbyRoomResponse>.Failure(
                "Không thể tạo phòng. Bạn đã ở trong một phòng rồi. Vui lòng rời phòng hiện tại trước khi tạo phòng mới.",
                ErrorCodeEnum.ValidationFailed,
                currentRoomInfo);
        }

        var room = _roomManager.CreateRoom(userId, "", maxPlayers, gameId);
        if (room == null)
            return Result<CreateLobbyRoomResponse>.Failure("Không tạo được phòng.", ErrorCodeEnum.InvalidOperation);

        return Result<CreateLobbyRoomResponse>.Success(new CreateLobbyRoomResponse
        {
            RoomId = room.RoomId,
            RoomCode = room.RoomCode,
            MaxPlayers = room.MaxPlayers,
            SelectedGameId = room.SelectedGameId
        }, "Phòng được tạo.");
    }
}

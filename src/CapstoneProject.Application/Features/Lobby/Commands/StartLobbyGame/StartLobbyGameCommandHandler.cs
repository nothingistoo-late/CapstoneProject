using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Lobby.Models;
using CapstoneProject.Application.Features.Maps.Queries.MapExists;

namespace CapstoneProject.Application.Features.Lobby.Commands.StartLobbyGame;

public class StartLobbyGameCommandHandler : IRequestHandler<StartLobbyGameCommand, Result<StartGameResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoomManager _roomManager;
    private readonly IMediator _mediator;

    public StartLobbyGameCommandHandler(ICurrentUserService currentUserService, IRoomManager roomManager, IMediator mediator)
    {
        _currentUserService = currentUserService;
        _roomManager = roomManager;
        _mediator = mediator;
    }

    public async Task<Result<StartGameResponse>> Handle(StartLobbyGameCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<StartGameResponse>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var room = _roomManager.GetRoomById(command.RoomId);
        if (room?.SelectedMapId is { } selectedMapId && selectedMapId != Guid.Empty)
        {
            var mapExists = await _mediator.Send(new MapExistsQuery(selectedMapId), cancellationToken);
            if (!mapExists.IsSuccess || mapExists.Data != true)
                return Result<StartGameResponse>.Failure(mapExists.Message ?? "Map not found or has been deleted. Choose another map.", ErrorCodeEnum.NotFound);
        }

        var (success, errorMessage, gameInstance, _) = _roomManager.StartGame(command.RoomId, userIdNullable.Value);
        if (!success || gameInstance == null)
        {
            var code = errorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true ? ErrorCodeEnum.NotFound : ErrorCodeEnum.ValidationFailed;
            return Result<StartGameResponse>.Failure(errorMessage ?? "Could not start game.", code);
        }

        var state = gameInstance.GameState as LobbyGameState;
        return Result<StartGameResponse>.Success(new StartGameResponse
        {
            RoomId = gameInstance.RoomId,
            RoomCode = gameInstance.RoomCode,
            MapId = gameInstance.MapId,
            TurnOrder = gameInstance.TurnOrder.ToList(),
            StartedAt = gameInstance.StartedAt,
            CurrentTurnIndex = state?.CurrentTurnIndex ?? 0,
            CurrentPlayerId = state?.CurrentPlayerId ?? Guid.Empty,
            RoundNumber = state?.RoundNumber ?? 1
        }, "Game started. Connect to SignalR to receive real-time updates.");
    }
}

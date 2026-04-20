using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Lobby.Models;
using CapstoneProject.Application.Features.Games.Queries.MapExists;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Lobby.Commands.StartLobbyGame;

public class StartLobbyGameCommandHandler : IRequestHandler<StartLobbyGameCommand, Result<StartGameResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoomManager _roomManager;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public StartLobbyGameCommandHandler(ICurrentUserService currentUserService, IRoomManager roomManager, IMediator mediator, IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _roomManager = roomManager;
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StartGameResponse>> Handle(StartLobbyGameCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<StartGameResponse>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var room = _roomManager.GetRoomById(command.RoomId);
        if (room?.SelectedGameId is { } selectedGameId && selectedGameId != Guid.Empty)
        {
            var mapExists = await _mediator.Send(new MapExistsQuery(selectedGameId), cancellationToken);
            if (!mapExists.IsSuccess || mapExists.Data != true)
                return Result<StartGameResponse>.Failure(mapExists.Message ?? "Bản đồ không được tìm thấy hoặc đã bị xóa. Chọn bản đồ khác.", ErrorCodeEnum.NotFound);

            var playerIds = room.Players.Keys.ToList();
            var ownershipCheck = await EnsurePlayersCanPlayGame(selectedGameId, playerIds, cancellationToken);
            if (!ownershipCheck.Success)
                return Result<StartGameResponse>.Failure(ownershipCheck.ErrorMessage!, ErrorCodeEnum.Forbidden);
        }

        var (success, errorMessage, gameInstance, _) = _roomManager.StartGame(command.RoomId, userIdNullable.Value);
        if (!success || gameInstance == null)
        {
            var code = errorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true ? ErrorCodeEnum.NotFound : ErrorCodeEnum.ValidationFailed;
            return Result<StartGameResponse>.Failure(errorMessage ?? "Không thể bắt đầu trò chơi.", code);
        }

        var state = gameInstance.GameState as LobbyGameState;
        return Result<StartGameResponse>.Success(new StartGameResponse
        {
            RoomId = gameInstance.RoomId,
            RoomCode = gameInstance.RoomCode,
            GameId = gameInstance.GameId,
            TurnOrder = gameInstance.TurnOrder.ToList(),
            StartedAt = gameInstance.StartedAt,
            CurrentTurnIndex = state?.CurrentTurnIndex ?? 0,
            CurrentPlayerId = state?.CurrentPlayerId ?? Guid.Empty,
            RoundNumber = state?.RoundNumber ?? 1
        }, "Trò chơi bắt đầu. Kết nối với SignalR để nhận thông tin cập nhật theo thời gian thực.");
    }

    private async Task<(bool Success, string? ErrorMessage)> EnsurePlayersCanPlayGame(
        Guid gameId,
        List<Guid> playerIds,
        CancellationToken cancellationToken)
    {
        var game = await _unitOfWork.Repository<Game>().GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == gameId, cancellationToken);
        if (game == null || game.IsDeleted)
            return (false, "Bản đồ không được tìm thấy hoặc đã bị xóa.");

        var price = game.Price.GetValueOrDefault();
        if (price <= 0)
            return (true, null);

        var paidUserIds = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .Where(p => !p.IsDeleted
                        && p.GameId == game.Id
                        && p.PaymentStatus == PaymentStatusEnum.Completed
                        && playerIds.Contains(p.UserId))
            .Select(p => p.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var myGameUserIds = await _unitOfWork.Repository<MyGame>().GetQueryable()
            .Where(mg => !mg.IsDeleted
                         && mg.GameId == game.Id
                         && playerIds.Contains(mg.UserId))
            .Select(mg => mg.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var ownedUserIds = paidUserIds.Concat(myGameUserIds).ToHashSet();
        foreach (var playerId in playerIds)
        {
            if (game.CreatedBy == playerId || ownedUserIds.Contains(playerId))
                continue;
            return (false, "Tất cả người chơi trong phòng phải sở hữu bản đồ trước khi chơi.");
        }

        return (true, null);
    }
}

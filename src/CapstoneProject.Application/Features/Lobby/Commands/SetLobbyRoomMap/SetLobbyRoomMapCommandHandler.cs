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

namespace CapstoneProject.Application.Features.Lobby.Commands.SetLobbyRoomMap;

public class SetLobbyRoomMapCommandHandler : IRequestHandler<SetLobbyRoomMapCommand, Result<LobbyRoomDetailResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoomManager _roomManager;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public SetLobbyRoomMapCommandHandler(ICurrentUserService currentUserService, IRoomManager roomManager, IMediator mediator, IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _roomManager = roomManager;
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LobbyRoomDetailResponse>> Handle(SetLobbyRoomMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<LobbyRoomDetailResponse>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        if (command.Request.GameId.HasValue && command.Request.GameId.Value != Guid.Empty)
        {
            var mapExists = await _mediator.Send(new MapExistsQuery(command.Request.GameId.Value), cancellationToken);
            if (!mapExists.IsSuccess || mapExists.Data != true)
                return Result<LobbyRoomDetailResponse>.Failure(mapExists.Message ?? "Trò chơi không được tìm thấy hoặc đã bị xóa.", ErrorCodeEnum.NotFound);

            var roomToValidate = _roomManager.GetRoomById(command.RoomId);
            if (roomToValidate == null)
                return Result<LobbyRoomDetailResponse>.Failure("Không tìm thấy phòng.", ErrorCodeEnum.NotFound);

            var playerIds = roomToValidate.Players.Keys.ToList();
            var ownershipCheck = await EnsurePlayersCanPlayGame(command.Request.GameId.Value, playerIds, cancellationToken);
            if (!ownershipCheck.Success)
                return Result<LobbyRoomDetailResponse>.Failure(ownershipCheck.ErrorMessage!, ErrorCodeEnum.Forbidden);
        }

        var (success, errorMessage, room) = _roomManager.SetRoomMap(
            command.RoomId,
            userIdNullable.Value,
            command.Request.GameId,
            command.Request.MaxPlayers);
        if (!success || room == null)
        {
            var code = errorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true ? ErrorCodeEnum.NotFound : ErrorCodeEnum.ValidationFailed;
            return Result<LobbyRoomDetailResponse>.Failure(errorMessage ?? "Không thể thiết lập trò chơi.", code);
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
            SelectedGameId = room.SelectedGameId,
            Players = room.Players.Values.Select(p => new LobbyPlayerDto { PlayerId = p.PlayerId, IsReady = p.IsReady, IsHost = p.IsHost }).ToList()
        };
        return Result<LobbyRoomDetailResponse>.Success(response, "Trò chơi được cập nhật.");
    }

    private async Task<(bool Success, string? ErrorMessage)> EnsurePlayersCanPlayGame(
        Guid gameId,
        List<Guid> playerIds,
        CancellationToken cancellationToken)
    {
        var gameRepo = _unitOfWork.Repository<Game>();
        var game = await gameRepo.GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == gameId, cancellationToken);
        if (game == null || game.IsDeleted)
            return (false, "Trò chơi không được tìm thấy hoặc đã bị xóa.");

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
            return (false, "Tất cả người chơi trong phòng phải sở hữu trò chơi trước khi chơi.");
        }

        return (true, null);
    }
}

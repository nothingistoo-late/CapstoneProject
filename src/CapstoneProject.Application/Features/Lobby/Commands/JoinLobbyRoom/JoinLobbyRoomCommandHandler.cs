using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Lobby.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Lobby.Commands.JoinLobbyRoom;

public class JoinLobbyRoomCommandHandler : IRequestHandler<JoinLobbyRoomCommand, Result<JoinLobbyRoomResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoomManager _roomManager;
    private readonly IUnitOfWork _unitOfWork;

    public JoinLobbyRoomCommandHandler(ICurrentUserService currentUserService, IRoomManager roomManager, IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _roomManager = roomManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<JoinLobbyRoomResponse>> Handle(JoinLobbyRoomCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<JoinLobbyRoomResponse>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var request = command.Request;
        LobbyRoom? room;
        if (request.RoomId.HasValue && request.RoomId != Guid.Empty)
        {
            room = _roomManager.GetRoomById(request.RoomId.Value);
            if (room == null)
                return Result<JoinLobbyRoomResponse>.Failure("Không tìm thấy phòng.", ErrorCodeEnum.NotFound);
        }
        else if (!string.IsNullOrWhiteSpace(request.RoomCode))
        {
            room = _roomManager.GetRoomByCode(request.RoomCode.Trim());
            if (room == null)
                return Result<JoinLobbyRoomResponse>.Failure("Không tìm thấy phòng.", ErrorCodeEnum.NotFound);
        }
        else
            return Result<JoinLobbyRoomResponse>.Failure("Cung cấp RoomId hoặc RoomCode.", ErrorCodeEnum.ValidationFailed);

        if (room.SelectedGameId.HasValue && room.SelectedGameId.Value != Guid.Empty)
        {
            var checkOwnership = await EnsureUserCanJoinSelectedMap(room.SelectedGameId.Value, userIdNullable.Value, cancellationToken);
            if (!checkOwnership.Success)
                return Result<JoinLobbyRoomResponse>.Failure(checkOwnership.ErrorMessage!, ErrorCodeEnum.Forbidden);
        }

        var (success, errorMessage, updatedRoom) = _roomManager.JoinRoom(room.RoomId, userIdNullable.Value, "", request.RoomCode);
        if (!success || updatedRoom == null)
        {
            var code = errorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true ? ErrorCodeEnum.NotFound : ErrorCodeEnum.ValidationFailed;
            return Result<JoinLobbyRoomResponse>.Failure(errorMessage ?? "Không thể tham gia.", code);
        }

        return Result<JoinLobbyRoomResponse>.Success(new JoinLobbyRoomResponse
        {
            RoomId = updatedRoom.RoomId,
            RoomCode = updatedRoom.RoomCode,
            CurrentPlayerCount = updatedRoom.PlayerCount,
            MaxPlayers = updatedRoom.MaxPlayers
        }, "Đã tham gia phòng.");
    }

    private async Task<(bool Success, string? ErrorMessage)> EnsureUserCanJoinSelectedMap(Guid gameId, Guid userId, CancellationToken cancellationToken)
    {
        var game = await _unitOfWork.Repository<Game>().GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == gameId, cancellationToken);
        if (game == null || game.IsDeleted)
            return (false, "Không thể vào phòng: trò chơi đã chọn không còn tồn tại.");

        if (game.CreatedBy == userId || game.Price.GetValueOrDefault() <= 0)
            return (true, null);

        var purchased = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .AnyAsync(p => !p.IsDeleted
                           && p.UserId == userId
                           && p.GameId == game.Id
                           && p.PaymentStatus == PaymentStatusEnum.Completed, cancellationToken);
        if (purchased)
            return (true, null);

        var inMyGame = await _unitOfWork.Repository<MyGame>().GetQueryable()
            .AnyAsync(mg => !mg.IsDeleted && mg.UserId == userId && mg.GameId == game.Id, cancellationToken);
        if (inMyGame)
            return (true, null);

        return (false, "Không thể vào phòng: bạn chưa sở hữu trò chơi đang được chọn.");
    }
}

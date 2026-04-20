using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;

namespace CapstoneProject.Application.Features.Lobby.Commands.LeaveLobbyRoom;

public class LeaveLobbyRoomCommandHandler : IRequestHandler<LeaveLobbyRoomCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoomManager _roomManager;

    public LeaveLobbyRoomCommandHandler(ICurrentUserService currentUserService, IRoomManager roomManager)
    {
        _currentUserService = currentUserService;
        _roomManager = roomManager;
    }

    public async Task<Result> Handle(LeaveLobbyRoomCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var (success, errorMessage, _) = _roomManager.LeaveRoom(command.RoomId, userIdNullable.Value);
        if (!success)
        {
            var code = errorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true ? ErrorCodeEnum.NotFound : ErrorCodeEnum.ValidationFailed;
            return Result.Failure(errorMessage ?? "Không thể rời khỏi phòng.", code);
        }
        return Result.Success("Đã rời khỏi phòng.");
    }
}

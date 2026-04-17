using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Gameplay;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Gameplay.Commands.ValidateSolution;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Lobby.Commands.SubmitLobbySolution;

public class SubmitLobbySolutionCommandHandler : IRequestHandler<SubmitLobbySolutionCommand, Result<SubmitGameResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoomManager _roomManager;
    private readonly IMediator _mediator;

    public SubmitLobbySolutionCommandHandler(
        ICurrentUserService currentUserService,
        IRoomManager roomManager,
        IMediator mediator)
    {
        _currentUserService = currentUserService;
        _roomManager = roomManager;
        _mediator = mediator;
    }

    public async Task<Result<SubmitGameResponse>> Handle(SubmitLobbySolutionCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<SubmitGameResponse>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var userId = userIdNullable.Value;
        var room = _roomManager.GetRoomById(command.RoomId);
        if (room == null)
            return Result<SubmitGameResponse>.Failure("Không tìm thấy phòng.", ErrorCodeEnum.NotFound);
        if (room.Status != RoomStatusEnum.Playing)
            return Result<SubmitGameResponse>.Failure("Trò chơi không được tiến hành.", ErrorCodeEnum.ValidationFailed);
        if (!room.SelectedGameId.HasValue)
            return Result<SubmitGameResponse>.Failure("Phòng chưa có bản đồ nào được chọn.", ErrorCodeEnum.ValidationFailed);
        if (!room.Players.ContainsKey(userId))
            return Result<SubmitGameResponse>.Failure("Bạn không ở trong phòng này.", ErrorCodeEnum.ValidationFailed);

        var validateRequest = new ValidateSolutionRequest
        {
            GameId = room.SelectedGameId.Value,
            GameDetailId = command.Request.GameDetailId,
            Language = "Blockly",
            AstSpec = command.Request.AstSpec,
            BytecodeSpec = command.Request.BytecodeSpec,
            PlayMode = PlayModeEnum.Lobby,
            RoomId = command.RoomId,
            IsWin = command.Request.IsWin,
            ClientStepsUsed = command.Request.StepsUsed,
            ClientBlocksUsed = command.Request.BlocksUsed,
            ClientElapsedSeconds = command.Request.Time
        };
        var validateResult = await _mediator.Send(new ValidateSolutionCommand(validateRequest), cancellationToken);
        if (!validateResult.IsSuccess || validateResult.Data == null)
            return Result<SubmitGameResponse>.Failure(validateResult.Message ?? "Xác thực không thành công.", ErrorCodeEnum.ValidationFailed);

        // Äiá»ƒm chá»‰ tá»« server (ValidateSolution) â€” khÃ´ng tin Score client.
        var score = validateResult.Data.Score ?? 0;
        var status = validateResult.Data.Status.ToString();
        var (recordSuccess, recordError, ranking) = _roomManager.RecordSubmission(
            command.RoomId, userId, score, status, validateResult.Data.SubmissionId);
        if (!recordSuccess)
            return Result<SubmitGameResponse>.Failure(recordError ?? "Không thể ghi lại bài nộp.", ErrorCodeEnum.ValidationFailed);

        var response = new SubmitGameResponse
        {
            Score = score,
            Status = status,
            SubmissionId = validateResult.Data.SubmissionId,
            RankingIfAllSubmitted = ranking?.ToList()
        };
        return Result<SubmitGameResponse>.Success(response, "Đã ghi lại nội dung gửi.");
    }
}

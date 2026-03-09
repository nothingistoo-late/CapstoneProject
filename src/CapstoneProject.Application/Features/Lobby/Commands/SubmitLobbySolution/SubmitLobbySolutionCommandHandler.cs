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
            return Result<SubmitGameResponse>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var userId = userIdNullable.Value;
        var room = _roomManager.GetRoomById(command.RoomId);
        if (room == null)
            return Result<SubmitGameResponse>.Failure("Room not found.", ErrorCodeEnum.NotFound);
        if (room.Status != RoomStatusEnum.Playing)
            return Result<SubmitGameResponse>.Failure("Game is not in progress.", ErrorCodeEnum.ValidationFailed);
        if (!room.SelectedMapId.HasValue)
            return Result<SubmitGameResponse>.Failure("Room has no map selected.", ErrorCodeEnum.ValidationFailed);
        if (!room.Players.ContainsKey(userId))
            return Result<SubmitGameResponse>.Failure("You are not in this room.", ErrorCodeEnum.ValidationFailed);

        var validateRequest = new ValidateSolutionRequest
        {
            MapId = room.SelectedMapId.Value,
            Language = "Blockly",
            AstSpec = command.Request.AstSpec,
            BytecodeSpec = command.Request.BytecodeSpec
        };
        var validateResult = await _mediator.Send(new ValidateSolutionCommand(validateRequest), cancellationToken);
        if (!validateResult.IsSuccess || validateResult.Data == null)
            return Result<SubmitGameResponse>.Failure(validateResult.Message ?? "Validation failed.", ErrorCodeEnum.ValidationFailed);

        var score = validateResult.Data.Score ?? 0;
        var status = validateResult.Data.Status.ToString();
        var (recordSuccess, recordError, ranking) = _roomManager.RecordSubmission(
            command.RoomId, userId, score, status, validateResult.Data.SubmissionId);
        if (!recordSuccess)
            return Result<SubmitGameResponse>.Failure(recordError ?? "Could not record submission.", ErrorCodeEnum.ValidationFailed);

        var response = new SubmitGameResponse
        {
            Score = score,
            Status = status,
            SubmissionId = validateResult.Data.SubmissionId,
            RankingIfAllSubmitted = ranking?.ToList()
        };
        return Result<SubmitGameResponse>.Success(response, "Submission recorded.");
    }
}

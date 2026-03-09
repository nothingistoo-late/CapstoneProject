using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Gameplay;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Gameplay.Commands.ValidateSolution;

/// <summary>
/// Nhận block strategy, tạo Submission, mô phỏng đơn giản (hoặc gọi engine thật),
/// lưu ExecutionsResult, cập nhật UserMapResult (score, stars, attempts) và cộng XP nếu Accepted.
/// </summary>
public class ValidateSolutionCommandHandler : IRequestHandler<ValidateSolutionCommand, Result<ValidateSolutionResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ValidateSolutionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ValidateSolutionResultDto>> Handle(ValidateSolutionCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<ValidateSolutionResultDto>.Failure("Authentication required. Please log in to validate a solution.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var mapRepo = _unitOfWork.Repository<Map>();
        var map = await mapRepo.GetQueryable().Include(m => m.MapDetail).FirstOrDefaultAsync(m => m.Id == command.Request.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result<ValidateSolutionResultDto>.Failure("Map not found", ErrorCodeEnum.NotFound);
        if (map.MapDetail == null)
            return Result<ValidateSolutionResultDto>.Failure("Map data not found", ErrorCodeEnum.ValidationFailed);

        // Placeholder: chỉ chấp nhận khi có đủ AstSpec + BytecodeSpec hợp lệ. Thực tế cần chạy engine/block interpreter để chấm đúng.
        var ast = command.Request.AstSpec?.Trim() ?? string.Empty;
        var bytecode = command.Request.BytecodeSpec?.Trim() ?? string.Empty;
        var stepsUsed = ast.Length;
        var blocksUsed = bytecode.Length;
        const int maxSteps = 10000;
        const int minLength = 2; // ít nhất phải có nội dung thật (không chấp nhận 1 ký tự rác)
        var hasValidInput = ast.Length >= minLength && bytecode.Length >= minLength;
        var accepted = hasValidInput && stepsUsed <= maxSteps;
        var statusEnum = accepted ? SubmissionStatusEnum.Accepted : SubmissionStatusEnum.WrongAnswer;
        var score = accepted ? Math.Max(0, 100 - stepsUsed / 50) : 0; // càng nhiều step càng trừ điểm
        var stars = accepted ? (score >= 90 ? 3 : score >= 60 ? 2 : 1) : 0;

        var submission = new Submission
        {
            UserId = userId,
            MapId = map.Id,
            Language = command.Request.Language,
            AstSpec = command.Request.AstSpec,
            BytecodeSpec = command.Request.BytecodeSpec,
            ResultStatus = statusEnum,
            Score = score,
            StepsUsed = stepsUsed,
            BlocksUsed = blocksUsed
        };
        submission.InitializeEntity(userId);
        await _unitOfWork.Repository<Submission>().AddAsync(submission);

        var execResult = new ExecutionsResult
        {
            SubmissionId = submission.Id,
            StartedAt = DateTime.UtcNow,
            FinishedAt = DateTime.UtcNow,
            IsDeterministic = true,
            ResultSpec = accepted ? "{\"win\":true}" : "{\"win\":false}"
        };
        execResult.InitializeEntity(userId);
        await _unitOfWork.Repository<ExecutionsResult>().AddAsync(execResult);

        var umrRepo = _unitOfWork.Repository<UserMapResult>();
        var umr = await umrRepo.GetQueryable().FirstOrDefaultAsync(u => u.UserId == userId && u.MapId == map.Id, cancellationToken);
        if (umr == null)
        {
            umr = new UserMapResult
            {
                UserId = userId,
                MapId = map.Id,
                BestScore = score,
                BestStars = stars,
                Attempts = 1,
                LastPlayedAt = DateTime.UtcNow
            };
            umr.InitializeEntity(userId);
            await umrRepo.AddAsync(umr);
        }
        else
        {
            umr.Attempts++;
            umr.LastPlayedAt = DateTime.UtcNow;
            if (score > umr.BestScore) umr.BestScore = score;
            if (stars > umr.BestStars) umr.BestStars = stars;
            umr.UpdateEntity(userId);
            umrRepo.Update(umr);
        }

        if (accepted)
        {
            var xpDelta = 10 + stars * 5;
            var xp = new XpTransaction { UserId = userId, MapId = map.Id, Delta = xpDelta, Reason = "Map completed" };
            xp.InitializeEntity(userId);
            await _unitOfWork.Repository<XpTransaction>().AddAsync(xp);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map response DTO từ entity Submission đã lưu (đúng diagram: SubmissionId, Status, Score, StepsUsed, BlocksUsed)
        var dto = new ValidateSolutionResultDto
        {
            SubmissionId = submission.Id,
            Status = submission.ResultStatus,
            Score = submission.Score,
            StepsUsed = submission.StepsUsed,
            BlocksUsed = submission.BlocksUsed,
            Stars = stars,
            Message = accepted ? "Accepted" : "Wrong answer or constraint violation"
        };
        return Result<ValidateSolutionResultDto>.Success(dto);
    }
}

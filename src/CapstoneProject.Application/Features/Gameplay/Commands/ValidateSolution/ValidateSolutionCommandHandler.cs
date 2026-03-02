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
        var map = await mapRepo.GetQueryable().Include(m => m.MapSpecs).FirstOrDefaultAsync(m => m.Id == command.Request.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result<ValidateSolutionResultDto>.Failure("Map not found", ErrorCodeEnum.NotFound);

        var spec = map.MapSpecs.OrderByDescending(s => s.Version).FirstOrDefault();
        if (spec == null)
            return Result<ValidateSolutionResultDto>.Failure("Map has no spec", ErrorCodeEnum.ValidationFailed);

        // Placeholder: run simple validation (in real app would run block interpreter/simulator)
        var stepsUsed = command.Request.AstSpec?.Length ?? 0;
        var blocksUsed = command.Request.BytecodeSpec?.Length ?? 0;
        const int maxSteps = 1000;
        var accepted = stepsUsed <= maxSteps && stepsUsed > 0;
        var status = accepted ? SubmissionStatusEnum.Accepted : SubmissionStatusEnum.WrongAnswer;
        var score = accepted ? Math.Max(0, 100 - stepsUsed / 10) : 0;
        var stars = accepted ? (score >= 90 ? 3 : score >= 60 ? 2 : 1) : 0;

        var submission = new Submission
        {
            UserId = userId,
            MapId = map.Id,
            Language = command.Request.Language,
            AstSpec = command.Request.AstSpec,
            BytecodeSpec = command.Request.BytecodeSpec,
            ResultStatus = status,
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

        var dto = new ValidateSolutionResultDto
        {
            SubmissionId = submission.Id,
            Status = status,
            Score = score,
            Stars = stars,
            StepsUsed = stepsUsed,
            BlocksUsed = blocksUsed,
            Message = accepted ? "Accepted" : "Wrong answer or constraint violation"
        };
        return Result<ValidateSolutionResultDto>.Success(dto);
    }
}

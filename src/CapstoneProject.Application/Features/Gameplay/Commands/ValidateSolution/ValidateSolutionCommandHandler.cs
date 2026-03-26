using System.Text.Json;
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
/// Nháº­n block strategy, táº¡o Submission, mÃ´ phá»ng Ä‘Æ¡n giáº£n (hoáº·c gá»i engine tháº­t),
/// lÆ°u ExecutionsResult, cáº­p nháº­t UserMapResult (score, stars, attempts) vÃ  cá»™ng XP náº¿u Accepted.
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

        var ast = command.Request.AstSpec?.Trim() ?? string.Empty;
        var bytecode = command.Request.BytecodeSpec?.Trim() ?? string.Empty;

        int score;
        int stars;
        SubmissionStatusEnum statusEnum;
        bool accepted;
        int stepsUsedForSubmission;
        int blocksUsedForSubmission;

        if (HasEngineMetrics(command.Request))
        {
            (score, stars, statusEnum, accepted, stepsUsedForSubmission, blocksUsedForSubmission) =
                ScoreFromEngineMetrics(command.Request, map.MapDetail.JsonContent, ast, bytecode);
        }
        else
        {
            (score, stars, statusEnum, accepted, stepsUsedForSubmission, blocksUsedForSubmission) =
                ScoreFromLegacyPlaceholder(ast, bytecode);
        }

        var submission = new Submission
        {
            UserId = userId,
            MapId = map.Id,
            Language = command.Request.Language,
            AstSpec = command.Request.AstSpec,
            BytecodeSpec = command.Request.BytecodeSpec,
            ResultStatus = statusEnum,
            Score = score,
            StepsUsed = stepsUsedForSubmission,
            BlocksUsed = blocksUsedForSubmission,
            MatchId = command.Request.MatchId
        };
        submission.InitializeEntity(userId);
        await _unitOfWork.Repository<Submission>().AddAsync(submission);

        var execResult = new ExecutionsResult
        {
            SubmissionId = submission.Id,
            StartedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
            FinishedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
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
                LastPlayedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow
            };
            umr.InitializeEntity(userId);
            await umrRepo.AddAsync(umr);
        }
        else
        {
            umr.Attempts++;
            umr.LastPlayedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
            if (score > umr.BestScore) umr.BestScore = score;
            if (stars > umr.BestStars) umr.BestStars = stars;
            umr.UpdateEntity(userId);
            umrRepo.Update(umr);
        }

        var history = new UserMapPlayHistory
        {
            UserId = userId,
            MapId = map.Id,
            PlayMode = command.Request.PlayMode,
            RoomId = command.Request.RoomId,
            MatchId = command.Request.MatchId,
            StartTime = execResult.StartedAt ?? CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
            EndTime = execResult.FinishedAt,
            IsCompleted = accepted,
            Score = score,
            Stars = stars,
            SubmissionId = submission.Id,
            ExecutionsResultId = execResult.Id,
            Language = command.Request.Language
        };
        history.InitializeEntity(userId);
        await _unitOfWork.Repository<UserMapPlayHistory>().AddAsync(history);

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
            Status = submission.ResultStatus,
            Score = submission.Score,
            StepsUsed = submission.StepsUsed,
            BlocksUsed = submission.BlocksUsed,
            Stars = stars,
            Message = accepted ? "Accepted" : "Wrong answer or constraint violation"
        };
        return Result<ValidateSolutionResultDto>.Success(dto);
    }

    /// <summary>Client gá»­i IsWin (+ metrics) â€” cháº¥m giá»‘ng logic sao trÃªn UI (thá»i gian / bÆ°á»›c / block).</summary>
    private static bool HasEngineMetrics(ValidateSolutionRequest r) => r.IsWin.HasValue;

    private static (int score, int stars, SubmissionStatusEnum status, bool accepted, int stepsUsed, int blocksUsed) ScoreFromEngineMetrics(
        ValidateSolutionRequest req,
        string mapJsonContent,
        string ast,
        string bytecode)
    {
        var stepsUsed = req.ClientStepsUsed ?? ast.Length;
        var blocksUsed = req.ClientBlocksUsed ?? bytecode.Length;

        if (req.IsWin != true)
        {
            return (0, 0, SubmissionStatusEnum.WrongAnswer, false, stepsUsed, blocksUsed);
        }

        var limits = ParseMissionLimits(mapJsonContent);
        var elapsed = req.ClientElapsedSeconds ?? 0;
        var starCount = ComputeStarCount(elapsed, stepsUsed, blocksUsed, limits);
        var s = ScoreFromWinAndStars(true, starCount);
        return (s, starCount, SubmissionStatusEnum.Accepted, true, stepsUsed, blocksUsed);
    }

    /// <summary>Placeholder cÅ©: khÃ´ng Ä‘Æ°á»£c tin â€” AST rá»—ng <c>[]</c> váº«n bá»‹ tÃ­nh ~100 Ä‘iá»ƒm; thÃªm cháº·n trivial.</summary>
    private static (int score, int stars, SubmissionStatusEnum status, bool accepted, int stepsUsed, int blocksUsed) ScoreFromLegacyPlaceholder(
        string ast,
        string bytecode)
    {
        var stepsUsed = ast.Length;
        var blocksUsed = bytecode.Length;
        const int maxSteps = 10000;
        const int minLength = 2;
        var trivial = IsTrivialEmptyAst(ast) && bytecode.Length < minLength;
        var hasValidInput = (ast.Length >= minLength || bytecode.Length >= minLength) && !trivial;
        var accepted = hasValidInput && stepsUsed <= maxSteps && blocksUsed <= maxSteps;
        var statusEnum = accepted ? SubmissionStatusEnum.Accepted : SubmissionStatusEnum.WrongAnswer;
        var score = accepted ? Math.Max(0, 100 - stepsUsed / 50) : 0;
        var stars = accepted ? (score >= 90 ? 3 : score >= 60 ? 2 : 1) : 0;
        return (score, stars, statusEnum, accepted, stepsUsed, blocksUsed);
    }

    private static bool IsTrivialEmptyAst(string ast)
    {
        if (string.IsNullOrWhiteSpace(ast)) return true;
        var t = ast.Trim();
        return t is "[]" or "{}" or "null";
    }

    private readonly record struct MissionLimits(double TimeLimitSeconds, double EstimatedSteps, double BlockLimit);

    private static MissionLimits ParseMissionLimits(string? json)
    {
        var inf = double.PositiveInfinity;
        if (string.IsNullOrWhiteSpace(json))
            return new MissionLimits(inf, inf, inf);
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return new MissionLimits(inf, inf, inf);

            var mapConfig = root.TryGetProperty("mapConfig", out var mc) ? mc : root;
            double time = inf, steps = inf, blocks = inf;

            if (mapConfig.TryGetProperty("timeLimitSeconds", out var t) && t.ValueKind == JsonValueKind.Number)
                time = t.GetDouble();
            if (mapConfig.TryGetProperty("estimatedSteps", out var es) && es.ValueKind == JsonValueKind.Number)
                steps = es.GetDouble();

            if (root.TryGetProperty("blockConstraints", out var bc) &&
                bc.TryGetProperty("blockLimit", out var bl) &&
                bl.ValueKind == JsonValueKind.Number)
                blocks = bl.GetDouble();

            return new MissionLimits(time, steps, blocks);
        }
        catch
        {
            return new MissionLimits(inf, inf, inf);
        }
    }

    /// <summary>Giá»‘ng GameResultsModal: 0â€“3 sao theo time / steps / block so vá»›i limit map.</summary>
    private static int ComputeStarCount(double elapsedSeconds, int steps, int blocks, MissionLimits lim)
    {
        if (lim.TimeLimitSeconds >= double.PositiveInfinity &&
            lim.EstimatedSteps >= double.PositiveInfinity &&
            lim.BlockLimit >= double.PositiveInfinity)
        {
            return 1;
        }

        var s = 0;
        if (elapsedSeconds <= lim.TimeLimitSeconds) s++;
        if (steps <= lim.EstimatedSteps) s++;
        if (blocks <= lim.BlockLimit) s++;
        return s;
    }

    /// <summary>Tháº¯ng: 10 + 30*stars (max 100), khá»›p UI (0 sao váº«n pass level thÃ¬ 10 Ä‘iá»ƒm).</summary>
    private static int ScoreFromWinAndStars(bool isWin, int stars)
    {
        if (!isWin) return 0;
        return Math.Min(100, 10 + stars * 30);
    }
}




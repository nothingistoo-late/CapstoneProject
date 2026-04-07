using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Gameplay;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Commons.Models.Xp;
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
    private readonly IXpEngineService _xpEngineService;

    public ValidateSolutionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IXpEngineService xpEngineService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _xpEngineService = xpEngineService;
    }

    public async Task<Result<ValidateSolutionResultDto>> Handle(ValidateSolutionCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<ValidateSolutionResultDto>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để xác nhận giải pháp.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var mapRepo = _unitOfWork.Repository<Map>();
        var map = await mapRepo.GetQueryable()
            .Include(m => m.MapDetails)
            .FirstOrDefaultAsync(m => m.Id == command.Request.MapId, cancellationToken);
        if (map == null)
            return Result<ValidateSolutionResultDto>.Failure("Không tìm thấy bản đồ", ErrorCodeEnum.NotFound);

        var isOwned = false;
        if (map.IsDeleted || map.FreeTrialAttemptLimit > 0)
            isOwned = await IsMapOwnedByUserAsync(map, userId, cancellationToken);

        if (map.IsDeleted)
        {
            if (!isOwned)
                return Result<ValidateSolutionResultDto>.Failure("Không tìm thấy bản đồ", ErrorCodeEnum.NotFound);
        }

        var levelsOrdered = map.MapDetails.OrderBy(d => d.LevelOrder).ToList();
        if (levelsOrdered.Count == 0)
            return Result<ValidateSolutionResultDto>.Failure("Không tìm thấy dữ liệu bản đồ", ErrorCodeEnum.ValidationFailed);

        var mapDetail = ResolveMapDetail(command.Request.MapDetailId, levelsOrdered);
        if (mapDetail == null)
            return Result<ValidateSolutionResultDto>.Failure(
                "MapDetailId là bắt buộc khi bản đồ có nhiều cấp độ hoặc không hợp lệ đối với bản đồ này.",
                ErrorCodeEnum.ValidationFailed);

        var umrRepo = _unitOfWork.Repository<UserMapResult>();
        var umr = await umrRepo.GetQueryable().FirstOrDefaultAsync(
            u => u.UserId == userId && u.MapDetailId == mapDetail.Id,
            cancellationToken);
        var currentMapAttempts = await umrRepo.GetQueryable()
            .Where(u => u.UserId == userId && u.MapId == map.Id && !u.IsDeleted)
            .Select(u => (int?)u.Attempts)
            .SumAsync(cancellationToken) ?? 0;
        var isTrialPlay = map.FreeTrialAttemptLimit > 0 && !isOwned;
        if (isTrialPlay && currentMapAttempts >= map.FreeTrialAttemptLimit)
            return Result<ValidateSolutionResultDto>.Failure("Không còn lượt dùng thử miễn phí nào cho bản đồ này.", ErrorCodeEnum.ValidationFailed);

        var mapSolveCfg = await _unitOfWork.Repository<MapSolveScoreConfig>().GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ConfigKey == MapSolveScoreConfig.DefaultConfigKey && !x.IsDeleted,
                cancellationToken);
        var scoreWeights = MapSolveScoreWeights.FromDbOrLegacy(mapSolveCfg);

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
                ScoreFromEngineMetrics(command.Request, mapDetail.JsonContent, ast, bytecode, scoreWeights);
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
            MapDetailId = mapDetail.Id,
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

        if (umr == null)
        {
            umr = new UserMapResult
            {
                UserId = userId,
                MapId = map.Id,
                MapDetailId = mapDetail.Id,
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
            MapDetailId = mapDetail.Id,
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

        if (accepted && !isTrialPlay)
        {
            var xpDelta = 10 + stars * 5;
            var xpResult = await _xpEngineService.GrantXpAsync(new XpGrantInput
            {
                UserId = userId,
                RequestedXp = xpDelta,
                SourceType = XpSourceTypeEnum.MapSolve,
                SourceId = map.Id,
                IdempotencyKey = $"xp:mapsolve:{userId}:{map.Id}:{mapDetail.Id}:{submission.Id}",
                Reason = "Map completed",
                Metadata = $"{{\"stars\":{stars},\"score\":{score}}}"
            }, cancellationToken);
            if (!xpResult.IsSuccess)
                return Result<ValidateSolutionResultDto>.Failure(xpResult.Message ?? "Không cấp được XP.", ErrorCodeEnum.DatabaseError);

            var selectedGoal = await _unitOfWork.Repository<UserLearningGoal>().GetQueryable()
                .Where(ug => ug.UserId == userId && !ug.IsDeleted)
                .OrderByDescending(ug => ug.SelectedAt)
                .Select(ug => ug.LearningGoalId)
                .FirstOrDefaultAsync(cancellationToken);

            if (selectedGoal != Guid.Empty)
            {
                var goalItems = await _unitOfWork.Repository<LearningPathItem>().GetQueryable()
                    .Where(i => i.LearningGoalId == selectedGoal && !i.IsDeleted)
                    .Select(i => new { i.ItemType, i.ConceptId, i.MapId })
                    .ToListAsync(cancellationToken);

                if (goalItems.Any(i => i.MapId == map.Id))
                {
                    var conceptIdsInGoal = goalItems.Where(i => i.ConceptId.HasValue).Select(i => i.ConceptId!.Value).ToHashSet();
                    var mapIdsInGoal = goalItems.Where(i => i.MapId.HasValue).Select(i => i.MapId!.Value).ToHashSet();

                    var completedConceptIds = await _unitOfWork.Repository<UserConceptProgress>().GetQueryable()
                        .Where(p => p.UserId == userId && !p.IsDeleted && p.IsCompleted && conceptIdsInGoal.Contains(p.ConceptId))
                        .Select(p => p.ConceptId)
                        .ToListAsync(cancellationToken);
                    var completedConceptSet = completedConceptIds.ToHashSet();

                    var completedMapSet = new HashSet<Guid>();
                    foreach (var mid in mapIdsInGoal)
                    {
                        if (await MapProgressHelper.MapHasAllLevelsCompletedAsync(_unitOfWork, userId, mid, minStars: 1, cancellationToken))
                            completedMapSet.Add(mid);
                    }

                    var isLearningPathCompleted = goalItems.All(i =>
                        i.ItemType == LearningPathItemTypeEnum.Concept
                            ? i.ConceptId.HasValue && completedConceptSet.Contains(i.ConceptId.Value)
                            : i.MapId.HasValue && completedMapSet.Contains(i.MapId.Value));

                    if (isLearningPathCompleted)
                    {
                        var pathXpResult = await _xpEngineService.GrantXpAsync(new XpGrantInput
                        {
                            UserId = userId,
                            RequestedXp = 0,
                            SourceType = XpSourceTypeEnum.LearningPathComplete,
                            SourceId = selectedGoal,
                            IdempotencyKey = $"xp:learningpath:{userId}:{selectedGoal}",
                            Reason = "Learning path completed",
                            Metadata = $"{{\"learningGoalId\":\"{selectedGoal}\"}}"
                        }, cancellationToken);
                        if (!pathXpResult.IsSuccess)
                            return Result<ValidateSolutionResultDto>.Failure(pathXpResult.Message ?? "Không cấp được lộ trình học tập XP.", ErrorCodeEnum.DatabaseError);
                    }
                }
            }
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

    private async Task<bool> IsMapOwnedByUserAsync(Map map, Guid userId, CancellationToken cancellationToken)
    {
        if (map.CreatedBy.HasValue && map.CreatedBy.Value == userId)
            return true;

        var purchased = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .AnyAsync(p => !p.IsDeleted && p.UserId == userId && p.MapId == map.Id && p.PaymentStatus == PaymentStatusEnum.Completed, cancellationToken);
        if (purchased)
            return true;

        return await _unitOfWork.Repository<MyMap>().GetQueryable()
            .AnyAsync(mm => !mm.IsDeleted && mm.UserId == userId && mm.MapId == map.Id, cancellationToken);
    }

    /// <summary>Null náº¿u map nhiá»u level mÃ  khÃ´ng gá»­i MapDetailId há»£p lá»‡.</summary>
    private static MapDetail? ResolveMapDetail(Guid? requestedId, List<MapDetail> levelsOrdered)
    {
        if (levelsOrdered.Count == 1)
            return levelsOrdered[0];
        if (requestedId.HasValue && requestedId.Value != Guid.Empty)
            return levelsOrdered.FirstOrDefault(d => d.Id == requestedId.Value);
        return null;
    }

    /// <summary>Client gá»­i IsWin (+ metrics) â€” cháº¥m giá»‘ng logic sao trÃªn UI (thá»i gian / bÆ°á»›c / block).</summary>
    private static bool HasEngineMetrics(ValidateSolutionRequest r) => r.IsWin.HasValue;

    private static (int score, int stars, SubmissionStatusEnum status, bool accepted, int stepsUsed, int blocksUsed) ScoreFromEngineMetrics(
        ValidateSolutionRequest req,
        string mapJsonContent,
        string ast,
        string bytecode,
        MapSolveScoreWeights weights)
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
        var s = ScoreWinFromEngineCriteria(starCount, limits, elapsed, stepsUsed, blocksUsed, weights);
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

    /// <summary>Äiá»ƒm khi tháº¯ng cÃ³ metrics: base + pháº§n time/steps/blocks khi Ä‘áº¡t; cáº£ 3 limit vÃ´ cá»±c thÃ¬ chia Ä‘á»u pool 3 tiÃªu chÃ­ theo sá»‘ sao.</summary>
    private static int ScoreWinFromEngineCriteria(
        int starCount,
        MissionLimits lim,
        double elapsedSeconds,
        int steps,
        int blocks,
        MapSolveScoreWeights w)
    {
        var inf = double.PositiveInfinity;
        if (lim.TimeLimitSeconds >= inf && lim.EstimatedSteps >= inf && lim.BlockLimit >= inf)
        {
            var criteriaPool = w.TimeScore + w.StepsScore + w.BlocksScore;
            return Math.Clamp(w.BaseScore + (int)Math.Round(criteriaPool * starCount / 3.0), 0, 100);
        }

        var timeMet = elapsedSeconds <= lim.TimeLimitSeconds;
        var stepsMet = steps <= lim.EstimatedSteps;
        var blocksMet = blocks <= lim.BlockLimit;
        var score = w.BaseScore
                    + (timeMet ? w.TimeScore : 0)
                    + (stepsMet ? w.StepsScore : 0)
                    + (blocksMet ? w.BlocksScore : 0);
        return Math.Clamp(score, 0, 100);
    }

    private readonly record struct MapSolveScoreWeights(int BaseScore, int TimeScore, int StepsScore, int BlocksScore)
    {
        public static MapSolveScoreWeights Legacy => new(10, 30, 30, 30);

        public static MapSolveScoreWeights FromDbOrLegacy(MapSolveScoreConfig? cfg)
        {
            if (cfg == null) return Legacy;
            if (cfg.BaseScore + cfg.TimeScore + cfg.StepsScore + cfg.BlocksScore != 100) return Legacy;
            return new MapSolveScoreWeights(cfg.BaseScore, cfg.TimeScore, cfg.StepsScore, cfg.BlocksScore);
        }
    }
}





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
/// lÆ°u ExecutionsResult, cáº­p nháº­t UserGameResult (score, stars, attempts) vÃ  cá»™ng XP náº¿u Accepted.
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

        var mapRepo = _unitOfWork.Repository<Game>();
        var game = await mapRepo.GetQueryable()
            .Include(m => m.GameDetails)
            .FirstOrDefaultAsync(m => m.Id == command.Request.GameId, cancellationToken);
        if (game == null)
            return Result<ValidateSolutionResultDto>.Failure("Không tìm thấy trò chơi", ErrorCodeEnum.NotFound);

        var isOwned = await IsMapOwnedByUserAsync(game, userId, cancellationToken);
        var isAuthor = game.CreatedBy.HasValue && game.CreatedBy.Value == userId;

        if (game.IsDeleted || (!game.IsPublished && !isAuthor))
        {
            if (!isOwned)
                return Result<ValidateSolutionResultDto>.Failure("Không tìm thấy trò chơi", ErrorCodeEnum.NotFound);
        }

        var mapPrice = game.Price.GetValueOrDefault();
        var isPaidMap = mapPrice > 0;
        if (isPaidMap && !isOwned)
            return Result<ValidateSolutionResultDto>.Failure("Bạn chưa sở hữu trò chơi này.", ErrorCodeEnum.Forbidden);

        var levelsOrdered = game.GameDetails.OrderBy(d => d.LevelOrder).ToList();
        if (levelsOrdered.Count == 0)
            return Result<ValidateSolutionResultDto>.Failure("Không tìm thấy dữ liệu trò chơi", ErrorCodeEnum.ValidationFailed);

        var mapDetail = ResolveGameDetail(command.Request.GameDetailId, levelsOrdered);
        if (mapDetail == null)
            return Result<ValidateSolutionResultDto>.Failure(
                "GameDetailId là bắt buộc khi trò chơi có nhiều cấp độ hoặc không hợp lệ đối với trò chơi này.",
                ErrorCodeEnum.ValidationFailed);

        var umrRepo = _unitOfWork.Repository<UserGameResult>();
        var umr = await umrRepo.GetQueryable().FirstOrDefaultAsync(
            u => u.UserId == userId && u.GameDetailId == mapDetail.Id,
            cancellationToken);
        var mapSolveCfg = await _unitOfWork.Repository<GameSolveScoreConfig>().GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ConfigKey == GameSolveScoreConfig.DefaultConfigKey && !x.IsDeleted,
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
            GameId = game.Id,
            GameDetailId = mapDetail.Id,
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
            umr = new UserGameResult
            {
                UserId = userId,
                GameId = game.Id,
                GameDetailId = mapDetail.Id,
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

        var history = new UserGamePlayHistory
        {
            UserId = userId,
            GameId = game.Id,
            GameDetailId = mapDetail.Id,
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
        await _unitOfWork.Repository<UserGamePlayHistory>().AddAsync(history);

        if (accepted)
        {
            var xpDelta = 10 + stars * 5;
            var xpResult = await _xpEngineService.GrantXpAsync(new XpGrantInput
            {
                UserId = userId,
                RequestedXp = xpDelta,
                SourceType = XpSourceTypeEnum.MapSolve,
                SourceId = game.Id,
                IdempotencyKey = $"xp:mapsolve:{userId}:{game.Id}:{mapDetail.Id}:{submission.Id}",
                Reason = "Game completed",
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
                    .Select(i => new { i.ItemType, i.ConceptId, i.GameId })
                    .ToListAsync(cancellationToken);

                if (goalItems.Any(i => i.GameId == game.Id))
                {
                    var conceptIdsInGoal = goalItems.Where(i => i.ConceptId.HasValue).Select(i => i.ConceptId!.Value).ToHashSet();
                    var gameIdsInGoal = goalItems.Where(i => i.GameId.HasValue).Select(i => i.GameId!.Value).ToHashSet();

                    var completedConceptIds = await _unitOfWork.Repository<UserConceptProgress>().GetQueryable()
                        .Where(p => p.UserId == userId && !p.IsDeleted && p.IsCompleted && conceptIdsInGoal.Contains(p.ConceptId))
                        .Select(p => p.ConceptId)
                        .ToListAsync(cancellationToken);
                    var completedConceptSet = completedConceptIds.ToHashSet();

                    var completedMapSet = new HashSet<Guid>();
                    foreach (var mid in gameIdsInGoal)
                    {
                        if (await MapProgressHelper.MapHasAllLevelsCompletedAsync(_unitOfWork, userId, mid, minStars: 1, cancellationToken))
                            completedMapSet.Add(mid);
                    }

                    var isLearningPathCompleted = goalItems.All(i =>
                        i.ItemType == LearningPathItemTypeEnum.Concept
                            ? i.ConceptId.HasValue && completedConceptSet.Contains(i.ConceptId.Value)
                            : i.GameId.HasValue && completedMapSet.Contains(i.GameId.Value));

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
        return Result<ValidateSolutionResultDto>.Success(dto, "Đã chấm lời giải thành công.");
    }

    private async Task<bool> IsMapOwnedByUserAsync(Game game, Guid userId, CancellationToken cancellationToken)
    {
        if (game.CreatedBy.HasValue && game.CreatedBy.Value == userId)
            return true;

        var purchased = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .AnyAsync(p => !p.IsDeleted && p.UserId == userId && p.GameId == game.Id && p.PaymentStatus == PaymentStatusEnum.Completed, cancellationToken);
        if (purchased)
            return true;

        return await _unitOfWork.Repository<MyGame>().GetQueryable()
            .AnyAsync(mm => !mm.IsDeleted && mm.UserId == userId && mm.GameId == game.Id, cancellationToken);
    }

    /// <summary>Null náº¿u game nhiá»u level mÃ  khÃ´ng gá»­i GameDetailId há»£p lá»‡.</summary>
    private static GameDetail? ResolveGameDetail(Guid? requestedId, List<GameDetail> levelsOrdered)
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
        var s = ScoreSolvedRunFromMetrics(limits, elapsed, stepsUsed, blocksUsed, weights);
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
            var metadata = root.TryGetProperty("metadata", out var md) ? md : default;
            double time = inf, steps = inf, blocks = inf;

            if (mapConfig.TryGetProperty("timeLimitSeconds", out var t) && t.ValueKind == JsonValueKind.Number)
                time = t.GetDouble();
            if (mapConfig.TryGetProperty("estimatedSteps", out var es) && es.ValueKind == JsonValueKind.Number)
                steps = es.GetDouble();
            else if (metadata.ValueKind == JsonValueKind.Object &&
                     metadata.TryGetProperty("estimatedSteps", out var mes) &&
                     mes.ValueKind == JsonValueKind.Number)
                steps = mes.GetDouble();

            if (root.TryGetProperty("blockConstraints", out var bc) &&
                bc.TryGetProperty("blockLimit", out var bl) &&
                bl.ValueKind == JsonValueKind.Number)
                blocks = bl.GetDouble();
            else if (mapConfig.ValueKind == JsonValueKind.Object &&
                     mapConfig.TryGetProperty("blockConstraints", out var mbc) &&
                     mbc.TryGetProperty("blockLimit", out var mbl) &&
                     mbl.ValueKind == JsonValueKind.Number)
                blocks = mbl.GetDouble();

            return new MissionLimits(time, steps, blocks);
        }
        catch
        {
            return new MissionLimits(inf, inf, inf);
        }
    }

    /// <summary>Giá»‘ng GameResultsModal: 0â€“3 sao theo time / steps / block so vá»›i limit game.</summary>
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

    private static int ScoreSolvedRunFromMetrics(
        MissionLimits lim,
        double elapsedSeconds,
        int steps,
        int blocks,
        MapSolveScoreWeights w)
    {
        var timeRatio = GetEfficiencyRatio(elapsedSeconds, lim.TimeLimitSeconds, 300d);
        var stepsRatio = GetEfficiencyRatio(steps, lim.EstimatedSteps, 100d);
        var blocksRatio = GetEfficiencyRatio(blocks, lim.BlockLimit, 100d);

        var score = w.BaseScore
                    + (int)Math.Round(w.TimeScore * timeRatio)
                    + (int)Math.Round(w.StepsScore * stepsRatio)
                    + (int)Math.Round(w.BlocksScore * blocksRatio);
        return Math.Clamp(score, 0, 100);
    }

    private static double GetEfficiencyRatio(double actualValue, double limitValue, double fallbackScale)
    {
        var scale = limitValue > 0 && !double.IsInfinity(limitValue) ? limitValue : fallbackScale;
        if (scale <= 0 || double.IsInfinity(scale))
            return 0;

        // Two-slope linear model (stricter):
        // - Best zone: full score from 0..50% of limit
        // - Competitive zone: still under limit, but linearly reduced to 70% at exactly limit
        // - Over-limit zone: linearly drops to 0 at 200% of limit
        const double fullScoreAtLimitRatio = 0.50;
        const double scoreRatioAtLimit = 0.70;

        var fullScoreAt = scale * fullScoreAtLimitRatio;
        var zeroScoreAt = scale * 2.0;

        if (actualValue <= fullScoreAt)
            return 1.0;

        if (actualValue <= scale)
        {
            var t = (actualValue - fullScoreAt) / (scale - fullScoreAt);
            return Math.Clamp(1.0 - t * (1.0 - scoreRatioAtLimit), 0.0, 1.0);
        }

        var overT = (actualValue - scale) / (zeroScoreAt - scale);
        return Math.Clamp(scoreRatioAtLimit * (1.0 - overT), 0.0, 1.0);
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

        public static MapSolveScoreWeights FromDbOrLegacy(GameSolveScoreConfig? cfg)
        {
            if (cfg == null) return Legacy;
            if (cfg.BaseScore + cfg.TimeScore + cfg.StepsScore + cfg.BlocksScore != 100) return Legacy;
            return new MapSolveScoreWeights(cfg.BaseScore, cfg.TimeScore, cfg.StepsScore, cfg.BlocksScore);
        }
    }
}





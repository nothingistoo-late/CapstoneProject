using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Recommendations.DTOs;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Recommendations.Queries.GetRecommendations;

public class GetRecommendationsQueryHandler : IRequestHandler<GetRecommendationsQuery, Result<RecommendationResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetRecommendationsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<RecommendationResultDto>> Handle(GetRecommendationsQuery request, CancellationToken cancellationToken)
    {
        // Controller đã chặn bằng AuthorizeRoles, nhưng vẫn validate lại để trả ErrorCode consistent.
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<RecommendationResultDto>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var userIdValue = userId.Value;

        // Selected learning goal (latest selection)
        var userGoal = await _unitOfWork.Repository<UserLearningGoal>().GetQueryable()
            .Where(ug => ug.UserId == userIdValue && !ug.IsDeleted)
            .OrderByDescending(ug => ug.SelectedAt)
            .AsNoTracking()
            .Select(ug => ug.LearningGoalId)
            .FirstOrDefaultAsync(cancellationToken);

        if (userGoal == Guid.Empty)
            return Result<RecommendationResultDto>.Success(new RecommendationResultDto(), "Retrieved successfully");

        var learningGoalId = userGoal;

        // Load learning path items (concepts + maps) for this goal
        var pathItems = await _unitOfWork.Repository<LearningPathItem>().GetQueryable()
            .Where(i => i.LearningGoalId == learningGoalId && !i.IsDeleted)
            .OrderBy(i => i.SortOrder)
            .Select(i => new
            {
                i.ItemType,
                i.ConceptId,
                i.MapId,
                i.SortOrder
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var conceptItems = pathItems
            .Where(i => i.ItemType == LearningPathItemTypeEnum.Concept && i.ConceptId.HasValue)
            .Select(i => new { ConceptId = i.ConceptId!.Value, i.SortOrder })
            .Distinct()
            .OrderBy(x => x.SortOrder)
            .ToList();

        var mapIds = pathItems
            .Where(i => i.ItemType == LearningPathItemTypeEnum.Map && i.MapId.HasValue)
            .Select(i => i.MapId!.Value)
            .Distinct()
            .ToList();

        if (mapIds.Count == 0 || conceptItems.Count == 0)
            return Result<RecommendationResultDto>.Success(new RecommendationResultDto(), "Retrieved successfully");

        var firstConceptId = conceptItems.First().ConceptId;

        // Preload concept metadata
        var conceptIdList = conceptItems.Select(x => x.ConceptId).ToList();
        var concepts = await _unitOfWork.Repository<Concept>().GetQueryable()
            .Where(c => conceptIdList.Contains(c.Id) && !c.IsDeleted)
            .Select(c => new { c.Id, c.Name, c.SortOrder })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var conceptById = concepts.ToDictionary(c => c.Id, c => c);

        // Preload user concept completion
        var userConceptProgress = await _unitOfWork.Repository<UserConceptProgress>().GetQueryable()
            .Where(p => p.UserId == userIdValue && conceptIdList.Contains(p.ConceptId) && !p.IsDeleted)
            .Select(p => new { p.ConceptId, p.IsCompleted })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var conceptCompletionById = userConceptProgress.ToDictionary(x => x.ConceptId, x => x.IsCompleted);

        // nextConceptId: concept after the last completed concept in the path
        Guid? nextConceptId = null;
        var lastCompletedSort = -1;
        foreach (var ci in conceptItems)
        {
            var completed = conceptCompletionById.TryGetValue(ci.ConceptId, out var isCompleted) && isCompleted;
            if (completed) lastCompletedSort = Math.Max(lastCompletedSort, ci.SortOrder);
        }

        var nextConceptItem = conceptItems.FirstOrDefault(ci => ci.SortOrder > lastCompletedSort);
        if (nextConceptItem != null)
            nextConceptId = nextConceptItem.ConceptId;

        var nextConceptDto = nextConceptId.HasValue && conceptById.TryGetValue(nextConceptId.Value, out var nextConcept)
            ? new RecommendationConceptDto
            {
                ConceptId = nextConceptId.Value,
                Name = nextConcept.Name,
                SortOrder = nextConcept.SortOrder
            }
            : null;

        // MapId -> ConceptId mapping using path order:
        // a map belongs to the latest concept before it in the path
        var mapToConceptId = new Dictionary<Guid, Guid?>();
        Guid? lastConceptId = null;
        foreach (var item in pathItems)
        {
            if (item.ItemType == LearningPathItemTypeEnum.Concept && item.ConceptId.HasValue)
            {
                lastConceptId = item.ConceptId.Value;
            }
            else if (item.ItemType == LearningPathItemTypeEnum.Map && item.MapId.HasValue)
            {
                mapToConceptId[item.MapId.Value] = lastConceptId ?? firstConceptId;
            }
        }

        // Preload map metadata
        var maps = await _unitOfWork.Repository<Map>().GetQueryable()
            .Where(m => mapIds.Contains(m.Id) && !m.IsDeleted && m.Status == EntityStatusEnum.Active)
            .Select(m => new { m.Id, m.Title, m.Difficulty })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var mapById = maps.ToDictionary(m => m.Id, m => m);
        if (mapById.Count == 0)
            return Result<RecommendationResultDto>.Success(new RecommendationResultDto(), "Retrieved successfully");

        // Preload user play history for these maps (single batch)
        var nowUtc = DateTime.UtcNow;
        var recentFrom = nowUtc.AddDays(-7);
        var relevantMapIds = mapById.Keys.ToList();

        var histories = await _unitOfWork.Repository<UserMapPlayHistory>().GetQueryable()
            .Where(h => h.UserId == userIdValue && relevantMapIds.Contains(h.MapId) && !h.IsDeleted)
            .Select(h => new
            {
                h.MapId,
                h.IsCompleted,
                h.StartTime
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var mapAttempts = new Dictionary<Guid, int>();
        var mapFailCount = new Dictionary<Guid, int>();
        var mapRecentCount = new Dictionary<Guid, int>();

        foreach (var h in histories)
        {
            mapAttempts[h.MapId] = mapAttempts.TryGetValue(h.MapId, out var a) ? a + 1 : 1;

            if (!h.IsCompleted)
                mapFailCount[h.MapId] = mapFailCount.TryGetValue(h.MapId, out var f) ? f + 1 : 1;

            if (h.StartTime >= recentFrom)
                mapRecentCount[h.MapId] = mapRecentCount.TryGetValue(h.MapId, out var r) ? r + 1 : 1;
        }

        // Concept failure aggregation from map failures
        var conceptFailCount = new Dictionary<Guid, int>();
        foreach (var mapId in mapById.Keys)
        {
            var failCount = mapFailCount.TryGetValue(mapId, out var fc) ? fc : 0;
            var conceptId = mapToConceptId.TryGetValue(mapId, out var cId) ? cId : null;
            if (!conceptId.HasValue) continue;

            conceptFailCount[conceptId.Value] = conceptFailCount.TryGetValue(conceptId.Value, out var prev)
                ? prev + failCount
                : failCount;
        }

        // Weak concept: highest failure count
        Guid? weakConceptId = null;
        var weakFail = int.MinValue;
        foreach (var ci in conceptItems)
        {
            var fail = conceptFailCount.TryGetValue(ci.ConceptId, out var f) ? f : 0;
            if (fail > weakFail)
            {
                weakFail = fail;
                weakConceptId = ci.ConceptId;
            }
        }

        // Build candidate map sets
        var reviewMapsIds = mapById.Keys
            .Where(mid => (mapFailCount.TryGetValue(mid, out var fc) ? fc : 0) >= 3)
            .ToHashSet();

        var nextMaps = nextConceptId.HasValue
            ? mapById.Keys.Where(mid => mapToConceptId.TryGetValue(mid, out var cId) && cId == nextConceptId.Value).ToList()
            : new List<Guid>();

        var suggestedPracticeMaps = weakConceptId.HasValue
            ? mapById.Keys.Where(mid => mapToConceptId.TryGetValue(mid, out var cId) && cId == weakConceptId.Value).ToList()
            : new List<Guid>();

        // Suggested result projection + scoring helpers
        var attemptedDifficulty = mapAttempts.Count > 0
            ? mapAttempts.Keys.Where(mid => mapById.ContainsKey(mid))
                .Select(mid => mapById[mid].Difficulty)
                .ToList()
            : new List<int>();

        var avgDifficulty = attemptedDifficulty.Count > 0 ? attemptedDifficulty.Average() : 0;
        var minDifficulty = attemptedDifficulty.Count > 0 ? attemptedDifficulty.Min() : 0;
        var maxDifficulty = attemptedDifficulty.Count > 0 ? attemptedDifficulty.Max() : 0;
        var difficultyRange = Math.Max(1, maxDifficulty - minDifficulty);

        double GetSuccessRate(Guid mapId)
        {
            var attempts = mapAttempts.TryGetValue(mapId, out var a) ? a : 0;
            var fails = mapFailCount.TryGetValue(mapId, out var f) ? f : 0;
            return attempts > 0 ? (double)(attempts - fails) / attempts : 0;
        }

        double GetRecentActivityScore(Guid mapId)
        {
            var recentCount = mapRecentCount.TryGetValue(mapId, out var rc) ? rc : 0;
            // Normalize: assume 3 recent attempts ~= 1.0
            return Math.Clamp(recentCount / 3.0, 0, 1);
        }

        RecommendationMapDto CreateMapDto(Guid mapId, double? score = null)
        {
            var m = mapById[mapId];
            var conceptId = mapToConceptId.TryGetValue(mapId, out var cId) ? cId : null;
            conceptById.TryGetValue(conceptId ?? Guid.Empty, out var c);
            var conceptName = conceptId.HasValue ? (conceptById.TryGetValue(conceptId.Value, out var cc) ? cc.Name : null) : null;

            var attempts = mapAttempts.TryGetValue(mapId, out var a) ? a : 0;
            var fails = mapFailCount.TryGetValue(mapId, out var f) ? f : 0;
            var successRate = GetSuccessRate(mapId);

            return new RecommendationMapDto
            {
                MapId = mapId,
                Title = m.Title,
                Difficulty = m.Difficulty,
                ConceptId = conceptId,
                ConceptName = conceptName,
                Score = score,
                Attempts = attempts,
                FailCount = fails,
                SuccessRate = successRate
            };
        }

        // Review maps list (fail >= 3)
        var reviewMaps = reviewMapsIds
            .Select(mid => new { mid, Fail = mapFailCount.TryGetValue(mid, out var f) ? f : 0, Attempts = mapAttempts.TryGetValue(mid, out var a) ? a : 0 })
            .OrderByDescending(x => x.Fail)
            .ThenByDescending(x => x.Attempts)
            .Take(20)
            .Select(x => CreateMapDto(x.mid))
            .ToList();

        // Recommended maps: scoring on union(next + suggestedPractice)
        var candidateMapIds = new HashSet<Guid>(nextMaps);
        foreach (var id in suggestedPracticeMaps) candidateMapIds.Add(id);

        var desiredConceptId = weakConceptId ?? nextConceptId;
        var scoredCandidates = candidateMapIds
            .Select(mid =>
            {
                var conceptId = mapToConceptId.TryGetValue(mid, out var cId) ? cId : null;
                var matchConcept = desiredConceptId.HasValue && conceptId == desiredConceptId.Value ? 1.0 : 0.0;

                var difficultyDiff = Math.Abs(mapById[mid].Difficulty - avgDifficulty);
                var difficultyFit = 1.0 - Math.Min(1.0, difficultyDiff / difficultyRange);
                difficultyFit = Math.Clamp(difficultyFit, 0, 1);

                var userPerformance = Math.Clamp(GetSuccessRate(mid), 0, 1);
                var recentActivity = Math.Clamp(GetRecentActivityScore(mid), 0, 1);

                // score = (match_concept * 0.4) * (difficulty_fit * 0.2) * (user_performance * 0.2) * (recent_activity * 0.2)
                var score = (matchConcept * 0.4) *
                            (difficultyFit * 0.2) *
                            (userPerformance * 0.2) *
                            (recentActivity * 0.2);

                var failCount = mapFailCount.TryGetValue(mid, out var fc) ? fc : 0;
                var difficulty = mapById[mid].Difficulty;

                return new { mid, score, failCount, difficulty };
            })
            .OrderByDescending(x => x.score)
            .ThenByDescending(x => x.failCount)
            .ThenBy(x => x.difficulty)
            .Take(10)
            .ToList();

        var recommendedMaps = scoredCandidates.Select(x => CreateMapDto(x.mid, x.score)).ToList();

        // Suggested practice maps list (weak concept)
        var suggestedPracticeList = suggestedPracticeMaps
            .Select(mid => new
            {
                mid,
                Fail = mapFailCount.TryGetValue(mid, out var f) ? f : 0,
                Difficulty = mapById[mid].Difficulty
            })
            .OrderByDescending(x => x.Fail)
            .ThenBy(x => x.Difficulty)
            .Take(20)
            .Select(x => CreateMapDto(x.mid))
            .ToList();

        return Result<RecommendationResultDto>.Success(new RecommendationResultDto
        {
            RecommendedMaps = recommendedMaps,
            ReviewMaps = reviewMaps,
            SuggestedPracticeMaps = suggestedPracticeList,
            NextConcept = nextConceptDto
        }, "Retrieved successfully");
    }
}


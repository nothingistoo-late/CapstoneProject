using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetMyLearningPathProgress;

public class GetMyLearningPathProgressQueryHandler : IRequestHandler<GetMyLearningPathProgressQuery, Result<LearningPathProgressDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyLearningPathProgressQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<LearningPathProgressDto>> Handle(GetMyLearningPathProgressQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<LearningPathProgressDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var dto = new LearningPathProgressDto();
        var userGoal = await _unitOfWork.Repository<UserLearningGoal>().GetQueryable()
            .Where(ug => ug.UserId == userId.Value && !ug.IsDeleted)
            .Include(ug => ug.LearningGoal)
            .OrderByDescending(ug => ug.SelectedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (userGoal?.LearningGoal == null)
            return Result<LearningPathProgressDto>.Success(dto, "Đã lấy tiến độ lộ trình học tập của bạn.");

        var goal = userGoal.LearningGoal;
        dto.LearningGoalId = goal.Id;
        dto.LearningGoalName = goal.Name;

        var pathItems = await _unitOfWork.Repository<LearningPathItem>().GetQueryable()
            .Where(i => i.LearningGoalId == goal.Id && !i.IsDeleted)
            .ToListAsync(cancellationToken);

        dto.TotalItems = pathItems.Count;
        if (dto.TotalItems == 0)
            return Result<LearningPathProgressDto>.Success(dto, "Đã lấy tiến độ lộ trình học tập của bạn.");

        var conceptIds = pathItems.Where(i => i.ConceptId.HasValue).Select(i => i.ConceptId!.Value).ToList();
        var gameIds = pathItems.Where(i => i.GameId.HasValue).Select(i => i.GameId!.Value).ToList();

        var completedConcepts = await _unitOfWork.Repository<UserConceptProgress>().GetQueryable()
            .CountAsync(p => p.UserId == userId.Value && !p.IsDeleted && p.IsCompleted && conceptIds.Contains(p.ConceptId), cancellationToken);

        var completedMaps = await _unitOfWork.Repository<UserGameResult>().GetQueryable()
            .CountAsync(r => r.UserId == userId.Value && !r.IsDeleted && r.BestStars >= 1 && gameIds.Contains(r.GameId), cancellationToken);

        dto.CompletedCount = completedConcepts + completedMaps;
        dto.PercentComplete = (int)Math.Round(100.0 * dto.CompletedCount / dto.TotalItems);

        var mapResults = await _unitOfWork.Repository<UserGameResult>().GetQueryable()
            .Where(r => r.UserId == userId.Value && !r.IsDeleted && gameIds.Contains(r.GameId))
            .Select(r => new { r.GameId, r.BestStars, r.BestScore })
            .ToListAsync(cancellationToken);

        foreach (var gameId in gameIds)
        {
            var mr = mapResults.FirstOrDefault(r => r.GameId == gameId);
            if (mr == null || mr.BestStars < 2)
                dto.SuggestedReviewGameIds.Add(gameId);
        }

        return Result<LearningPathProgressDto>.Success(dto, "Đã lấy tiến độ lộ trình học tập của bạn.");
    }
}

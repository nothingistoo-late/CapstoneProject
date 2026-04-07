using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetMyLearningPath;

public class GetMyLearningPathQueryHandler : IRequestHandler<GetMyLearningPathQuery, Result<MyLearningPathDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyLearningPathQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MyLearningPathDto>> Handle(GetMyLearningPathQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<MyLearningPathDto>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để xem lộ trình học tập của bạn.", ErrorCodeEnum.Unauthorized);

        var userGoal = await _unitOfWork.Repository<UserLearningGoal>().GetQueryable()
            .Where(ug => ug.UserId == userId.Value && !ug.IsDeleted)
            .Include(ug => ug.LearningGoal)
            .OrderByDescending(ug => ug.SelectedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new MyLearningPathDto();
        if (userGoal?.LearningGoal == null)
        {
            return Result<MyLearningPathDto>.Success(dto, "Đã lấy lộ trình học tập của bạn.");
        }

        var goal = userGoal.LearningGoal;
        dto.LearningGoalId = goal.Id;
        dto.LearningGoalName = goal.Name;
        dto.LearningGoalDescription = goal.Description;

        var pathItems = await _unitOfWork.Repository<LearningPathItem>().GetQueryable()
            .Where(i => i.LearningGoalId == goal.Id && !i.IsDeleted)
            .Include(i => i.Concept)
            .Include(i => i.Map)
            .OrderBy(i => i.SortOrder)
            .ToListAsync(cancellationToken);

        var conceptProgress = await _unitOfWork.Repository<UserConceptProgress>().GetQueryable()
            .Where(p => p.UserId == userId.Value && !p.IsDeleted && p.IsCompleted)
            .Select(p => p.ConceptId)
            .ToListAsync(cancellationToken);

        var completedMapIds = await _unitOfWork.Repository<UserMapResult>().GetQueryable()
            .Where(r => r.UserId == userId.Value && !r.IsDeleted && r.BestStars >= 1)
            .Select(r => r.MapId)
            .ToListAsync(cancellationToken);

        var completedConceptSet = conceptProgress.ToHashSet();
        var completedMapSet = completedMapIds.ToHashSet();

        bool allPreviousCompleted = true;
        foreach (var item in pathItems)
        {
            bool completed = item.ItemType == LearningPathItemTypeEnum.Concept
                ? item.ConceptId.HasValue && completedConceptSet.Contains(item.ConceptId.Value)
                : item.MapId.HasValue && completedMapSet.Contains(item.MapId.Value);

            bool unlocked = allPreviousCompleted;
            if (!completed)
                allPreviousCompleted = false;

            int? bestStars = null;
            DateTime? completedAt = null;
            if (item.ItemType == LearningPathItemTypeEnum.Map && item.MapId.HasValue)
            {
                var umr = await _unitOfWork.Repository<UserMapResult>().GetQueryable()
                    .Where(r => r.UserId == userId.Value && r.MapId == item.MapId.Value && !r.IsDeleted)
                    .Select(r => new { r.BestStars, r.LastPlayedAt })
                    .FirstOrDefaultAsync(cancellationToken);
                if (umr != null)
                {
                    bestStars = umr.BestStars;
                    completedAt = umr.LastPlayedAt;
                }
            }
            else if (item.ItemType == LearningPathItemTypeEnum.Concept && item.ConceptId.HasValue)
            {
                var ucp = await _unitOfWork.Repository<UserConceptProgress>().GetQueryable()
                    .Where(p => p.UserId == userId.Value && p.ConceptId == item.ConceptId.Value && !p.IsDeleted && p.IsCompleted)
                    .Select(p => p.CompletedAt)
                    .FirstOrDefaultAsync(cancellationToken);
                completedAt = ucp;
            }

            dto.Items.Add(new LearningPathItemDto
            {
                ItemId = item.Id,
                ItemType = item.ItemType.ToString(),
                SortOrder = item.SortOrder,
                ConceptId = item.ConceptId,
                ConceptName = item.Concept?.Name,
                ConceptDescription = item.Concept?.Description,
                ConceptContentKey = item.Concept?.ContentKey,
                MapId = item.MapId,
                MapTitle = item.Map?.Title,
                MapDescription = item.Map?.Description,
                MapDifficulty = item.Map?.Difficulty,
                MapAvatarUrl = item.Map?.AvatarUrl,
                IsCompleted = completed,
                IsUnlocked = unlocked,
                BestStars = bestStars,
                CompletedAt = completedAt
            });
        }

        return Result<MyLearningPathDto>.Success(dto, "Đã lấy lộ trình học tập của bạn.");
    }
}

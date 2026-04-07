using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetPathItemsByGoal;

public class GetPathItemsByGoalQueryHandler : IRequestHandler<GetPathItemsByGoalQuery, Result<List<PathItemPreviewDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPathItemsByGoalQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<PathItemPreviewDto>>> Handle(GetPathItemsByGoalQuery request, CancellationToken cancellationToken)
    {
        var goalExists = await _unitOfWork.Repository<LearningGoal>().GetQueryable()
            .AnyAsync(g => g.Id == request.LearningGoalId && !g.IsDeleted && g.Status == EntityStatusEnum.Active, cancellationToken);
        if (!goalExists)
            return Result<List<PathItemPreviewDto>>.Failure("Không tìm thấy mục tiêu học tập.", ErrorCodeEnum.NotFound);

        var items = await _unitOfWork.Repository<LearningPathItem>().GetQueryable()
            .Where(i => i.LearningGoalId == request.LearningGoalId && !i.IsDeleted)
            .OrderBy(i => i.SortOrder)
            .Select(i => new PathItemPreviewDto
            {
                ItemId = i.Id,
                ItemType = i.ItemType.ToString(),
                SortOrder = i.SortOrder,
                ConceptId = i.ConceptId,
                ConceptName = i.Concept != null ? i.Concept.Name : null,
                ConceptDescription = i.Concept != null ? i.Concept.Description : null,
                ConceptContentKey = i.Concept != null ? i.Concept.ContentKey : null,
                MapId = i.MapId,
                MapTitle = i.Map != null ? i.Map.Title : null,
                MapDescription = i.Map != null ? i.Map.Description : null,
                MapDifficulty = i.Map != null ? i.Map.Difficulty : null,
                MapAvatarUrl = i.Map != null ? i.Map.AvatarUrl : null
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result<List<PathItemPreviewDto>>.Success(items);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetLearningGoals;

public class GetLearningGoalsQueryHandler : IRequestHandler<GetLearningGoalsQuery, Result<List<LearningGoalDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetLearningGoalsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<LearningGoalDto>>> Handle(GetLearningGoalsQuery request, CancellationToken cancellationToken)
    {
        var list = await _unitOfWork.Repository<LearningGoal>().GetQueryable()
            .Where(g => !g.IsDeleted && g.Status == EntityStatusEnum.Active)
            .OrderBy(g => g.SortOrder)
            .Select(g => new LearningGoalDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                SortOrder = g.SortOrder,
                IconUrl = g.IconUrl
            })
            .ToListAsync(cancellationToken);
        return Result<List<LearningGoalDto>>.Success(list);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using CapstoneProject.Application.Features.LearningPath.Queries.GetLearningGoals;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetLearningGoalById;

public class GetLearningGoalByIdQueryHandler : IRequestHandler<GetLearningGoalByIdQuery, Result<LearningGoalDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetLearningGoalByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<LearningGoalDto>> Handle(GetLearningGoalByIdQuery request, CancellationToken cancellationToken)
    {
        var goal = await _unitOfWork.Repository<LearningGoal>().GetQueryable()
            .Where(g => g.Id == request.GoalId && !g.IsDeleted && g.Status == EntityStatusEnum.Active)
            .Select(g => new LearningGoalDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                SortOrder = g.SortOrder,
                IconUrl = g.IconUrl
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (goal == null)
            return Result<LearningGoalDto>.Failure("Không tìm thấy mục tiêu học tập.", ErrorCodeEnum.NotFound);

        return Result<LearningGoalDto>.Success(goal, "Đã lấy chi tiết mục tiêu học tập.");
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetSelectedGoal;

public class GetSelectedGoalQueryHandler : IRequestHandler<GetSelectedGoalQuery, Result<SelectedGoalDto?>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetSelectedGoalQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SelectedGoalDto?>> Handle(GetSelectedGoalQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<SelectedGoalDto?>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var userGoal = await _unitOfWork.Repository<UserLearningGoal>().GetQueryable()
            .Where(ug => ug.UserId == userId.Value && !ug.IsDeleted)
            .OrderByDescending(ug => ug.SelectedAt)
            .Select(ug => new SelectedGoalDto
            {
                LearningGoalId = ug.LearningGoalId,
                LearningGoalName = ug.LearningGoal != null ? ug.LearningGoal.Name : "",
                LearningGoalDescription = ug.LearningGoal != null ? ug.LearningGoal.Description : null
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return Result<SelectedGoalDto?>.Success(userGoal);
    }
}

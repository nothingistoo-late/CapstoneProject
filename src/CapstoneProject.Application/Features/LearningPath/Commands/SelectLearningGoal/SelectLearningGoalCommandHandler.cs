using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.LearningPath.Commands.SelectLearningGoal;

public class SelectLearningGoalCommandHandler : IRequestHandler<SelectLearningGoalCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SelectLearningGoalCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(SelectLearningGoalCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result.Failure("Authentication required. Please log in to select a learning goal.", ErrorCodeEnum.Unauthorized);

        var goalExists = await _unitOfWork.Repository<LearningGoal>().GetQueryable()
            .AnyAsync(g => g.Id == request.LearningGoalId && !g.IsDeleted && g.Status == EntityStatusEnum.Active, cancellationToken);
        if (!goalExists)
            return Result.Failure("Learning goal not found.", ErrorCodeEnum.NotFound);

        var repo = _unitOfWork.Repository<UserLearningGoal>();
        var existing = await repo.GetQueryable()
            .FirstOrDefaultAsync(ug => ug.UserId == userId.Value && !ug.IsDeleted, cancellationToken);

        if (existing != null)
        {
            existing.LearningGoalId = request.LearningGoalId;
            existing.SelectedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
            existing.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
            existing.UpdatedBy = userId;
            repo.Update(existing);
        }
        else
        {
            var userGoal = new UserLearningGoal
            {
                UserId = userId.Value,
                LearningGoalId = request.LearningGoalId,
                SelectedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow
            };
            userGoal.InitializeEntity(userId);
            await repo.AddAsync(userGoal);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Learning goal selected. Your path has been updated.");
    }
}




using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Games.Commands.DeleteGameReviewCriterion;

public class DeleteGameReviewCriterionCommandHandler : IRequestHandler<DeleteGameReviewCriterionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteGameReviewCriterionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteGameReviewCriterionCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin))
            return Result.Failure("Chỉ Admin có thể xóa tiêu chí duyệt game.", ErrorCodeEnum.Forbidden);

        var entity = await _unitOfWork.Repository<GameReviewCriterionCatalog>().GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (entity == null)
            return Result.Failure("Không tìm thấy tiêu chí.", ErrorCodeEnum.NotFound);

        entity.SoftDeleteEntity(userIdNullable.Value);
        _unitOfWork.Repository<GameReviewCriterionCatalog>().Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Đã xóa tiêu chí.");
    }
}

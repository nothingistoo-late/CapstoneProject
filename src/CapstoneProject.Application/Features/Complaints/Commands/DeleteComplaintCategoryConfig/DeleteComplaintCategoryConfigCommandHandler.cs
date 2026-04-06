using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Commands.DeleteComplaintCategoryConfig;

public class DeleteComplaintCategoryConfigCommandHandler : IRequestHandler<DeleteComplaintCategoryConfigCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteComplaintCategoryConfigCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteComplaintCategoryConfigCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result.Failure("Only Admin/Moderator can delete complaint category configs.", ErrorCodeEnum.Forbidden);

        if (string.IsNullOrWhiteSpace(request.CategoryKey))
            return Result.Failure("CategoryKey is required.", ErrorCodeEnum.ValidationFailed);

        var row = await _unitOfWork.Repository<ComplaintCategoryCatalog>().GetQueryable()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.CategoryKey == request.CategoryKey.Trim(), cancellationToken);
        if (row == null)
            return Result.Failure("Complaint category config not found.", ErrorCodeEnum.NotFound);

        row.SoftDeleteEntity(userIdNullable.Value);
        row.UpdateEntity(userIdNullable.Value);
        _unitOfWork.Repository<ComplaintCategoryCatalog>().Update(row);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Complaint category config deleted.");
    }
}

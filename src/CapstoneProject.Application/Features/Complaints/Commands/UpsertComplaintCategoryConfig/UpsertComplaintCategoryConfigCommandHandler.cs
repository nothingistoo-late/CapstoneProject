using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Commands.UpsertComplaintCategoryConfig;

public class UpsertComplaintCategoryConfigCommandHandler : IRequestHandler<UpsertComplaintCategoryConfigCommand, Result<UpsertComplaintCategoryConfigDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpsertComplaintCategoryConfigCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UpsertComplaintCategoryConfigDto>> Handle(UpsertComplaintCategoryConfigCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<UpsertComplaintCategoryConfigDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<UpsertComplaintCategoryConfigDto>.Failure("Chỉ Quản trị viên/Người điều hành mới có thể cập nhật cấu hình danh mục khiếu nại.", ErrorCodeEnum.Forbidden);

        if (string.IsNullOrWhiteSpace(request.CategoryKey))
            return Result<UpsertComplaintCategoryConfigDto>.Failure("CategoryKey là bắt buộc.", ErrorCodeEnum.ValidationFailed);
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            return Result<UpsertComplaintCategoryConfigDto>.Failure("Tên hiển thị là bắt buộc.", ErrorCodeEnum.ValidationFailed);

        var userId = userIdNullable.Value;
        var categoryKey = request.CategoryKey.Trim();

        var repo = _unitOfWork.Repository<ComplaintCategoryCatalog>();
        var row = await repo.GetQueryable().FirstOrDefaultAsync(x => x.CategoryKey == categoryKey, cancellationToken);

        if (row == null)
        {
            row = new ComplaintCategoryCatalog
            {
                CategoryKey = categoryKey,
                DisplayName = request.DisplayName.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                IsEnabled = request.IsEnabled,
                SortOrder = request.SortOrder,
                Status = EntityStatusEnum.Active
            };
            row.InitializeEntity(userId);
            await repo.AddAsync(row);
        }
        else
        {
            row.DisplayName = request.DisplayName.Trim();
            row.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            row.IsEnabled = request.IsEnabled;
            row.SortOrder = request.SortOrder;
            if (row.IsDeleted)
                row.RestoreEntity(userId);
            row.UpdateEntity(userId);
            repo.Update(row);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var result = new UpsertComplaintCategoryConfigDto
        {
            CategoryKey = row.CategoryKey,
            DisplayName = row.DisplayName,
            Description = row.Description,
            IsEnabled = row.IsEnabled,
            SortOrder = row.SortOrder
        };

        return Result<UpsertComplaintCategoryConfigDto>.Success(result, "Đã lưu cấu hình danh mục khiếu nại.");
    }
}

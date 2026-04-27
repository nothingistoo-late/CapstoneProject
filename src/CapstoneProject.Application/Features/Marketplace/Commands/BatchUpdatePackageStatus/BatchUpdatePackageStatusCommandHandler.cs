using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Marketplace.Commands.BatchUpdatePackageStatus;

public class BatchUpdatePackageStatusCommandHandler : IRequestHandler<BatchUpdatePackageStatusCommand, Result<BatchUpdatePackageStatusResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public BatchUpdatePackageStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<BatchUpdatePackageStatusResultDto>> Handle(BatchUpdatePackageStatusCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<BatchUpdatePackageStatusResultDto>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để cập nhật trạng thái gói.", ErrorCodeEnum.Unauthorized);
        if (!(await _currentUserService.GetCurrentRolesAsync()).Contains(RoleEnum.Admin))
            return Result<BatchUpdatePackageStatusResultDto>.Failure("Bạn không có quyền cập nhật trạng thái gói. Chỉ Quản trị viên mới có thể thực hiện hành động này.", ErrorCodeEnum.Forbidden);

        var repo = _unitOfWork.Repository<Package>();
        var toUpdate = await repo.GetQueryable()
            .Where(p => command.PackageIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync(cancellationToken);
        var foundIds = toUpdate.Select(p => p.Id).ToHashSet();
        var notFoundIds = command.PackageIds.Where(id => !foundIds.Contains(id)).ToList();

        var newStatus = command.IsActive ? EntityStatusEnum.Active : EntityStatusEnum.Inactive;
        foreach (var pkg in toUpdate)
        {
            if (newStatus == EntityStatusEnum.Active)
            {
                if (pkg.Price < 0 || pkg.DurationDays <= 0 || string.IsNullOrWhiteSpace(pkg.FeaturesSpec))
                    continue;
            }

            pkg.Status = newStatus;
            pkg.UpdateEntity(userIdNullable.Value);
            repo.Update(pkg);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var successCount = toUpdate.Count(p => p.Status == newStatus);
        var dto = new BatchUpdatePackageStatusResultDto
        {
            SuccessCount = successCount,
            FailedCount = notFoundIds.Count + (toUpdate.Count - successCount),
            NotFoundIds = notFoundIds
        };
        return Result<BatchUpdatePackageStatusResultDto>.Success(dto, $"Đã cập nhật (các) gói {dto.SuccessCount}.");
    }
}

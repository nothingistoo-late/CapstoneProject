using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Marketplace.Commands.DeletePackage;

public class DeletePackageCommandHandler : IRequestHandler<DeletePackageCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeletePackageCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeletePackageCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để xóa một gói.", ErrorCodeEnum.Unauthorized);
        if (!(await _currentUserService.GetCurrentRolesAsync()).Contains(RoleEnum.Admin))
            return Result.Failure("Bạn không có quyền xóa các gói. Chỉ Quản trị viên mới có thể thực hiện hành động này.", ErrorCodeEnum.Forbidden);

        var pkg = await _unitOfWork.Repository<Package>().GetQueryable().FirstOrDefaultAsync(p => p.Id == command.PackageId && !p.IsDeleted, cancellationToken);
        if (pkg == null)
            return Result.Failure($"Không tìm thấy gói có Id: {command.PackageId}. Gói có thể đã bị xóa hoặc không tồn tại.", ErrorCodeEnum.NotFound);

        pkg.SoftDeleteEntity(userIdNullable.Value);
        _unitOfWork.Repository<Package>().Update(pkg);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Đã xóa gói.");
    }
}

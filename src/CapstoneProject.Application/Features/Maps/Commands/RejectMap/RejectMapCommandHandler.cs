using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Commands.RejectMap;

public class RejectMapCommandHandler : IRequestHandler<RejectMapCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RejectMapCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(RejectMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để thực hiện hành động này.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result.Failure("Bạn không có quyền từ chối bản đồ. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện hành động này.", ErrorCodeEnum.Forbidden);

        var mapRepo = _unitOfWork.Repository<Map>();
        var map = await mapRepo.GetQueryable().FirstOrDefaultAsync(m => m.Id == command.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result.Failure($"Không tìm thấy bản đồ có Id: {command.MapId}. Bản đồ có thể đã bị xóa hoặc không tồn tại.", ErrorCodeEnum.NotFound);
        if (map.MapStatus != MapStatusEnum.PendingReview)
            return Result.Failure($"Bản đồ không thể bị từ chối. Trạng thái dự kiến: Đang chờ xem xét. Trạng thái hiện tại: {map.MapStatus}. Chỉ những bản đồ đang chờ xem xét mới có thể bị từ chối.", ErrorCodeEnum.InvalidOperation);

        map.MapStatus = MapStatusEnum.Rejected;
        map.UpdateEntity(userIdNullable!.Value);
        mapRepo.Update(map);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Bản đồ đã bị từ chối thành công.");
    }
}

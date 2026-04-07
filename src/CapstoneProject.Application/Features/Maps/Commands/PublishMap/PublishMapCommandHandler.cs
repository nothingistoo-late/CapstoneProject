using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Commands.PublishMap;

public class PublishMapCommandHandler : IRequestHandler<PublishMapCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public PublishMapCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(PublishMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để xuất bản bản đồ.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        var isAdminOrModerator = roles.Contains(RoleEnum.Admin) || roles.Contains(RoleEnum.Moderator);
        var isLearner = roles.Contains(RoleEnum.Learner);

        var mapRepo = _unitOfWork.Repository<Map>();
        var map = await mapRepo.GetQueryable().FirstOrDefaultAsync(m => m.Id == command.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result.Failure($"Không tìm thấy bản đồ có Id: {command.MapId}. Bản đồ có thể đã bị xóa hoặc không tồn tại.", ErrorCodeEnum.NotFound);
        if (map.MapStatus != MapStatusEnum.Approved)
            return Result.Failure($"Bản đồ không thể được xuất bản. Trạng thái dự kiến: Đã phê duyệt. Trạng thái hiện tại: {map.MapStatus}. Chỉ những bản đồ được phê duyệt mới có thể được xuất bản.", ErrorCodeEnum.InvalidOperation);

        if (isAdminOrModerator)
        {
            // Staff can publish any approved map (Learner API or CMS).
        }
        else if (isLearner)
        {
            if (map.CreatedBy != userIdNullable.Value)
                return Result.Failure("Chỉ tác giả của bản đồ này mới có thể xuất bản nó.", ErrorCodeEnum.Forbidden);
        }
        else
            return Result.Failure("Bạn không có quyền xuất bản bản đồ.", ErrorCodeEnum.Forbidden);

        map.MapStatus = MapStatusEnum.Published;
        map.IsPublished = true;
        map.UpdateEntity(userIdNullable!.Value);
        mapRepo.Update(map);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Bản đồ được xuất bản thành công.");
    }
}

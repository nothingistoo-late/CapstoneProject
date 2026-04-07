using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Commands.DeleteMap;

public class DeleteMapCommandHandler : IRequestHandler<DeleteMapCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteMapCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để xóa bản đồ.", ErrorCodeEnum.Unauthorized);

        var map = await _unitOfWork.Repository<Map>().GetQueryable()
            .FirstOrDefaultAsync(m => m.Id == command.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result.Failure($"Không tìm thấy bản đồ có Id: {command.MapId}. Bản đồ có thể đã bị xóa hoặc không tồn tại.", ErrorCodeEnum.NotFound);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        var isAdminOrMod = roles.Contains(RoleEnum.Admin) || roles.Contains(RoleEnum.Moderator);
        if (map.CreatedBy != userIdNullable.Value && !isAdminOrMod)
            return Result.Failure("Bạn không có quyền xóa bản đồ này. Chỉ tác giả bản đồ hoặc Quản trị viên/Người điều hành mới có thể xóa nó.", ErrorCodeEnum.Forbidden);

        map.IsPublished = false;
        if (map.MapStatus == MapStatusEnum.Published)
            map.MapStatus = MapStatusEnum.Approved;

        map.SoftDeleteEntity(userIdNullable.Value);
        _unitOfWork.Repository<Map>().Update(map);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Bản đồ đã được xóa thành công.");
    }
}

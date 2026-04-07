using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Commands.SubmitMapForReview;

public class SubmitMapForReviewCommandHandler : IRequestHandler<SubmitMapForReviewCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SubmitMapForReviewCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(SubmitMapForReviewCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để gửi bản đồ để xem xét.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var mapRepo = _unitOfWork.Repository<Map>();
        var map = await mapRepo.GetQueryable().FirstOrDefaultAsync(m => m.Id == command.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result.Failure($"Không tìm thấy bản đồ có Id: {command.MapId}. Bản đồ có thể đã bị xóa hoặc không tồn tại.", ErrorCodeEnum.NotFound);
        if (map.CreatedBy != userId)
            return Result.Failure("Bạn chỉ có thể gửi bản đồ của riêng mình để xem xét. Bản đồ này được tạo bởi một người dùng khác.", ErrorCodeEnum.Forbidden);
        if (map.MapStatus != MapStatusEnum.Draft)
            return Result.Failure($"Bản đồ không thể được gửi để xem xét. Trạng thái dự kiến: Bản nháp. Trạng thái hiện tại: {map.MapStatus}. Chỉ có thể gửi bản đồ dự thảo.", ErrorCodeEnum.InvalidOperation);

        map.MapStatus = MapStatusEnum.PendingReview;
        map.UpdateEntity(userId);
        mapRepo.Update(map);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Bản đồ đã được gửi để xem xét thành công.");
    }
}

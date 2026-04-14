using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using System.Text.Json;

namespace CapstoneProject.Application.Features.Maps.Commands.RejectMap;

public class RejectMapCommandHandler : IRequestHandler<RejectMapCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationPersistenceService _notificationPersistenceService;

    public RejectMapCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        INotificationPersistenceService notificationPersistenceService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationPersistenceService = notificationPersistenceService;
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

        var normalizedRejectReason = NormalizeNote(command.RejectReason);
        map.MapStatus = MapStatusEnum.Rejected;
        map.ReviewNote = normalizedRejectReason;
        map.UpdateEntity(userIdNullable!.Value);
        mapRepo.Update(map);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (map.CreatedBy.HasValue)
        {
            try
            {
                var payloadJson = JsonSerializer.Serialize(new
                {
                    mapId = map.Id,
                    mapTitle = map.Title,
                    status = map.MapStatus.ToString(),
                    rejectReason = normalizedRejectReason
                });

                var body = string.IsNullOrWhiteSpace(normalizedRejectReason)
                    ? $"Map \"{map.Title}\" đã bị từ chối."
                    : $"Map \"{map.Title}\" đã bị từ chối. Lý do: {normalizedRejectReason}";

                await _notificationPersistenceService.CreateNotificationAsync(
                    NotificationTypeEnum.SystemAnnouncement,
                    "Map của bạn đã bị từ chối",
                    body,
                    new List<Guid> { map.CreatedBy.Value },
                    userIdNullable.Value,
                    payloadJson,
                    "/app/my-maps",
                    cancellationToken);
            }
            catch
            {
                // Notification failure must not break reject flow.
            }
        }

        return Result.Success("Bản đồ đã bị từ chối thành công.");
    }

    private static string? NormalizeNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return null;
        return note.Trim();
    }
}

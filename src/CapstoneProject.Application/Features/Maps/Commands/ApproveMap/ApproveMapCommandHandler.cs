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

namespace CapstoneProject.Application.Features.Maps.Commands.ApproveMap;

public class ApproveMapCommandHandler : IRequestHandler<ApproveMapCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationPersistenceService _notificationPersistenceService;

    public ApproveMapCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        INotificationPersistenceService notificationPersistenceService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationPersistenceService = notificationPersistenceService;
    }

    public async Task<Result> Handle(ApproveMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để thực hiện hành động này.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result.Failure("Bạn không có quyền phê duyệt bản đồ. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện hành động này.", ErrorCodeEnum.Forbidden);

        var mapRepo = _unitOfWork.Repository<Map>();
        var map = await mapRepo.GetQueryable().FirstOrDefaultAsync(m => m.Id == command.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result.Failure($"Không tìm thấy bản đồ có Id: {command.MapId}. Bản đồ có thể đã bị xóa hoặc không tồn tại.", ErrorCodeEnum.NotFound);
        if (map.MapStatus != MapStatusEnum.PendingReview)
            return Result.Failure($"Bản đồ không thể được phê duyệt. Trạng thái dự kiến: Đang chờ xem xét. Trạng thái hiện tại: {map.MapStatus}. Chỉ những bản đồ đang chờ xem xét mới có thể được phê duyệt.", ErrorCodeEnum.InvalidOperation);

        var normalizedReviewNote = NormalizeNote(command.ReviewNote);
        map.MapStatus = MapStatusEnum.Approved;
        map.ReviewNote = normalizedReviewNote;
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
                    reviewNote = normalizedReviewNote
                });

                var body = string.IsNullOrWhiteSpace(normalizedReviewNote)
                    ? $"Map \"{map.Title}\" đã được duyệt."
                    : $"Map \"{map.Title}\" đã được duyệt. Ghi chú: {normalizedReviewNote}";

                await _notificationPersistenceService.CreateNotificationAsync(
                    NotificationTypeEnum.SystemAnnouncement,
                    "Map của bạn đã được duyệt",
                    body,
                    new List<Guid> { map.CreatedBy.Value },
                    userIdNullable.Value,
                    payloadJson,
                    "/app/my-maps",
                    cancellationToken);
            }
            catch
            {
                // Notification failure must not break approval flow.
            }
        }

        return Result.Success("Bản đồ đã được phê duyệt thành công.");
    }

    private static string? NormalizeNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return null;
        return note.Trim();
    }
}

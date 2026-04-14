using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Maps.Commands.BatchApproveMaps;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using System.Text.Json;

namespace CapstoneProject.Application.Features.Maps.Commands.BatchRejectMaps;

public class BatchRejectMapsCommandHandler : IRequestHandler<BatchRejectMapsCommand, Result<BatchMapResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationPersistenceService _notificationPersistenceService;

    public BatchRejectMapsCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        INotificationPersistenceService notificationPersistenceService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationPersistenceService = notificationPersistenceService;
    }

    public async Task<Result<BatchMapResultDto>> Handle(BatchRejectMapsCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<BatchMapResultDto>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để thực hiện từ chối hàng loạt.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<BatchMapResultDto>.Failure("Bạn không có quyền từ chối bản đồ. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện từ chối hàng loạt.", ErrorCodeEnum.Forbidden);

        var repo = _unitOfWork.Repository<Map>();
        var maps = await repo.GetQueryable()
            .Where(m => command.MapIds.Contains(m.Id) && !m.IsDeleted)
            .ToListAsync(cancellationToken);
        var foundIds = maps.Select(m => m.Id).ToHashSet();
        var notFoundIds = command.MapIds.Where(id => !foundIds.Contains(id)).ToList();
        var toReject = maps.Where(m => m.MapStatus == MapStatusEnum.PendingReview).ToList();
        var invalidStatusIds = maps.Where(m => m.MapStatus != MapStatusEnum.PendingReview).Select(m => m.Id).ToList();
        var normalizedRejectReason = NormalizeNote(command.RejectReason);

        foreach (var map in toReject)
        {
            map.MapStatus = MapStatusEnum.Rejected;
            map.ReviewNote = normalizedRejectReason;
            map.UpdateEntity(userIdNullable!.Value);
            repo.Update(map);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var map in toReject.Where(m => m.CreatedBy.HasValue))
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
                    new List<Guid> { map.CreatedBy!.Value },
                    userIdNullable.Value,
                    payloadJson,
                    "/app/my-maps",
                    cancellationToken);
            }
            catch
            {
                // Notification failure must not break batch reject flow.
            }
        }

        var dto = new BatchMapResultDto
        {
            SuccessCount = toReject.Count,
            FailedCount = notFoundIds.Count + invalidStatusIds.Count,
            NotFoundIds = notFoundIds,
            InvalidStatusIds = invalidStatusIds
        };
        return Result<BatchMapResultDto>.Success(dto, $"Đã từ chối (các) bản đồ {dto.SuccessCount}.");
    }

    private static string? NormalizeNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return null;
        return note.Trim();
    }
}

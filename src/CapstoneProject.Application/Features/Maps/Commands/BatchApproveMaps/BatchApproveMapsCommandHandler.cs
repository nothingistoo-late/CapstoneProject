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

namespace CapstoneProject.Application.Features.Maps.Commands.BatchApproveMaps;

public class BatchApproveMapsCommandHandler : IRequestHandler<BatchApproveMapsCommand, Result<BatchMapResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationPersistenceService _notificationPersistenceService;

    public BatchApproveMapsCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        INotificationPersistenceService notificationPersistenceService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationPersistenceService = notificationPersistenceService;
    }

    public async Task<Result<BatchMapResultDto>> Handle(BatchApproveMapsCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<BatchMapResultDto>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để thực hiện phê duyệt hàng loạt.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<BatchMapResultDto>.Failure("Bạn không có quyền phê duyệt bản đồ. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện phê duyệt hàng loạt.", ErrorCodeEnum.Forbidden);

        var repo = _unitOfWork.Repository<Map>();
        var maps = await repo.GetQueryable()
            .Where(m => command.MapIds.Contains(m.Id) && !m.IsDeleted)
            .ToListAsync(cancellationToken);
        var foundIds = maps.Select(m => m.Id).ToHashSet();
        var notFoundIds = command.MapIds.Where(id => !foundIds.Contains(id)).ToList();
        var toApprove = maps.Where(m => m.MapStatus == MapStatusEnum.PendingReview).ToList();
        var invalidStatusIds = maps.Where(m => m.MapStatus != MapStatusEnum.PendingReview).Select(m => m.Id).ToList();
        var normalizedReviewNote = NormalizeNote(command.ReviewNote);

        foreach (var map in toApprove)
        {
            map.MapStatus = MapStatusEnum.Approved;
            map.ReviewNote = normalizedReviewNote;
            map.UpdateEntity(userIdNullable!.Value);
            repo.Update(map);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var map in toApprove.Where(m => m.CreatedBy.HasValue))
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
                    new List<Guid> { map.CreatedBy!.Value },
                    userIdNullable.Value,
                    payloadJson,
                    "/app/my-maps",
                    cancellationToken);
            }
            catch
            {
                // Notification failure must not break batch approval flow.
            }
        }

        var dto = new BatchMapResultDto
        {
            SuccessCount = toApprove.Count,
            FailedCount = notFoundIds.Count + invalidStatusIds.Count,
            NotFoundIds = notFoundIds,
            InvalidStatusIds = invalidStatusIds
        };
        return Result<BatchMapResultDto>.Success(dto, $"(Các) bản đồ {dto.SuccessCount} đã được phê duyệt.");
    }

    private static string? NormalizeNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return null;
        return note.Trim();
    }
}

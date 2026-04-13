using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CapstoneProject.Application.Features.Notifications.Commands.CreateSystemAnnouncement;

public class CreateSystemAnnouncementCommandHandler : IRequestHandler<CreateSystemAnnouncementCommand, Result<CreateSystemAnnouncementResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationPersistenceService _notificationPersistenceService;

    public CreateSystemAnnouncementCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        INotificationPersistenceService notificationPersistenceService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationPersistenceService = notificationPersistenceService;
    }

    public async Task<Result<CreateSystemAnnouncementResponse>> Handle(CreateSystemAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var title = request.Title?.Trim();
        var body = request.Body?.Trim();

        if (string.IsNullOrWhiteSpace(title))
            return Result<CreateSystemAnnouncementResponse>.Failure("Tiêu đề thông báo không được để trống", ErrorCodeEnum.ValidationFailed);

        if (string.IsNullOrWhiteSpace(body))
            return Result<CreateSystemAnnouncementResponse>.Failure("Nội dung thông báo không được để trống", ErrorCodeEnum.ValidationFailed);

        var recipientUserIds = await _unitOfWork.Repository<AppUser>()
            .GetQueryable()
            .Where(x => x.Status == EntityStatusEnum.Active)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (recipientUserIds.Count == 0)
            return Result<CreateSystemAnnouncementResponse>.Failure("Không có người dùng hoạt động để nhận thông báo", ErrorCodeEnum.NotFound);

        Guid? actorUserId = null;
        if (Guid.TryParse(_currentUserService.UserId, out var parsedActorUserId))
            actorUserId = parsedActorUserId;

        var payload = request.PayloadJson;
        if (string.IsNullOrWhiteSpace(payload))
        {
            payload = JsonSerializer.Serialize(new
            {
                scope = "all_active_users",
                sentAt = VietnamDateTime.DbNow
            });
        }

        var notificationId = await _notificationPersistenceService.CreateNotificationAsync(
            NotificationTypeEnum.SystemAnnouncement,
            title!,
            body!,
            recipientUserIds,
            actorUserId,
            payload,
            request.ActionUrl,
            cancellationToken);

        var response = new CreateSystemAnnouncementResponse
        {
            NotificationId = notificationId,
            RecipientCount = recipientUserIds.Count
        };

        return Result<CreateSystemAnnouncementResponse>.Success(response, "Đã gửi thông báo hệ thống cho toàn bộ người dùng");
    }
}

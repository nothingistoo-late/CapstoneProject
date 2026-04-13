using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Notifications.Commands.MarkAllAsRead;

public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationBroadcastService _notificationBroadcastService;

    public MarkAllAsReadCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        INotificationBroadcastService notificationBroadcastService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationBroadcastService = notificationBroadcastService;
    }

    public async Task<Result> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (!Guid.TryParse(userIdStr, out var userId))
            return Result.Failure("Người dùng chưa được xác thực", ErrorCodeEnum.Unauthorized);

        var unreadNotifications = await _unitOfWork.Repository<UserNotification>()
            .GetQueryable()
            .Where(x => x.UserId == userId && !x.IsDeleted && !x.IsRead)
            .ToListAsync(cancellationToken);

        if (unreadNotifications.Any())
        {
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = VietnamDateTime.DbNow;
            }

            _unitOfWork.Repository<UserNotification>().UpdateRange(unreadNotifications);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var unreadCount = await _unitOfWork.Repository<UserNotification>()
                .GetQueryable()
                .Where(x => x.UserId == userId && !x.IsDeleted && !x.IsRead)
                .CountAsync(cancellationToken);

            await _notificationBroadcastService.BroadcastAllNotificationsReadAsync(
                userId,
                unreadCount,
                cancellationToken);
        }

        return Result.Success("Đã đánh dấu tất cả thông báo là đã đọc");
    }
}


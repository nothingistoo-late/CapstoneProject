using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Notifications.Commands.MarkAsRead;

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationBroadcastService _notificationBroadcastService;

    public MarkAsReadCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        INotificationBroadcastService notificationBroadcastService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationBroadcastService = notificationBroadcastService;
    }

    public async Task<Result> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (!Guid.TryParse(userIdStr, out var userId))
            return Result.Failure("Người dùng chưa được xác thực", ErrorCodeEnum.Unauthorized);

        var userNotification = await _unitOfWork.Repository<UserNotification>()
            .GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.NotificationId && x.UserId == userId && !x.IsDeleted, cancellationToken);

        if (userNotification == null)
            return Result.Failure("Không tìm thấy thông báo", ErrorCodeEnum.NotFound);

        if (!userNotification.IsRead)
        {
            userNotification.IsRead = true;
            userNotification.ReadAt = VietnamDateTime.DbNow;
            _unitOfWork.Repository<UserNotification>().Update(userNotification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var unreadCount = await _unitOfWork.Repository<UserNotification>()
                .GetQueryable()
                .Where(x => x.UserId == userId && !x.IsDeleted && !x.IsRead)
                .CountAsync(cancellationToken);

            await _notificationBroadcastService.BroadcastNotificationReadAsync(
                userId,
                userNotification.Id,
                userNotification.ReadAt,
                unreadCount,
                cancellationToken);
        }

        return Result.Success("Thông báo đã được đánh dấu là đã đọc");
    }
}


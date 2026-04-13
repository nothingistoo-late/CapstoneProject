using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Notifications.Queries.GetNotifications;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Notifications.Queries.GetUnreadNotifications;

public class GetUnreadNotificationsQueryHandler : IRequestHandler<GetUnreadNotificationsQuery, Result<List<NotificationItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetUnreadNotificationsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<NotificationItemDto>>> Handle(GetUnreadNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (!Guid.TryParse(userIdStr, out var userId))
            return Result<List<NotificationItemDto>>.Failure("Người dùng chưa được xác thực", ErrorCodeEnum.Unauthorized);

        var notifications = await _unitOfWork.Repository<UserNotification>().GetQueryable()
            .Where(x => x.UserId == userId && !x.IsDeleted && !x.IsRead)
            .Include(x => x.Notification)
            .ThenInclude(x => x.ActorUser)
            .OrderByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var items = notifications.Select(x => new NotificationItemDto
        {
            Id = x.Id,
            NotificationId = x.NotificationId,
            Type = x.Notification.NotificationType.ToString(),
            Title = x.Notification.Title,
            Body = x.Notification.Body,
            IsRead = x.IsRead,
            ReadAt = x.ReadAt,
            CreatedAt = x.CreatedAt ?? VietnamDateTime.DbNow,
            ActionUrl = x.Notification.ActionUrl,
            Actor = x.Notification.ActorUser != null
                ? new SimpleActorDto
                {
                    Id = x.Notification.ActorUser.Id,
                    FullName = $"{x.Notification.ActorUser.FirstName} {x.Notification.ActorUser.LastName}".Trim(),
                    AvatarUrl = x.Notification.ActorUser.AvatarPath,
                }
                : null
        }).ToList();

        return Result<List<NotificationItemDto>>.Success(items, "Đã lấy danh sách thông báo chưa đọc thành công");
    }
}


using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<PaginationResult<NotificationItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetNotificationsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<NotificationItemDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (!Guid.TryParse(userIdStr, out var userId))
            return Result<PaginationResult<NotificationItemDto>>.Failure("Người dùng chưa được xác thực", ErrorCodeEnum.Unauthorized);

        var query = _unitOfWork.Repository<UserNotification>().GetQueryable()
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .Include(x => x.Notification)
            .ThenInclude(x => x.ActorUser)
            .OrderByDescending(x => x.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var notifications = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
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

        var paginated = PaginationResult<NotificationItemDto>.Success(items, pageNumber, pageSize, total);
        return Result<PaginationResult<NotificationItemDto>>.Success(paginated, "Đã lấy danh sách thông báo thành công");
    }
}

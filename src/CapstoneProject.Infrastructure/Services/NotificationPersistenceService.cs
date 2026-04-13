using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using CapstoneProject.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CapstoneProject.Infrastructure.Services;

public class NotificationPersistenceService : INotificationPersistenceService
{
    private readonly CapstoneProjectDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationBroadcastService _notificationBroadcastService;
    private readonly ILogger<NotificationPersistenceService> _logger;

    public NotificationPersistenceService(
        CapstoneProjectDbContext dbContext,
        IUnitOfWork unitOfWork,
        INotificationBroadcastService notificationBroadcastService,
        ILogger<NotificationPersistenceService> logger)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _notificationBroadcastService = notificationBroadcastService;
        _logger = logger;
    }

    public async Task<Guid> CreateNotificationAsync(
        NotificationTypeEnum type,
        string title,
        string body,
        List<Guid> recipientUserIds,
        Guid? actorUserId = null,
        string? payloadJson = null,
        string? actionUrl = null,
        CancellationToken cancellationToken = default
    )
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            NotificationType = type,
            Title = title,
            Body = body,
            PayloadJson = payloadJson,
            ActorUserId = actorUserId,
            ActionUrl = actionUrl,
            CreatedAt = VietnamDateTime.DbNow,
            CreatedBy = actorUserId,
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var uniqueRecipientIds = recipientUserIds.Distinct().ToList();

        // Create UserNotification records for each recipient
        var userNotifications = uniqueRecipientIds.Select(userId => new UserNotification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            NotificationId = notification.Id,
            IsRead = false,
            CreatedAt = VietnamDateTime.DbNow,
        }).ToList();

        _dbContext.UserNotifications.AddRange(userNotifications);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await BroadcastCreatedNotificationsAsync(notification, userNotifications, cancellationToken);

        return notification.Id;
    }

    private async Task BroadcastCreatedNotificationsAsync(
        Notification notification,
        IReadOnlyCollection<UserNotification> userNotifications,
        CancellationToken cancellationToken)
    {
        if (userNotifications.Count == 0)
            return;

        SimpleActorBroadcastDto? actorDto = null;
        if (notification.ActorUserId.HasValue)
        {
            var actor = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == notification.ActorUserId.Value, cancellationToken);

            if (actor != null)
            {
                actorDto = new SimpleActorBroadcastDto
                {
                    Id = actor.Id,
                    FullName = $"{actor.FirstName} {actor.LastName}".Trim(),
                    AvatarUrl = actor.AvatarPath,
                };
            }
        }

        foreach (var userNotification in userNotifications)
        {
            try
            {
                var unreadCount = await _dbContext.UserNotifications
                    .AsNoTracking()
                    .CountAsync(x => x.UserId == userNotification.UserId && !x.IsDeleted && !x.IsRead, cancellationToken);

                var dto = new NotificationBroadcastDto
                {
                    UserNotificationId = userNotification.Id,
                    NotificationId = notification.Id,
                    Type = notification.NotificationType.ToString(),
                    Title = notification.Title,
                    Body = notification.Body,
                    IsRead = false,
                    ReadAt = null,
                    CreatedAt = userNotification.CreatedAt ?? VietnamDateTime.DbNow,
                    ActionUrl = notification.ActionUrl,
                    PayloadJson = notification.PayloadJson,
                    Actor = actorDto
                };

                await _notificationBroadcastService.BroadcastNotificationCreatedAsync(
                    userNotification.UserId,
                    dto,
                    unreadCount,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting created notification {NotificationId} to user {UserId}", notification.Id, userNotification.UserId);
            }
        }
    }

    public async Task MarkAsReadAsync(Guid userNotificationId, CancellationToken cancellationToken = default)
    {
        var userNotification = await _dbContext.UserNotifications
            .FirstOrDefaultAsync(x => x.Id == userNotificationId && !x.IsDeleted, cancellationToken);

        if (userNotification != null && !userNotification.IsRead)
        {
            userNotification.IsRead = true;
            userNotification.ReadAt = VietnamDateTime.DbNow;
            _dbContext.UserNotifications.Update(userNotification);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _dbContext.UserNotifications
            .Where(x => x.UserId == userId && !x.IsDeleted && !x.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = VietnamDateTime.DbNow;
        }

        if (unreadNotifications.Any())
        {
            _dbContext.UserNotifications.UpdateRange(unreadNotifications);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserNotifications
            .Where(x => x.UserId == userId && !x.IsDeleted && !x.IsRead)
            .CountAsync(cancellationToken);
    }

    public async Task<List<NotificationListDto>> GetNotificationsAsync(
        Guid userId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default
    )
    {
        var skip = (pageNumber - 1) * pageSize;

        var notifications = await _dbContext.UserNotifications
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .Include(x => x.Notification)
            .ThenInclude(x => x.ActorUser)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new NotificationListDto
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
                    ? new SimpleUserDto
                    {
                        Id = x.Notification.ActorUser.Id,
                        FullName = $"{x.Notification.ActorUser.FirstName} {x.Notification.ActorUser.LastName}",
                        AvatarUrl = x.Notification.ActorUser.AvatarPath,
                    }
                    : null
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return notifications;
    }

    public async Task<List<NotificationListDto>> GetUnreadNotificationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var notifications = await _dbContext.UserNotifications
            .Where(x => x.UserId == userId && !x.IsDeleted && !x.IsRead)
            .Include(x => x.Notification)
            .ThenInclude(x => x.ActorUser)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new NotificationListDto
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
                    ? new SimpleUserDto
                    {
                        Id = x.Notification.ActorUser.Id,
                        FullName = $"{x.Notification.ActorUser.FirstName} {x.Notification.ActorUser.LastName}",
                        AvatarUrl = x.Notification.ActorUser.AvatarPath,
                    }
                    : null
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return notifications;
    }
}
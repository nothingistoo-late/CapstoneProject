using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using MediatR;

namespace CapstoneProject.Application.Features.Notifications.Commands.CreateNotification;

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Result<CreateNotificationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateNotificationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateNotificationResponse>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        if (!request.RecipientUserIds.Any())
            return Result<CreateNotificationResponse>.Failure("Phải có ít nhất một người nhận", Application.Common.Enums.ErrorCodeEnum.ValidationFailed);

        // Remove duplicates
        var uniqueRecipients = request.RecipientUserIds.Distinct().ToList();

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            NotificationType = request.Type,
            Title = request.Title,
            Body = request.Body,
            PayloadJson = request.PayloadJson,
            ActorUserId = request.ActorUserId,
            ActionUrl = request.ActionUrl,
            CreatedAt = VietnamDateTime.DbNow,
            CreatedBy = request.ActorUserId,
        };

        await _unitOfWork.Repository<Notification>().AddAsync(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create UserNotification records for each recipient
        var userNotifications = uniqueRecipients.Select(userId => new UserNotification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            NotificationId = notification.Id,
            IsRead = false,
            CreatedAt = VietnamDateTime.DbNow,
        }).ToList();

        await _unitOfWork.Repository<UserNotification>().AddRangeAsync(userNotifications);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateNotificationResponse
        {
            NotificationId = notification.Id,
            RecipientCount = uniqueRecipients.Count
        };

        return Result<CreateNotificationResponse>.Success(response, "Đã tạo thông báo thành công");
    }
}


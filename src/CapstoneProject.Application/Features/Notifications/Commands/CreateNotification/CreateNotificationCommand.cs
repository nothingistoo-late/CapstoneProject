using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Notifications.Commands.CreateNotification;

public record CreateNotificationCommand(
    NotificationTypeEnum Type,
    string Title,
    string Body,
    List<Guid> RecipientUserIds,
    Guid? ActorUserId = null,
    string? PayloadJson = null,
    string? ActionUrl = null) : IRequest<Result<CreateNotificationResponse>>;

public class CreateNotificationResponse
{
    public Guid NotificationId { get; set; }
    public int RecipientCount { get; set; }
}

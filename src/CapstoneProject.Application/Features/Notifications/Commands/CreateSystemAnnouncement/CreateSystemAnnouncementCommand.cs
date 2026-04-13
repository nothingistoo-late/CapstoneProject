using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Notifications.Commands.CreateSystemAnnouncement;

public record CreateSystemAnnouncementCommand(
    string Title,
    string Body,
    string? ActionUrl = null,
    string? PayloadJson = null) : IRequest<Result<CreateSystemAnnouncementResponse>>;

public class CreateSystemAnnouncementResponse
{
    public Guid NotificationId { get; set; }
    public int RecipientCount { get; set; }
}

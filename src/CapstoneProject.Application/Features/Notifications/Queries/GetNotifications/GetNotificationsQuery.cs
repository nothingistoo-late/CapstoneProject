using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery(
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PaginationResult<NotificationItemDto>>>;

public class NotificationItemDto
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ActionUrl { get; set; }
    public string? PayloadJson { get; set; }
    public SimpleActorDto? Actor { get; set; }
}

public class SimpleActorDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

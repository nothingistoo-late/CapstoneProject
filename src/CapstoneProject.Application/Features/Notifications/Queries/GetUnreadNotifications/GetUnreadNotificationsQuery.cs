using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Notifications.Queries.GetNotifications;
using MediatR;

namespace CapstoneProject.Application.Features.Notifications.Queries.GetUnreadNotifications;

public record GetUnreadNotificationsQuery() : IRequest<Result<List<NotificationItemDto>>>;

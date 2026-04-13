using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Notifications.Commands.MarkAllAsRead;

public record MarkAllAsReadCommand() : IRequest<Result>;

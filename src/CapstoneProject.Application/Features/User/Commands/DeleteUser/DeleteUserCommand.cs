using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.User.Commands.DeleteUser;

public record DeleteUserCommand(Guid UserId) : IRequest<Result>;

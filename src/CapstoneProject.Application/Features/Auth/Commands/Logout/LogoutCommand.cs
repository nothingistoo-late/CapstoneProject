using MediatR;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Application.Features.Auth.Commands.Logout;

public record LogoutCommand : IRequest<Result>;
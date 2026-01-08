using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.User;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CapstoneProject.Application.Features.User.Commands.UpdateUser;

public record UpdateUserCommand(Guid UserId, UpdateUserRequest Request, IFormFile? AvatarFile) : IRequest<Result>;

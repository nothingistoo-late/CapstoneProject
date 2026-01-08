using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.User;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CapstoneProject.Application.Features.User.Commands.CreateUser;

public record CreateUserCommand(CreateUserRequest Request, IFormFile? AvatarFile) : IRequest<Result>;

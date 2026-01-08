using MediatR;
using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Application.Features.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(ChangePasswordRequest Request) : IRequest<Result>;
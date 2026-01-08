using MediatR;
using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest<Result>;
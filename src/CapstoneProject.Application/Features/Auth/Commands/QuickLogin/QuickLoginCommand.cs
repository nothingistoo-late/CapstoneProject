using MediatR;
using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Application.Features.Auth.Commands.QuickLogin;

public record QuickLoginCommand(QuickLoginRequest Request) : IRequest<Result<AuthResponse>>;

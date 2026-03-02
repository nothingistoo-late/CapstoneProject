using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Auth.Commands.GoogleLogin;

public record GoogleLoginCommand(GoogleLoginRequest Request) : IRequest<Result<AuthResponse>>;

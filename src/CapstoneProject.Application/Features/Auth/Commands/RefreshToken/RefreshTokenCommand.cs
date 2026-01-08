using MediatR;
using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<Result<AuthResponse>>;
using MediatR;
using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Application.Features.Auth.Commands.Register;

public record RegisterCommand(RegisterRequest Request) : IRequest<Result>;


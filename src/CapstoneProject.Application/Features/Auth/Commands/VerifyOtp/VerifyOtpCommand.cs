using MediatR;
using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Application.Features.Auth.Commands.VerifyOtp;

public record VerifyOtpCommand(VerifyOtpRequest Request) : IRequest<Result>;

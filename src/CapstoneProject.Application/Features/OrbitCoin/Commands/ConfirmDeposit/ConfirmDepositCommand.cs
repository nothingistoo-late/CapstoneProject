using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.OrbitCoin.Commands.ConfirmDeposit;

/// <summary>
/// Confirm deposit after user is redirected from PayOS success page. Backend checks PayOS payment status and credits OrbitCoin if paid.
/// </summary>
public record ConfirmDepositCommand(Guid OrderId) : IRequest<Result>;

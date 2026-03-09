using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.OrbitCoin;
using MediatR;

namespace CapstoneProject.Application.Features.OrbitCoin.Commands.CreateDepositOrder;

public record CreateDepositOrderCommand(decimal AmountOrbitCoin) : IRequest<Result<CreateDepositOrderResult>>;

using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.OrbitCoin.Commands.CreateDepositOrder;

public record CreateDepositOrderCommand(decimal AmountOrbitCoin) : IRequest<Result<CreateDepositOrderResult>>;

public class CreateDepositOrderResult
{
    public Guid OrderId { get; set; }
    public string CheckoutUrl { get; set; } = string.Empty;
}

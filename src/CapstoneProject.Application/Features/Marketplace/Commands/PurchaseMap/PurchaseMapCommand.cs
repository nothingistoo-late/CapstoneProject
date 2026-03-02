using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Marketplace.Commands.PurchaseMap;

public record PurchaseMapCommand(Guid MapId, Guid? PaymentMethodId = null) : IRequest<Result<Guid>>;

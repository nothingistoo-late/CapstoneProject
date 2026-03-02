using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Marketplace.Commands.PurchasePackage;

public record PurchasePackageCommand(Guid PackageId, Guid? PaymentMethodId = null) : IRequest<Result<Guid>>;

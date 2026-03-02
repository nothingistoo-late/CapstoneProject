using FluentValidation;

namespace CapstoneProject.Application.Features.Marketplace.Commands.PurchasePackage;

public class PurchasePackageCommandValidator : AbstractValidator<PurchasePackageCommand>
{
    public PurchasePackageCommandValidator()
    {
        RuleFor(x => x.PackageId)
            .NotEmpty().WithMessage("Package Id is required.");
    }
}

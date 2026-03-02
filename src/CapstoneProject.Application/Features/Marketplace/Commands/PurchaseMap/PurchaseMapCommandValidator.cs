using FluentValidation;

namespace CapstoneProject.Application.Features.Marketplace.Commands.PurchaseMap;

public class PurchaseMapCommandValidator : AbstractValidator<PurchaseMapCommand>
{
    public PurchaseMapCommandValidator()
    {
        RuleFor(x => x.MapId)
            .NotEmpty().WithMessage("Map Id is required.");
    }
}

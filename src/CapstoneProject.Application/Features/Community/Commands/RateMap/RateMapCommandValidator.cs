using FluentValidation;

namespace CapstoneProject.Application.Features.Community.Commands.RateMap;

public class RateMapCommandValidator : AbstractValidator<RateMapCommand>
{
    public RateMapCommandValidator()
    {
        RuleFor(x => x.MapId)
            .NotEmpty().WithMessage("Map Id is required.");
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");
    }
}

using FluentValidation;

namespace CapstoneProject.Application.Features.Community.Commands.RateMap;

public class RateMapCommandValidator : AbstractValidator<RateMapCommand>
{
    public RateMapCommandValidator()
    {
        RuleFor(x => x.MapId)
            .NotEmpty().WithMessage("Id bản đồ là bắt buộc.");
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");
    }
}

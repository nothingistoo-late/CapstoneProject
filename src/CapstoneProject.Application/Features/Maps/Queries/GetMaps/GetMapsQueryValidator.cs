using FluentValidation;

namespace CapstoneProject.Application.Features.Maps.Queries.GetMaps;

public class GetMapsQueryValidator : AbstractValidator<GetMapsQuery>
{
    public GetMapsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.");

        When(x => x.Difficulty.HasValue, () =>
        {
            RuleFor(x => x.Difficulty!.Value)
                .InclusiveBetween(1, 5)
                .WithMessage("Difficulty filter must be between 1 and 5.");
        });
    }
}

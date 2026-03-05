using FluentValidation;

namespace CapstoneProject.Application.Features.Maps.Commands.UpdateMap;

public class UpdateMapCommandValidator : AbstractValidator<UpdateMapCommand>
{
    public UpdateMapCommandValidator()
    {
        RuleFor(x => x.MapId).NotEmpty().WithMessage("Map Id is required.");
        RuleFor(x => x.Request).NotNull().WithMessage("Request is required.");
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request!.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Request!.Difficulty).InclusiveBetween(1, 5);
            RuleFor(x => x.Request!.TimeLimitMs).GreaterThan(0);
            When(x => x.Request!.UnlockEditorialAfterStars.HasValue, () =>
                RuleFor(x => x.Request!.UnlockEditorialAfterStars!.Value).InclusiveBetween(0, 3));
        });
    }
}

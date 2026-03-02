using FluentValidation;

namespace CapstoneProject.Application.Features.Challenge.Commands.UpdateMap;

public class UpdateMapCommandValidator : AbstractValidator<UpdateMapCommand>
{
    public UpdateMapCommandValidator()
    {
        RuleFor(x => x.MapId)
            .NotEmpty().WithMessage("Map Id is required.");
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request is required.");
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request!.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");
            RuleFor(x => x.Request!.Difficulty)
                .InclusiveBetween(1, 5).WithMessage("Difficulty must be between 1 and 5.");
            RuleFor(x => x.Request!.TimeLimitMs)
                .GreaterThan(0).WithMessage("TimeLimitMs must be positive.");
            When(x => x.Request!.UnlockEditorialAfterStars.HasValue, () =>
                RuleFor(x => x.Request!.UnlockEditorialAfterStars!.Value)
                    .InclusiveBetween(0, 3).WithMessage("UnlockEditorialAfterStars must be between 0 and 3."));
        });
    }
}

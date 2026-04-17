using FluentValidation;

namespace CapstoneProject.Application.Features.Games.Commands.BatchApproveMaps;

public class BatchApproveMapsCommandValidator : AbstractValidator<BatchApproveMapsCommand>
{
    public BatchApproveMapsCommandValidator()
    {
        RuleFor(x => x.GameIds)
            .NotNull().WithMessage("Game Ids are required.")
            .NotEmpty().WithMessage("At least one Game Id is required.");

        RuleFor(x => x.ReviewNote)
            .MaximumLength(1000)
            .WithMessage("Review note must not exceed 1000 characters.");
    }
}

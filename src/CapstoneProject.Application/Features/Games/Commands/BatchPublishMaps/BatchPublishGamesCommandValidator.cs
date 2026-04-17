using FluentValidation;

namespace CapstoneProject.Application.Features.Games.Commands.BatchPublishMaps;

public class BatchPublishMapsCommandValidator : AbstractValidator<BatchPublishMapsCommand>
{
    public BatchPublishMapsCommandValidator()
    {
        RuleFor(x => x.GameIds)
            .NotNull().WithMessage("Game Ids are required.")
            .NotEmpty().WithMessage("At least one Game Id is required.");
    }
}

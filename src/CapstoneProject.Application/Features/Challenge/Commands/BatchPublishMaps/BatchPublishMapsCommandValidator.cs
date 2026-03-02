using FluentValidation;

namespace CapstoneProject.Application.Features.Challenge.Commands.BatchPublishMaps;

public class BatchPublishMapsCommandValidator : AbstractValidator<BatchPublishMapsCommand>
{
    public BatchPublishMapsCommandValidator()
    {
        RuleFor(x => x.MapIds)
            .NotNull().WithMessage("Map Ids are required.")
            .NotEmpty().WithMessage("At least one Map Id is required.");
    }
}

using FluentValidation;

namespace CapstoneProject.Application.Features.Challenge.Commands.BatchRejectMaps;

public class BatchRejectMapsCommandValidator : AbstractValidator<BatchRejectMapsCommand>
{
    public BatchRejectMapsCommandValidator()
    {
        RuleFor(x => x.MapIds)
            .NotNull().WithMessage("Map Ids are required.")
            .NotEmpty().WithMessage("At least one Map Id is required.");
    }
}

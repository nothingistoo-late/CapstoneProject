using FluentValidation;

namespace CapstoneProject.Application.Features.Maps.Commands.BatchApproveMaps;

public class BatchApproveMapsCommandValidator : AbstractValidator<BatchApproveMapsCommand>
{
    public BatchApproveMapsCommandValidator()
    {
        RuleFor(x => x.MapIds)
            .NotNull().WithMessage("Map Ids are required.")
            .NotEmpty().WithMessage("At least one Map Id is required.");

        RuleFor(x => x.ReviewNote)
            .MaximumLength(1000)
            .WithMessage("Review note must not exceed 1000 characters.");
    }
}

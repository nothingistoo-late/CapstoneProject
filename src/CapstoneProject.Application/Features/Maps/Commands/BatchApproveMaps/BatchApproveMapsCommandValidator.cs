using FluentValidation;

namespace CapstoneProject.Application.Features.Maps.Commands.BatchApproveMaps;

public class BatchApproveMapsCommandValidator : AbstractValidator<BatchApproveMapsCommand>
{
    public BatchApproveMapsCommandValidator()
    {
        RuleFor(x => x.MapIds)
            .NotNull().WithMessage("Map Ids are required.")
            .NotEmpty().WithMessage("At least one Map Id is required.");
    }
}

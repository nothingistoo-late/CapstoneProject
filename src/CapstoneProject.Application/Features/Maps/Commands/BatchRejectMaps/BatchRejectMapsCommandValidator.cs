using FluentValidation;

namespace CapstoneProject.Application.Features.Maps.Commands.BatchRejectMaps;

public class BatchRejectMapsCommandValidator : AbstractValidator<BatchRejectMapsCommand>
{
    public BatchRejectMapsCommandValidator()
    {
        RuleFor(x => x.MapIds)
            .NotNull().WithMessage("Map Ids are required.")
            .NotEmpty().WithMessage("At least one Map Id is required.");
    }
}

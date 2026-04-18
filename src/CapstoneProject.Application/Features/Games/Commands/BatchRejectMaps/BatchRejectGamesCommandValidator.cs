using FluentValidation;

namespace CapstoneProject.Application.Features.Games.Commands.BatchRejectMaps;

public class BatchRejectMapsCommandValidator : AbstractValidator<BatchRejectMapsCommand>
{
    public BatchRejectMapsCommandValidator()
    {
        RuleFor(x => x.GameIds)
            .NotNull().WithMessage("Game Ids are required.")
            .NotEmpty().WithMessage("At least one Game Id is required.");

        RuleFor(x => x.RejectReason)
            .MaximumLength(1000)
            .WithMessage("Reject reason must not exceed 1000 characters.");
    }
}

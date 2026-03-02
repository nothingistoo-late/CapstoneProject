using FluentValidation;

namespace CapstoneProject.Application.Features.Challenge.Commands.ApproveMap;

public class ApproveMapCommandValidator : AbstractValidator<ApproveMapCommand>
{
    public ApproveMapCommandValidator()
    {
        RuleFor(x => x.MapId)
            .NotEmpty().WithMessage("Map Id is required.");
    }
}

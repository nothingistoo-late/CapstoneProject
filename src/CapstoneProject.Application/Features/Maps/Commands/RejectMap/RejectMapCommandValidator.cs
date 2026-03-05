using FluentValidation;

namespace CapstoneProject.Application.Features.Maps.Commands.RejectMap;

public class RejectMapCommandValidator : AbstractValidator<RejectMapCommand>
{
    public RejectMapCommandValidator()
    {
        RuleFor(x => x.MapId)
            .NotEmpty().WithMessage("Map Id is required.");
    }
}

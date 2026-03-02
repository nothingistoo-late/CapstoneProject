using FluentValidation;

namespace CapstoneProject.Application.Features.Challenge.Commands.DeleteMap;

public class DeleteMapCommandValidator : AbstractValidator<DeleteMapCommand>
{
    public DeleteMapCommandValidator()
    {
        RuleFor(x => x.MapId)
            .NotEmpty().WithMessage("Map Id is required.");
    }
}

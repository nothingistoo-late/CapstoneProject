using FluentValidation;

namespace CapstoneProject.Application.Features.Maps.Commands.DeleteMap;

public class DeleteMapCommandValidator : AbstractValidator<DeleteMapCommand>
{
    public DeleteMapCommandValidator()
    {
        RuleFor(x => x.MapId)
            .NotEmpty().WithMessage("Map Id is required.");
    }
}

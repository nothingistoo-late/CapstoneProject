using FluentValidation;

namespace CapstoneProject.Application.Features.Maps.Commands.PublishMap;

public class PublishMapCommandValidator : AbstractValidator<PublishMapCommand>
{
    public PublishMapCommandValidator()
    {
        RuleFor(x => x.MapId)
            .NotEmpty().WithMessage("Map Id is required.");
    }
}

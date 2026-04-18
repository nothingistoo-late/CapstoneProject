using FluentValidation;

namespace CapstoneProject.Application.Features.Games.Commands.PublishMap;

public class PublishMapCommandValidator : AbstractValidator<PublishMapCommand>
{
    public PublishMapCommandValidator()
    {
        RuleFor(x => x.GameId)
            .NotEmpty().WithMessage("Id bản đồ là bắt buộc.");
    }
}

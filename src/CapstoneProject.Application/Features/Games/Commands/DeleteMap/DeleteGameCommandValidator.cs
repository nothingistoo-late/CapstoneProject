using FluentValidation;

namespace CapstoneProject.Application.Features.Games.Commands.DeleteMap;

public class DeleteMapCommandValidator : AbstractValidator<DeleteMapCommand>
{
    public DeleteMapCommandValidator()
    {
        RuleFor(x => x.GameId)
            .NotEmpty().WithMessage("Id trò chơi là bắt buộc.");
    }
}

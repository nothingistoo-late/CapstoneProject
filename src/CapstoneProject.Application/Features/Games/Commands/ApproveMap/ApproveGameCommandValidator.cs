using FluentValidation;

namespace CapstoneProject.Application.Features.Games.Commands.ApproveMap;

public class ApproveMapCommandValidator : AbstractValidator<ApproveMapCommand>
{
    public ApproveMapCommandValidator()
    {
        RuleFor(x => x.GameId)
            .NotEmpty().WithMessage("Id trò chơi là bắt buộc.");

        RuleFor(x => x.ReviewNote)
            .MaximumLength(1000)
            .WithMessage("Ghi chú duyệt không được vượt quá 1000 ký tự.");
    }
}

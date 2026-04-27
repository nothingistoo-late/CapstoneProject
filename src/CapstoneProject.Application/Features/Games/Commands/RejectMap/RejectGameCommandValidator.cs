using FluentValidation;

namespace CapstoneProject.Application.Features.Games.Commands.RejectMap;

public class RejectMapCommandValidator : AbstractValidator<RejectMapCommand>
{
    public RejectMapCommandValidator()
    {
        RuleFor(x => x.GameId)
            .NotEmpty().WithMessage("Id trò chơi là bắt buộc.");

        RuleFor(x => x.RejectReason)
            .MaximumLength(1000)
            .WithMessage("Lý do từ chối không được vượt quá 1000 ký tự.");
    }
}

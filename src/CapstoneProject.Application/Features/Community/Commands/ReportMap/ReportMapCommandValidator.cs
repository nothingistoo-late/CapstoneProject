using FluentValidation;

namespace CapstoneProject.Application.Features.Community.Commands.ReportMap;

public class ReportMapCommandValidator : AbstractValidator<ReportMapCommand>
{
    public ReportMapCommandValidator()
    {
        RuleFor(x => x.GameId)
            .NotEmpty().WithMessage("Id trò chơi là bắt buộc.");
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Report reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}

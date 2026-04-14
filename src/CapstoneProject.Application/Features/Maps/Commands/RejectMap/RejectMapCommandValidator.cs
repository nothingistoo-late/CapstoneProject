using FluentValidation;

namespace CapstoneProject.Application.Features.Maps.Commands.RejectMap;

public class RejectMapCommandValidator : AbstractValidator<RejectMapCommand>
{
    public RejectMapCommandValidator()
    {
        RuleFor(x => x.MapId)
            .NotEmpty().WithMessage("Id bản đồ là bắt buộc.");

        RuleFor(x => x.RejectReason)
            .MaximumLength(1000)
            .WithMessage("Lý do từ chối không được vượt quá 1000 ký tự.");
    }
}

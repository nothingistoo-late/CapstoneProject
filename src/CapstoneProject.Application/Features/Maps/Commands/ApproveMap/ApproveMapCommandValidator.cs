using FluentValidation;

namespace CapstoneProject.Application.Features.Maps.Commands.ApproveMap;

public class ApproveMapCommandValidator : AbstractValidator<ApproveMapCommand>
{
    public ApproveMapCommandValidator()
    {
        RuleFor(x => x.MapId)
            .NotEmpty().WithMessage("Id bản đồ là bắt buộc.");

        RuleFor(x => x.ReviewNote)
            .MaximumLength(1000)
            .WithMessage("Ghi chú duyệt không được vượt quá 1000 ký tự.");
    }
}

using FluentValidation;

namespace CapstoneProject.Application.Features.Maps.Commands.ApproveMap;

public class ApproveMapCommandValidator : AbstractValidator<ApproveMapCommand>
{
    public ApproveMapCommandValidator()
    {
        RuleFor(x => x.MapId)
            .NotEmpty().WithMessage("Id bản đồ là bắt buộc.");
    }
}

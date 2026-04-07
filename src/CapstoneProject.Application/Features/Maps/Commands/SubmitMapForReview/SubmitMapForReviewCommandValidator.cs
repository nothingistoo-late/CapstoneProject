using FluentValidation;

namespace CapstoneProject.Application.Features.Maps.Commands.SubmitMapForReview;

public class SubmitMapForReviewCommandValidator : AbstractValidator<SubmitMapForReviewCommand>
{
    public SubmitMapForReviewCommandValidator()
    {
        RuleFor(x => x.MapId)
            .NotEmpty().WithMessage("Id bản đồ là bắt buộc.");
    }
}

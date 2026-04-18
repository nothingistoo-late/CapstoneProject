using FluentValidation;

namespace CapstoneProject.Application.Features.Games.Commands.SubmitMapForReview;

public class SubmitMapForReviewCommandValidator : AbstractValidator<SubmitMapForReviewCommand>
{
    public SubmitMapForReviewCommandValidator()
    {
        RuleFor(x => x.GameId)
            .NotEmpty().WithMessage("Id bản đồ là bắt buộc.");
    }
}

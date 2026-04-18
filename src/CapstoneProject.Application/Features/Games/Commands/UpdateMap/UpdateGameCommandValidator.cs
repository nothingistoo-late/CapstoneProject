using CapstoneProject.Application.Commons.Helpers;
using FluentValidation;

namespace CapstoneProject.Application.Features.Games.Commands.UpdateMap;

public class UpdateMapCommandValidator : AbstractValidator<UpdateMapCommand>
{
    public UpdateMapCommandValidator()
    {
        RuleFor(x => x.GameId).NotEmpty().WithMessage("Id bản đồ là bắt buộc.");
        RuleFor(x => x.Request).NotNull().WithMessage("Request is required.");
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request!.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Request!.Difficulty).InclusiveBetween(1, 5);
            When(x => x.Request!.FreeTrialAttemptLimit.HasValue, () =>
                RuleFor(x => x.Request!.FreeTrialAttemptLimit!.Value).GreaterThanOrEqualTo(0));
            When(x => x.Request!.Levels is { Count: > 0 }, () =>
            {
                RuleFor(x => x.Request!)
                    .Must(req =>
                    {
                        MapLevelOrderNormalizer.NormalizeIfDuplicate(req.Levels!);
                        return true;
                    });
            });
            When(x => x.Request!.UnlockEditorialAfterStars.HasValue, () =>
                RuleFor(x => x.Request!.UnlockEditorialAfterStars!.Value).InclusiveBetween(0, 3));

            When(x => x.Request!.TagIds != null, () =>
            {
                RuleForEach(x => x.Request!.TagIds!)
                    .NotEmpty().WithMessage("TagIds must contain valid Guid values.");
                RuleFor(x => x.Request!.TagIds!)
                    .Must(ids => ids.Distinct().Count() == ids.Count)
                    .WithMessage("TagIds must not contain duplicates.");
            });

            When(x => x.Request!.LearnedTags != null, () =>
            {
                RuleForEach(x => x.Request!.LearnedTags!)
                    .NotEmpty().WithMessage("LearnedTags must contain valid Guid values.");
                RuleFor(x => x.Request!.LearnedTags!)
                    .Must(ids => ids.Distinct().Count() == ids.Count)
                    .WithMessage("LearnedTags must not contain duplicates.");
            });
        });
    }
}

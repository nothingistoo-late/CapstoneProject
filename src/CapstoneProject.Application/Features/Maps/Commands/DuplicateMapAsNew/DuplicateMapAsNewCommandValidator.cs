using FluentValidation;

namespace CapstoneProject.Application.Features.Maps.Commands.DuplicateMapAsNew;

public class DuplicateMapAsNewCommandValidator : AbstractValidator<DuplicateMapAsNewCommand>
{
    public DuplicateMapAsNewCommandValidator()
    {
        RuleFor(x => x.SourceMapId).NotEmpty();
        When(x => x.Request != null && !string.IsNullOrWhiteSpace(x.Request.Title), () =>
        {
            RuleFor(x => x.Request!.Title!)
                .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự.");
        });
        When(x => x.Request?.Difficulty != null, () =>
        {
            RuleFor(x => x.Request!.Difficulty!.Value).InclusiveBetween(1, 5);
        });
        When(x => x.Request?.UnlockEditorialAfterStars != null, () =>
        {
            RuleFor(x => x.Request!.UnlockEditorialAfterStars!.Value).InclusiveBetween(0, 3);
        });
        When(x => x.Request?.Price != null, () =>
        {
            RuleFor(x => x.Request!.Price!.Value).GreaterThanOrEqualTo(0);
        });
        When(x => x.Request?.TagIds != null, () =>
        {
            RuleForEach(x => x.Request!.TagIds!).NotEmpty();
            RuleFor(x => x.Request!.TagIds!).Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("TagIds must not contain duplicates.");
        });
        When(x => x.Request?.LearnedTags != null, () =>
        {
            RuleForEach(x => x.Request!.LearnedTags!).NotEmpty();
        });
    }
}

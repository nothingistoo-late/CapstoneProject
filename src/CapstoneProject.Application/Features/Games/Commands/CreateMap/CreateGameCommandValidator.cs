using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Commons.Helpers;
using FluentValidation;

namespace CapstoneProject.Application.Features.Games.Commands.CreateMap;

public class CreateMapCommandValidator : AbstractValidator<CreateMapCommand>
{
    public CreateMapCommandValidator()
    {
        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");
        RuleFor(x => x.Request.Difficulty)
            .InclusiveBetween(1, 5).WithMessage("Difficulty must be between 1 and 5");
        RuleFor(x => x.Request.FreeTrialAttemptLimit)
            .GreaterThanOrEqualTo(0).WithMessage("FreeTrialAttemptLimit must be greater than or equal to 0");
        RuleFor(x => x.Request)
            .Must(r => HasLevelsOrSingleJson(r))
            .WithMessage("Provide Levels (non-empty) or GameDetailJson for a single level.");
        RuleFor(x => x.Request)
            .Must(r =>
            {
                if (r.Levels is { Count: > 0 })
                    MapLevelOrderNormalizer.NormalizeIfDuplicate(r.Levels);
                return true;
            });
        RuleFor(x => x.Request)
            .Must(r => r.Levels == null || r.Levels.Count == 0 || r.Levels.Select(l => l.LevelOrder).Distinct().Count() == r.Levels.Count)
            .WithMessage("Levels must have unique LevelOrder values.");

        RuleForEach(x => x.Request.TagIds)
            .NotEmpty().WithMessage("TagIds must contain valid Guid values.");
        RuleFor(x => x.Request.TagIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("TagIds must not contain duplicates.");

        RuleForEach(x => x.Request.LearnedTags)
            .NotEmpty().WithMessage("LearnedTags must contain valid Guid values.");
        RuleFor(x => x.Request.LearnedTags)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("LearnedTags must not contain duplicates.");
    }

    static bool HasLevelsOrSingleJson(CreateMapRequest r)
    {
        if (r.Levels is { Count: > 0 })
            return r.Levels.All(l =>
                l.JsonContent.ValueKind != System.Text.Json.JsonValueKind.Undefined &&
                l.JsonContent.ValueKind != System.Text.Json.JsonValueKind.Null);
        return r.GameDetailJson.HasValue &&
               r.GameDetailJson.Value.ValueKind != System.Text.Json.JsonValueKind.Undefined &&
               r.GameDetailJson.Value.ValueKind != System.Text.Json.JsonValueKind.Null;
    }
}

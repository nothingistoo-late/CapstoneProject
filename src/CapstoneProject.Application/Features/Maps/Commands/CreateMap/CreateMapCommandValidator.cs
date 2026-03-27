using FluentValidation;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateMap;

public class CreateMapCommandValidator : AbstractValidator<CreateMapCommand>
{
    public CreateMapCommandValidator()
    {
        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");
        RuleFor(x => x.Request.Difficulty)
            .InclusiveBetween(1, 5).WithMessage("Difficulty must be between 1 and 5");
        RuleFor(x => x.Request.TimeLimitMs)
            .GreaterThan(0).WithMessage("TimeLimitMs must be positive");
        RuleFor(x => x.Request.WinCondition)
            .GreaterThan(0).WithMessage("WinCondition must be positive");
        RuleFor(x => x.Request.MapDetailJson.ValueKind)
            .NotEqual(System.Text.Json.JsonValueKind.Undefined).WithMessage("MapDetailJson is required")
            .NotEqual(System.Text.Json.JsonValueKind.Null).WithMessage("MapDetailJson is required");

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
}

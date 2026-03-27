using FluentValidation;

namespace CapstoneProject.Application.Features.Maps.Commands.UpdateMapFromJsonFile;

public class UpdateMapFromJsonFileCommandValidator : AbstractValidator<UpdateMapFromJsonFileCommand>
{
    public UpdateMapFromJsonFileCommandValidator()
    {
        RuleFor(x => x.MapId)
            .NotEmpty().WithMessage("MapId is required.");

        RuleFor(x => x.Input)
            .NotNull().WithMessage("Input is required.");

        When(x => x.Input != null, () =>
        {
            RuleFor(x => x.Input.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");
            RuleFor(x => x.Input.Difficulty)
                .InclusiveBetween(1, 5)
                .WithMessage("Difficulty must be between 1 and 5.");
            RuleFor(x => x.Input.TimeLimitMs)
                .GreaterThan(0).WithMessage("TimeLimitMs must be positive.");
            RuleFor(x => x.Input.WinCondition)
                .GreaterThan(0).WithMessage("WinCondition must be positive.");
            RuleFor(x => x.Input.TagIdsCsv)
                .Must(BeValidGuidCsv).WithMessage("TagIdsCsv contains invalid Guid(s).")
                .Must(NotContainDuplicatesInGuidCsv).WithMessage("TagIdsCsv must not contain duplicates.");
            RuleFor(x => x.Input.LearnedTagsCsv)
                .Must(BeValidGuidCsv).WithMessage("LearnedTagsCsv contains invalid Guid(s).")
                .Must(NotContainDuplicatesInGuidCsv).WithMessage("LearnedTagsCsv must not contain duplicates.");
        });
    }

    private static bool BeValidGuidCsv(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return true;
        var tokens = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return tokens.All(token => Guid.TryParse(token, out _));
    }

    private static bool NotContainDuplicatesInGuidCsv(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return true;
        var tokens = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var guids = new List<Guid>();
        foreach (var token in tokens)
        {
            if (!Guid.TryParse(token, out var id)) return true;
            guids.Add(id);
        }
        return guids.Distinct().Count() == guids.Count;
    }
}

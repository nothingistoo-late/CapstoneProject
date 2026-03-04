using FluentValidation;
using System.Text.Json;

namespace CapstoneProject.Application.Features.Maps.Commands.BatchCreateMaps;

public class BatchCreateMapsCommandValidator : AbstractValidator<BatchCreateMapsCommand>
{
    public BatchCreateMapsCommandValidator()
    {
        RuleFor(x => x.Request)
            .Must(r =>
            {
                var hasLevels = r.Levels?.Count > 0 && r.Levels.Any(e => e.ValueKind != JsonValueKind.Null && e.ValueKind != JsonValueKind.Undefined);
                var hasJsonContents = r.JsonContents?.Count > 0 && r.JsonContents.Any(s => !string.IsNullOrWhiteSpace(s));
                return hasLevels || hasJsonContents;
            })
            .WithMessage("Either Levels (array of objects) or JsonContents (array of strings) with at least one item is required.");
    }
}

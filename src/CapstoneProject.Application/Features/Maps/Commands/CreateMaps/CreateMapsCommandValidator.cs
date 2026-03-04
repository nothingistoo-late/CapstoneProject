using FluentValidation;
using System.Text.Json;
using CapstoneProject.Application.Commons.DTOs.Maps;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateMaps;

public class CreateMapsCommandValidator : AbstractValidator<CreateMapsCommand>
{
    public CreateMapsCommandValidator()
    {
        RuleFor(x => x.Request)
            .Must(r => HasValidLevel(r))
            .WithMessage("Level is required: send either 'level' (JSON object) in body or upload a JSON file (levelFile).");
    }

    private static bool HasValidLevel(CreateMapsRequest r)
    {
        if (!string.IsNullOrWhiteSpace(r.LevelJson)) return true;
        return r.Level.HasValue && r.Level.Value.ValueKind != JsonValueKind.Null && r.Level.Value.ValueKind != JsonValueKind.Undefined;
    }
}

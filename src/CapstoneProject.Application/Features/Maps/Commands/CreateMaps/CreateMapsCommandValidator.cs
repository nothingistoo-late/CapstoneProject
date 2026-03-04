using FluentValidation;
using CapstoneProject.Application.Commons.DTOs.Maps;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateMaps;

public class CreateMapsCommandValidator : AbstractValidator<CreateMapsCommand>
{
    public CreateMapsCommandValidator()
    {
        RuleFor(x => x.Request)
            .Must(r => (r.Level.HasValue && r.Level.Value.ValueKind != System.Text.Json.JsonValueKind.Null && r.Level.Value.ValueKind != System.Text.Json.JsonValueKind.Undefined) || !string.IsNullOrWhiteSpace(r.JsonContent))
            .WithMessage("Either Level (object) or JsonContent (string) is required.");
    }
}

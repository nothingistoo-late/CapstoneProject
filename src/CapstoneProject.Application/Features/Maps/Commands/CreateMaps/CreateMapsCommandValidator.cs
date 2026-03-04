using FluentValidation;
using System.Text.Json;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateMaps;

public class CreateMapsCommandValidator : AbstractValidator<CreateMapsCommand>
{
    public CreateMapsCommandValidator()
    {
        RuleFor(x => x.Request)
            .Must(r => r.Level.HasValue && r.Level.Value.ValueKind != JsonValueKind.Null && r.Level.Value.ValueKind != JsonValueKind.Undefined)
            .WithMessage("Level (object) is required.");
    }
}

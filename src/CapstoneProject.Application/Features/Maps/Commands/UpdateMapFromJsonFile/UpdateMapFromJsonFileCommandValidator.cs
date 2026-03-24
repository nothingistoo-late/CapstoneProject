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
            RuleFor(x => x.Input.Difficulty)
                .InclusiveBetween(1, 5)
                .WithMessage("Difficulty must be between 1 and 5.");
        });
    }
}

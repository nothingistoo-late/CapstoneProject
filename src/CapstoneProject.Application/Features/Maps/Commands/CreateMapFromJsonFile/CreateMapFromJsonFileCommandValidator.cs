using FluentValidation;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateMapFromJsonFile;

public class CreateMapFromJsonFileCommandValidator : AbstractValidator<CreateMapFromJsonFileCommand>
{
    public CreateMapFromJsonFileCommandValidator()
    {
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

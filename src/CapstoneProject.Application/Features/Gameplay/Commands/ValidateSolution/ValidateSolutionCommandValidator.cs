using FluentValidation;

namespace CapstoneProject.Application.Features.Gameplay.Commands.ValidateSolution;

public class ValidateSolutionCommandValidator : AbstractValidator<ValidateSolutionCommand>
{
    public ValidateSolutionCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request is required.");
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request!.MapId)
                .NotEmpty().WithMessage("Map Id is required.");
            RuleFor(x => x.Request!.Language)
                .NotEmpty().WithMessage("Language is required.")
                .MaximumLength(50).WithMessage("Language must not exceed 50 characters.");
            RuleFor(x => x.Request)
                .Must(r => !string.IsNullOrEmpty(r!.AstSpec) || !string.IsNullOrEmpty(r!.BytecodeSpec))
                .WithMessage("Either AstSpec or BytecodeSpec must be provided.");
        });
    }
}

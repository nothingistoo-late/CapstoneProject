using FluentValidation;

namespace CapstoneProject.Application.Features.Challenge.Commands.CreateConcept;

public class CreateConceptCommandValidator : AbstractValidator<CreateConceptCommand>
{
    public CreateConceptCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Concept name is required.")
            .MaximumLength(200).WithMessage("Concept name must not exceed 200 characters.");
    }
}

using FluentValidation;

namespace CapstoneProject.Application.Features.Challenge.Commands.UpdateConcept;

public class UpdateConceptCommandValidator : AbstractValidator<UpdateConceptCommand>
{
    public UpdateConceptCommandValidator()
    {
        RuleFor(x => x.ConceptId)
            .NotEmpty().WithMessage("Concept Id is required.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Concept name is required.")
            .MaximumLength(200).WithMessage("Concept name must not exceed 200 characters.");
    }
}

using FluentValidation;

namespace CapstoneProject.Application.Features.Challenge.Commands.DeleteConcept;

public class DeleteConceptCommandValidator : AbstractValidator<DeleteConceptCommand>
{
    public DeleteConceptCommandValidator()
    {
        RuleFor(x => x.ConceptId)
            .NotEmpty().WithMessage("Concept Id is required.");
    }
}

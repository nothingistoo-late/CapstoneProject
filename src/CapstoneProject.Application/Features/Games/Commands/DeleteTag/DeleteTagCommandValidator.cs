using FluentValidation;

namespace CapstoneProject.Application.Features.Games.Commands.DeleteTag;

public class DeleteTagCommandValidator : AbstractValidator<DeleteTagCommand>
{
    public DeleteTagCommandValidator()
    {
        RuleFor(x => x.TagId)
            .NotEmpty().WithMessage("Tag Id is required.");
    }
}

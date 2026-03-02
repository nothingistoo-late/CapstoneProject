using FluentValidation;

namespace CapstoneProject.Application.Features.Marketplace.Commands.DeletePackage;

public class DeletePackageCommandValidator : AbstractValidator<DeletePackageCommand>
{
    public DeletePackageCommandValidator()
    {
        RuleFor(x => x.PackageId)
            .NotEmpty().WithMessage("Package Id is required.");
    }
}

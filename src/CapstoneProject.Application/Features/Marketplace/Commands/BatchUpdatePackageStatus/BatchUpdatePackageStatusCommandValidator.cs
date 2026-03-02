using FluentValidation;

namespace CapstoneProject.Application.Features.Marketplace.Commands.BatchUpdatePackageStatus;

public class BatchUpdatePackageStatusCommandValidator : AbstractValidator<BatchUpdatePackageStatusCommand>
{
    public BatchUpdatePackageStatusCommandValidator()
    {
        RuleFor(x => x.PackageIds)
            .NotNull().WithMessage("Package Ids are required.")
            .NotEmpty().WithMessage("At least one Package Id is required.");
    }
}

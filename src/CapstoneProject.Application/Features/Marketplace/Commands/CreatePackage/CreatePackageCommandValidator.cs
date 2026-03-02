using FluentValidation;

namespace CapstoneProject.Application.Features.Marketplace.Commands.CreatePackage;

public class CreatePackageCommandValidator : AbstractValidator<CreatePackageCommand>
{
    public CreatePackageCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request is required.");
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request!.Name)
                .NotEmpty().WithMessage("Package name is required.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
            RuleFor(x => x.Request!.DurationDays)
                .GreaterThan(0).WithMessage("DurationDays must be positive.");
            RuleFor(x => x.Request!.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative.");
        });
    }
}

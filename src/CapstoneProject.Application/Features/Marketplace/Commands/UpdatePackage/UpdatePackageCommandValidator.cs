using FluentValidation;

namespace CapstoneProject.Application.Features.Marketplace.Commands.UpdatePackage;

public class UpdatePackageCommandValidator : AbstractValidator<UpdatePackageCommand>
{
    public UpdatePackageCommandValidator()
    {
        RuleFor(x => x.PackageId)
            .NotEmpty().WithMessage("Package Id is required.");
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request is required.");
        When(x => x.Request != null && x.Request.Name != null, () =>
            RuleFor(x => x.Request!.Name)
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters."));
        When(x => x.Request != null && x.Request.DurationDays.HasValue, () =>
            RuleFor(x => x.Request!.DurationDays!.Value)
                .GreaterThan(0).WithMessage("DurationDays must be positive."));
        When(x => x.Request != null && x.Request.Price.HasValue, () =>
            RuleFor(x => x.Request!.Price!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative."));
        When(x => x.Request != null && x.Request.Limit.HasValue, () =>
            RuleFor(x => x.Request!.Limit!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Limit must be non-negative."));
    }
}

using FluentValidation;
using CapstoneProject.Application.Common.Validators;
using CapstoneProject.Application.Common.DTOs.Auth;

namespace CapstoneProject.Application.Commons.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        When(x => x.FirstName != null, () =>
        {
            RuleFor(x => x.FirstName!)
                .ValidPersonName("First name", 50);
        });

        When(x => x.LastName != null, () =>
        {
            RuleFor(x => x.LastName!)
                .ValidPersonName("Last name", 50);
        });

        When(x => x.PhoneNumber != null, () =>
        {
            RuleFor(x => x.PhoneNumber!)
                .ValidPhoneNumber();
        });

        When(x => x.Bio != null, () =>
        {
            RuleFor(x => x.Bio!)
                .MaximumLength(500)
                .WithMessage("Bio must be at most 500 characters.");
        });
    }
}

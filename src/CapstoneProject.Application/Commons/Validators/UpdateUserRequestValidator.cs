using FluentValidation;
using CapstoneProject.Application.Common.Validators;
using CapstoneProject.Application.Commons.DTOs.User;

namespace CapstoneProject.Application.Commons.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .ValidPersonName("First name", 100);

        RuleFor(x => x.LastName)
            .ValidPersonName("Last name", 100);

        // Email is optional when updating - only validate if provided
        When(x => !string.IsNullOrEmpty(x.Email), () =>
        {
            RuleFor(x => x.Email!)
                .ValidEmail()
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters");
        });

        When(x => !string.IsNullOrEmpty(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber!)
                .ValidPhoneNumber();
        });
    }
}

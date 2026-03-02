using FluentValidation;
using CapstoneProject.Application.Common.Validators;
using CapstoneProject.Application.Commons.DTOs.User;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .ValidPersonName("First name", 100);

        RuleFor(x => x.LastName)
            .ValidPersonName("Last name", 100);

        RuleFor(x => x.Email)
            .ValidEmail()
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.Password)
            .ValidPassword(6);

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role. Must be Learner, Moderator, or Admin");

        When(x => x.Status.HasValue, () =>
            RuleFor(x => x.Status!.Value).IsInEnum().WithMessage("Invalid status"));

        When(x => !string.IsNullOrEmpty(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber!)
                .ValidPhoneNumber();
        });
    }
}

using FluentValidation;
using CapstoneProject.Application.Common.Validators;

namespace CapstoneProject.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.Request.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");
        RuleFor(x => x.Request.ConfirmPassword).ValidConfirmPassword(x => x.Request.ConfirmPassword);
        RuleFor(x => x.Request.NewPassword).ValidPassword(8);
    }
}
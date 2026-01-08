using FluentValidation;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Validators;

namespace CapstoneProject.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : BaseAuthValidator<LoginCommand>
{
    public LoginCommandValidator(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        SetupValidationRules();
    }

    protected override void SetupValidationRules()
    {
        RuleFor(x => x.Request.Email)
            .ValidEmail();

        RuleFor(x => x.Request.Password)
            .NotEmpty().WithMessage("Password is required");

        RuleFor(x => x.Request.GrantType)
            .IsInEnum()
            .WithMessage("Grant type is required");
    }

    // private static bool BeAValidPassword(string password)
    // {
    //     if (string.IsNullOrEmpty(password))
    //         return false;

    //     // Check for at least one uppercase letter
    //     var hasUpper = password.Any(char.IsUpper);
        
    //     // Check for at least one lowercase letter
    //     var hasLower = password.Any(char.IsLower);
        
    //     // Check for at least one digit
    //     var hasDigit = password.Any(char.IsDigit);
        
    //     // Check for at least one special character
    //     var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

    //     return hasUpper && hasLower && hasDigit && hasSpecial;
    // }
}
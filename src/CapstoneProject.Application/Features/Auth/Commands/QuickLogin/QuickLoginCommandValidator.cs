using FluentValidation;

namespace CapstoneProject.Application.Features.Auth.Commands.QuickLogin;

public class QuickLoginCommandValidator : AbstractValidator<QuickLoginCommand>
{
    public QuickLoginCommandValidator()
    {
        RuleFor(x => x.Request.QuickCode)
            .NotEmpty().WithMessage("Quick code is required")
            .MinimumLength(3).WithMessage("Quick code must be at least 3 characters");
    }
}

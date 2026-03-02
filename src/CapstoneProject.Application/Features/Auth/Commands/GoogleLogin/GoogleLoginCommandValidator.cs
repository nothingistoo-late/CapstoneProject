using FluentValidation;

namespace CapstoneProject.Application.Features.Auth.Commands.GoogleLogin;

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request is required.");
        When(x => x.Request != null, () =>
            RuleFor(x => x.Request!.IdToken)
                .NotEmpty().WithMessage("Google IdToken is required."));
    }
}

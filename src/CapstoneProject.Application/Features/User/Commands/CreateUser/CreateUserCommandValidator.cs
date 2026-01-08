using FluentValidation;
using CapstoneProject.Application.Commons.Validators;
using static CapstoneProject.Application.Common.Validators.FileValidatorExtension;

namespace CapstoneProject.Application.Features.User.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request data is required")
            .SetValidator(new CreateUserRequestValidator());

        RuleFor(x => x.AvatarFile)
            .ValidImageFile(maxSizeInMB: 10.0)
            .When(x => x.AvatarFile != null);
    }
}

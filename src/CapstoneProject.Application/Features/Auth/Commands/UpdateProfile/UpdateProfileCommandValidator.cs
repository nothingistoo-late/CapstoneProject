using FluentValidation;
using CapstoneProject.Application.Commons.Validators;
using static CapstoneProject.Application.Common.Validators.FileValidatorExtension;

namespace CapstoneProject.Application.Features.Auth.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request data is required")
            .SetValidator(new UpdateProfileRequestValidator());

        RuleFor(x => x.AvatarFile)
            .ValidImageFile(maxSizeInMB: 10.0)
            .When(x => x.AvatarFile != null);
    }
}

using FluentValidation;
using CapstoneProject.Application.Commons.Validators;
using static CapstoneProject.Application.Common.Validators.FileValidatorExtension;

namespace CapstoneProject.Application.Features.User.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("ID người dùng là bắt buộc");

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request data is required")
            .SetValidator(new UpdateUserRequestValidator());

        RuleFor(x => x.AvatarFile)
            .ValidImageFile(maxSizeInMB: 10.0)
            .When(x => x.AvatarFile != null);
    }
}

using FluentValidation;

namespace CapstoneProject.Application.Features.User.Commands.BatchUpdateUserStatus;

public class BatchUpdateUserStatusCommandValidator : AbstractValidator<BatchUpdateUserStatusCommand>
{
    public BatchUpdateUserStatusCommandValidator()
    {
        RuleFor(x => x.UserIds)
            .NotNull().WithMessage("User Ids are required.")
            .NotEmpty().WithMessage("At least one User Id is required.");
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be a valid EntityStatus value.");
    }
}

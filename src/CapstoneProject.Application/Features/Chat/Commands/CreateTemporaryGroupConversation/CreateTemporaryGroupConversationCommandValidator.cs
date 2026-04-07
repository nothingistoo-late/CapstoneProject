using FluentValidation;

namespace CapstoneProject.Application.Features.Chat.Commands.CreateTemporaryGroupConversation;

public class CreateTemporaryGroupConversationCommandValidator : AbstractValidator<CreateTemporaryGroupConversationCommand>
{
    public CreateTemporaryGroupConversationCommandValidator()
    {
        SetupValidationRules();
    }

    private void SetupValidationRules()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên nhóm là bắt buộc")
            .MaximumLength(200).WithMessage("Group name must not exceed 200 characters")
            .MinimumLength(3).WithMessage("Group name must be at least 3 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_.,!?]+$").WithMessage("Group name contains invalid characters. Only letters, numbers, spaces, and basic punctuation are allowed");
    }
}

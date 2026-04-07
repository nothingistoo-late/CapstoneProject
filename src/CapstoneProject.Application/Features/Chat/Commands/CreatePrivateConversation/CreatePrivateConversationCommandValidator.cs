using FluentValidation;
using CapstoneProject.Application.Common.Interfaces;

namespace CapstoneProject.Application.Features.Chat.Commands.CreatePrivateConversation;

public class CreatePrivateConversationCommandValidator : AbstractValidator<CreatePrivateConversationCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePrivateConversationCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        SetupValidationRules();
    }

    private void SetupValidationRules()
    {
        RuleFor(x => x.OtherUserId)
            .NotEmpty().WithMessage("Cần có ID người dùng khác")
            .MustAsync(async (otherUserId, cancellation) =>
            {
                if (otherUserId == Guid.Empty)
                    return false;

                var userRepo = _unitOfWork.Repository<Domain.Entities.AppUser>();
                var user = await userRepo.GetFirstOrDefaultAsync(u => u.Id == otherUserId && u.Status == Domain.Enums.EntityStatusEnum.Active);
                return user != null;
            }).WithMessage("Other user does not exist or is not active");
    }
}

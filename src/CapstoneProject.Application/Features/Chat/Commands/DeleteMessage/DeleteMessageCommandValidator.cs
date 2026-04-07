using FluentValidation;
using CapstoneProject.Application.Common.Interfaces;

namespace CapstoneProject.Application.Features.Chat.Commands.DeleteMessage;

public class DeleteMessageCommandValidator : AbstractValidator<DeleteMessageCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMessageCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        SetupValidationRules();
    }

    private void SetupValidationRules()
    {
        RuleFor(x => x.MessageId)
            .NotEmpty().WithMessage("ID tin nhắn là bắt buộc")
            .MustAsync(async (messageId, cancellation) =>
            {
                if (messageId == Guid.Empty)
                    return false;

                var messageRepo = _unitOfWork.Repository<Domain.Entities.Message>();
                var message = await messageRepo.GetFirstOrDefaultAsync(
                    m => m.Id == messageId && !m.IsDeleted);

                return message != null;
            }).WithMessage("Message not found or has been deleted");
    }
}

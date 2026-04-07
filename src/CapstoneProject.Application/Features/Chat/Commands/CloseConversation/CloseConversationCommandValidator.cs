using FluentValidation;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Chat.Commands.CloseConversation;

public class CloseConversationCommandValidator : AbstractValidator<CloseConversationCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CloseConversationCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        SetupValidationRules();
    }

    private void SetupValidationRules()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("ID cuộc trò chuyện là bắt buộc")
            .MustAsync(async (conversationId, cancellation) =>
            {
                if (conversationId == Guid.Empty)
                    return false;

                var conversationRepo = _unitOfWork.Repository<Domain.Entities.ChatRoom>();
                var conversation = await conversationRepo.GetFirstOrDefaultAsync(
                    c => c.Id == conversationId && !c.IsDeleted);

                return conversation != null;
            }).WithMessage("Không tìm thấy cuộc trò chuyện");

        RuleFor(x => x.ConversationId)
            .MustAsync(async (conversationId, cancellation) =>
            {
                if (conversationId == Guid.Empty)
                    return false;

                var conversationRepo = _unitOfWork.Repository<Domain.Entities.ChatRoom>();
                var conversation = await conversationRepo.GetFirstOrDefaultAsync(
                    c => c.Id == conversationId && !c.IsDeleted);

                if (conversation == null)
                    return false;

                // Only temporary group conversations can be closed
                return conversation.RoomType == ChatRoomTypeEnum.TemporaryGroup;
            }).WithMessage("Only temporary group conversations can be closed");
    }
}

using FluentValidation;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Chat.Commands.SendMessage;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public SendMessageCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        SetupValidationRules();
    }

    private void SetupValidationRules()
    {
        RuleFor(x => x.Request.ChatRoomId)
            .NotEmpty().WithMessage("Cần có ID phòng trò chuyện")
            .MustAsync(async (chatRoomId, cancellation) =>
            {
                if (chatRoomId == Guid.Empty)
                    return false;

                var conversationRepo = _unitOfWork.Repository<Domain.Entities.ChatRoom>();
                var conversation = await conversationRepo.GetFirstOrDefaultAsync(
                    c => c.Id == chatRoomId && !c.IsDeleted);

                return conversation != null;
            }).WithMessage("Không tìm thấy cuộc trò chuyện");

        RuleFor(x => x.Request)
            .MustAsync(async (request, cancellation) =>
            {
                if (request.ChatRoomId == Guid.Empty)
                    return false;

                var conversationRepo = _unitOfWork.Repository<Domain.Entities.ChatRoom>();
                var conversation = await conversationRepo.GetFirstOrDefaultAsync(
                    c => c.Id == request.ChatRoomId && !c.IsDeleted);

                if (conversation == null)
                    return false;

                // Cannot send messages to closed conversations
                return !conversation.IsClosed;
            }).WithMessage("Không thể gửi tin nhắn đến cuộc trò chuyện đã đóng");

        RuleFor(x => x.Request.Content)
            .NotEmpty().WithMessage("Message content is required")
            .When(x => x.Request.MessageType == MessageTypeEnum.Text)
            .MaximumLength(5000).WithMessage("Nội dung tin nhắn không được vượt quá 5000 ký tự");

        RuleFor(x => x.Request.MessageType)
            .IsInEnum().WithMessage("Invalid message type");

        // File validation rules
        When(x => x.Request.MessageType == MessageTypeEnum.File || 
                  x.Request.MessageType == MessageTypeEnum.Image ||
                  x.Request.MessageType == MessageTypeEnum.Video ||
                  x.Request.MessageType == MessageTypeEnum.Audio, () =>
        {
            RuleFor(x => x.FilePath)
                .NotEmpty().WithMessage("File path is required for file attachments");

            RuleFor(x => x.Request.FileName)
                .NotEmpty().WithMessage("File name is required for file attachments")
                .MaximumLength(255).WithMessage("File name must not exceed 255 characters");

            RuleFor(x => x.Request.FileSize)
                .GreaterThan(0).WithMessage("File size must be greater than 0")
                .LessThanOrEqualTo(10 * 1024 * 1024) // 10MB max
                .WithMessage("File size must not exceed 10MB");
        });

        // Reply validation
        When(x => x.Request.ReplyToMessageId.HasValue, () =>
        {
            RuleFor(x => x.Request.ReplyToMessageId)
                .MustAsync(async (messageId, cancellation) =>
                {
                    if (!messageId.HasValue || messageId.Value == Guid.Empty)
                        return false;

                    var messageRepo = _unitOfWork.Repository<Domain.Entities.Message>();
                    var message = await messageRepo.GetFirstOrDefaultAsync(
                        m => m.Id == messageId.Value && !m.IsDeleted);

                    return message != null;
                }).WithMessage("Reply to message not found or has been deleted");
        });
    }
}

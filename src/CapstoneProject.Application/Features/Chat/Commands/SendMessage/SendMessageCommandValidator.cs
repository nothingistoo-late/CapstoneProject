using FluentValidation;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Chat.Commands.SendMessage;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".webp"
    };

    private readonly IUnitOfWork _unitOfWork;

    public SendMessageCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        SetupValidationRules();
    }

    private void SetupValidationRules()
    {
        RuleFor(x => x.ChatRoomId)
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

        RuleFor(x => x.ChatRoomId)
            .MustAsync(async (request, cancellation) =>
            {
                if (request == Guid.Empty)
                    return false;

                var conversationRepo = _unitOfWork.Repository<Domain.Entities.ChatRoom>();
                var conversation = await conversationRepo.GetFirstOrDefaultAsync(
                    c => c.Id == request && !c.IsDeleted);

                if (conversation == null)
                    return false;

                // Cannot send messages to closed conversations
                return !conversation.IsClosed;
            }).WithMessage("Không thể gửi tin nhắn đến cuộc trò chuyện đã đóng");

        // Content is required ONLY when sending a plain text message (no image attached)
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Nội dung tin nhắn không được để trống")
            .When(x => x.ImageFile == null);

        // Max length always applies (when content is provided)
        RuleFor(x => x.Content)
            .MaximumLength(5000).WithMessage("Nội dung tin nhắn không được vượt quá 5000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Content));

        When(x => x.ImageFile != null, () =>
        {
            RuleFor(x => x.ImageFile!)
                .Must(file => file.Length > 0).WithMessage("File ảnh không hợp lệ")
                .Must(file => file.Length <= 10 * 1024 * 1024).WithMessage("Ảnh tải lên không được vượt quá 10MB")
                .Must(file =>
                {
                    var extension = Path.GetExtension(file.FileName);
                    return !string.IsNullOrWhiteSpace(extension) && AllowedImageExtensions.Contains(extension);
                }).WithMessage("Chỉ hỗ trợ ảnh PNG, JPG, JPEG, GIF và WEBP");
        });

        // Reply validation
        When(x => x.ReplyToMessageId.HasValue, () =>
        {
            RuleFor(x => x.ReplyToMessageId)
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

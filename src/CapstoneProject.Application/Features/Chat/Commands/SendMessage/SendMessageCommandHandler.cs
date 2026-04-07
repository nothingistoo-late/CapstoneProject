using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Commons.DTOs.Chat;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Chat.Commands.SendMessage;

/// <summary>
/// Handler for sending messages in conversations.
/// Validates conversation is not closed and user is a participant.
/// </summary>
public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<MessageResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConversationService _conversationService;
    private readonly IChatBroadcastService _broadcastService;
    private readonly IAvatarUrlResolverService _avatarUrlResolver;
    private readonly ILogger<SendMessageCommandHandler> _logger;

    public SendMessageCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IConversationService conversationService,
        IChatBroadcastService broadcastService,
        IAvatarUrlResolverService avatarUrlResolver,
        ILogger<SendMessageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _conversationService = conversationService;
        _broadcastService = broadcastService;
        _avatarUrlResolver = avatarUrlResolver;
        _logger = logger;
    }

    public async Task<Result<MessageResponse>> Handle(SendMessageCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var userIdString = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
            {
                return Result<MessageResponse>.Failure("Người dùng chưa được xác thực", ErrorCodeEnum.Unauthorized);
            }

            var request = command.Request;

            // Validate request
            if (request == null)
            {
                return Result<MessageResponse>.Failure("Yêu cầu không thể rỗng", ErrorCodeEnum.InvalidInput);
            }

            if (request.ChatRoomId == Guid.Empty)
            {
                return Result<MessageResponse>.Failure("Cần có ID phòng trò chuyện", ErrorCodeEnum.InvalidInput);
            }

            // Get conversation and validate
            var conversation = await _conversationService.GetConversationAsync(request.ChatRoomId, cancellationToken);
            if (conversation == null)
            {
                return Result<MessageResponse>.Failure("Không tìm thấy cuộc trò chuyện", ErrorCodeEnum.NotFound);
            }

            // Validate conversation is not closed
            if (!conversation.CanSendMessages())
            {
                return Result<MessageResponse>.Failure("Không thể gửi tin nhắn đến cuộc trò chuyện đã đóng", ErrorCodeEnum.InvalidOperation);
            }

            // Verify user is a participant
            var isParticipant = await _conversationService.IsParticipantAsync(request.ChatRoomId, currentUserId, cancellationToken);
            if (!isParticipant)
            {
                return Result<MessageResponse>.Failure("Bạn không phải là người tham gia vào cuộc trò chuyện này", ErrorCodeEnum.Forbidden);
            }

            // Validate content based on message type
            if (request.MessageType == MessageTypeEnum.Text && string.IsNullOrWhiteSpace(request.Content))
            {
                return Result<MessageResponse>.Failure("Nội dung tin nhắn là bắt buộc đối với tin nhắn văn bản", ErrorCodeEnum.InvalidInput);
            }

        // Create message
        var messageRepo = _unitOfWork.Repository<Message>();
        var message = new Message
        {
            ChatRoomId = request.ChatRoomId,
            SenderId = currentUserId,
            Content = request.Content,
            MessageType = request.MessageType,
            FilePath = command.FilePath,
            FileName = request.FileName,
            FileSize = request.FileSize,
            ReplyToMessageId = request.ReplyToMessageId,
            CreatedBy = currentUserId,
            CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
            Status = EntityStatusEnum.Active
        };

        await messageRepo.AddAsync(message);

        // Update conversation last message
        var conversationRepo = _unitOfWork.Repository<ChatRoom>();
        conversation.LastMessageId = message.Id;
        conversation.LastMessageAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
        conversation.UpdatedBy = currentUserId;
        conversation.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
        conversationRepo.Update(conversation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load message with sender info
        var messageWithSender = await messageRepo.GetQueryable()
            .Where(m => m.Id == message.Id)
            .Include(m => m.Sender)
            .Include(m => m.ReplyToMessage)
            .ThenInclude(rm => rm!.Sender)
            .FirstOrDefaultAsync(cancellationToken);

        if (messageWithSender == null)
        {
            return Result<MessageResponse>.Failure("Không tạo được tin nhắn", ErrorCodeEnum.InternalError);
        }

        // Map to response
        var response = new MessageResponse
        {
            Id = messageWithSender.Id,
            ChatRoomId = messageWithSender.ChatRoomId,
            SenderId = messageWithSender.SenderId,
            SenderName = $"{messageWithSender.Sender.FirstName} {messageWithSender.Sender.LastName}".Trim(),
            SenderAvatar = _avatarUrlResolver.ResolveAvatarUrl(messageWithSender.Sender.AvatarPath),
            Content = messageWithSender.Content,
            MessageType = messageWithSender.MessageType,
            FilePath = messageWithSender.FilePath,
            FileName = messageWithSender.FileName,
            FileSize = messageWithSender.FileSize,
            ReplyToMessageId = messageWithSender.ReplyToMessageId,
            ReplyToMessage = messageWithSender.ReplyToMessage != null ? new MessageResponse
            {
                Id = messageWithSender.ReplyToMessage.Id,
                Content = messageWithSender.ReplyToMessage.Content,
                SenderName = $"{messageWithSender.ReplyToMessage.Sender.FirstName} {messageWithSender.ReplyToMessage.Sender.LastName}".Trim()
            } : null,
            IsEdited = messageWithSender.IsEdited,
            EditedAt = messageWithSender.EditedAt,
            IsDeleted = messageWithSender.IsDeleted,
            CreatedAt = messageWithSender.CreatedAt ?? CapstoneProject.Domain.Common.VietnamDateTime.DbNow
        };

            // Broadcast message via SignalR
            try
            {
                await _broadcastService.BroadcastMessageAsync(request.ChatRoomId, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast message {MessageId} to conversation {ConversationId}. Message was saved but notification failed.", response.Id, request.ChatRoomId);
                // Don't fail the operation if broadcast fails - message is already saved
            }

            return Result<MessageResponse>.Success(response);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while sending message to conversation {ConversationId}", command.Request?.ChatRoomId);
            return Result<MessageResponse>.Failure("Không gửi được tin nhắn do lỗi cơ sở dữ liệu", ErrorCodeEnum.DatabaseError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while sending message to conversation {ConversationId}", command.Request?.ChatRoomId);
            return Result<MessageResponse>.Failure("Đã xảy ra lỗi không mong muốn khi gửi tin nhắn", ErrorCodeEnum.InternalError);
        }
    }
}




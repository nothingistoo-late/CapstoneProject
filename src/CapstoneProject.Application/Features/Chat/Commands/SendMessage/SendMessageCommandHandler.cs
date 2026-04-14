using MediatR;
using Microsoft.AspNetCore.Http;
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
    private const long MaxImageSizeBytes = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".webp"
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConversationService _conversationService;
    private readonly IChatBroadcastService _broadcastService;
    private readonly IAvatarUrlResolverService _avatarUrlResolver;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<SendMessageCommandHandler> _logger;

    public SendMessageCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IConversationService conversationService,
        IChatBroadcastService broadcastService,
        IAvatarUrlResolverService avatarUrlResolver,
        ICloudinaryService cloudinaryService,
        ILogger<SendMessageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _conversationService = conversationService;
        _broadcastService = broadcastService;
        _avatarUrlResolver = avatarUrlResolver;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    public async Task<Result<MessageResponse>> Handle(SendMessageCommand command, CancellationToken cancellationToken)
    {
        string? uploadedImageUrl = null;

        try
        {
            var userIdString = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
            {
                return Result<MessageResponse>.Failure("Người dùng chưa được xác thực", ErrorCodeEnum.Unauthorized);
            }

            if (command.ChatRoomId == Guid.Empty)
            {
                return Result<MessageResponse>.Failure("Cần có ID phòng trò chuyện", ErrorCodeEnum.InvalidInput);
            }

            // Get conversation and validate
            var conversation = await _conversationService.GetConversationAsync(command.ChatRoomId, cancellationToken);
            if (conversation == null)
            {
                return Result<MessageResponse>.Failure("Không tìm thấy cuộc trò chuyện", ErrorCodeEnum.NotFound);
            }

            if (!conversation.CanSendMessages())
            {
                return Result<MessageResponse>.Failure("Không thể gửi tin nhắn đến cuộc trò chuyện đã đóng", ErrorCodeEnum.InvalidOperation);
            }

            var isParticipant = await _conversationService.IsParticipantAsync(command.ChatRoomId, currentUserId, cancellationToken);
            if (!isParticipant)
            {
                return Result<MessageResponse>.Failure("Bạn không phải là người tham gia vào cuộc trò chuyện này", ErrorCodeEnum.Forbidden);
            }

            var effectiveMessageType = command.ImageFile != null ? MessageTypeEnum.Image : command.MessageType;
            if (effectiveMessageType == MessageTypeEnum.Text && string.IsNullOrWhiteSpace(command.Content))
            {
                return Result<MessageResponse>.Failure("Nội dung tin nhắn là bắt buộc đối với tin nhắn văn bản", ErrorCodeEnum.InvalidInput);
            }

            if (command.ImageFile != null)
            {
                if (command.ImageFile.Length <= 0)
                {
                    return Result<MessageResponse>.Failure("File ảnh không hợp lệ", ErrorCodeEnum.InvalidInput);
                }

                if (command.ImageFile.Length > MaxImageSizeBytes)
                {
                    return Result<MessageResponse>.Failure("Ảnh tải lên không được vượt quá 10MB", ErrorCodeEnum.InvalidInput);
                }

                var extension = Path.GetExtension(command.ImageFile.FileName);
                if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
                {
                    return Result<MessageResponse>.Failure("Chỉ hỗ trợ ảnh PNG, JPG, JPEG, GIF và WEBP", ErrorCodeEnum.InvalidInput);
                }

                uploadedImageUrl = await _cloudinaryService.UploadImageAsync(
                    command.ImageFile,
                    "chat/messages",
                    $"chat_{command.ChatRoomId:N}_{Guid.NewGuid():N}",
                    cancellationToken);

                if (string.IsNullOrWhiteSpace(uploadedImageUrl))
                {
                    return Result<MessageResponse>.Failure("Không thể tải lên ảnh tin nhắn", ErrorCodeEnum.FileUploadFailed);
                }
            }

            var messageRepo = _unitOfWork.Repository<Message>();
            var now = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
            var message = new Message
            {
                ChatRoomId = command.ChatRoomId,
                SenderId = currentUserId,
                Content = (command.Content ?? string.Empty).Trim(),
                MessageType = effectiveMessageType,
                FilePath = uploadedImageUrl,
                FileName = command.ImageFile?.FileName,
                FileSize = command.ImageFile?.Length,
                ReplyToMessageId = command.ReplyToMessageId,
                CreatedBy = currentUserId,
                CreatedAt = now,
                Status = EntityStatusEnum.Active
            };

            await messageRepo.AddAsync(message);

            var conversationRepo = _unitOfWork.Repository<ChatRoom>();
            conversation.LastMessageId = message.Id;
            conversation.LastMessageAt = now;
            conversation.UpdatedBy = currentUserId;
            conversation.UpdatedAt = now;
            conversationRepo.Update(conversation);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var messageWithSender = await messageRepo.GetQueryable()
                .Where(m => m.Id == message.Id)
                .Include(m => m.Sender)
                .Include(m => m.ReplyToMessage)
                .ThenInclude(rm => rm!.Sender)
                .FirstOrDefaultAsync(cancellationToken);

            if (messageWithSender == null)
            {
                await CleanupUploadedImageAsync(uploadedImageUrl, cancellationToken);
                return Result<MessageResponse>.Failure("Không tạo được tin nhắn", ErrorCodeEnum.InternalError);
            }

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
                CreatedAt = messageWithSender.CreatedAt ?? now
            };

            try
            {
                await _broadcastService.BroadcastMessageAsync(command.ChatRoomId, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast message {MessageId} to conversation {ConversationId}. Message was saved but notification failed.", response.Id, command.ChatRoomId);
            }

            return Result<MessageResponse>.Success(response, "Đã gửi tin nhắn.");
        }
        catch (DbUpdateException ex)
        {
            await CleanupUploadedImageAsync(uploadedImageUrl, cancellationToken);
            _logger.LogError(ex, "Database error while sending message to conversation {ConversationId}", command.ChatRoomId);
            return Result<MessageResponse>.Failure("Không gửi được tin nhắn do lỗi cơ sở dữ liệu", ErrorCodeEnum.DatabaseError);
        }
        catch (Exception ex)
        {
            await CleanupUploadedImageAsync(uploadedImageUrl, cancellationToken);
            _logger.LogError(ex, "Unexpected error while sending message to conversation {ConversationId}", command.ChatRoomId);
            return Result<MessageResponse>.Failure("Đã xảy ra lỗi không mong muốn khi gửi tin nhắn", ErrorCodeEnum.InternalError);
        }
    }

    private async Task CleanupUploadedImageAsync(string? uploadedImageUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(uploadedImageUrl)) return;

        try
        {
            var publicId = _cloudinaryService.GetPublicIdFromUrl(uploadedImageUrl);
            if (!string.IsNullOrWhiteSpace(publicId))
            {
                await _cloudinaryService.DeleteAsync(publicId, cancellationToken);
            }
        }
        catch
        {
            // Ignore cleanup failures.
        }
    }
}




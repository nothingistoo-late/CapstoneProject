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

namespace CapstoneProject.Application.Features.Chat.Commands.UpdateMessage;

public class UpdateMessageCommandHandler : IRequestHandler<UpdateMessageCommand, Result<MessageResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAvatarUrlResolverService _avatarUrlResolver;
    private readonly ILogger<UpdateMessageCommandHandler> _logger;

    public UpdateMessageCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAvatarUrlResolverService avatarUrlResolver,
        ILogger<UpdateMessageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _avatarUrlResolver = avatarUrlResolver;
        _logger = logger;
    }

    public async Task<Result<MessageResponse>> Handle(UpdateMessageCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var userIdString = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
            {
                return Result<MessageResponse>.Failure("User not authenticated", ErrorCodeEnum.Unauthorized);
            }

            if (command == null)
            {
                return Result<MessageResponse>.Failure("Command cannot be null", ErrorCodeEnum.InvalidInput);
            }

            if (command.MessageId == Guid.Empty)
            {
                return Result<MessageResponse>.Failure("Message ID is required", ErrorCodeEnum.InvalidInput);
            }

            if (string.IsNullOrWhiteSpace(command.Content))
            {
                return Result<MessageResponse>.Failure("Message content cannot be empty", ErrorCodeEnum.InvalidInput);
            }

            if (command.Content.Length > 5000)
            {
                return Result<MessageResponse>.Failure("Message content must not exceed 5000 characters", ErrorCodeEnum.InvalidInput);
            }

            var messageRepo = _unitOfWork.Repository<Message>();

            var message = await messageRepo.GetQueryable()
                .Where(m => m.Id == command.MessageId)
                .Include(m => m.Sender)
                .Include(m => m.ReplyToMessage)
                .ThenInclude(rm => rm!.Sender)
                .FirstOrDefaultAsync(cancellationToken);

            if (message == null)
            {
                return Result<MessageResponse>.Failure("Message not found", ErrorCodeEnum.NotFound);
            }

            if (message.SenderId != currentUserId)
            {
                return Result<MessageResponse>.Failure("You can only edit your own messages", ErrorCodeEnum.Forbidden);
            }

            if (message.IsDeleted)
            {
                return Result<MessageResponse>.Failure("Cannot edit deleted message", ErrorCodeEnum.InvalidOperation);
            }

            // Check if conversation is closed
            var conversationRepo = _unitOfWork.Repository<ChatRoom>();
            var conversation = await conversationRepo.GetFirstOrDefaultAsync(c => c.Id == message.ChatRoomId && !c.IsDeleted);
            if (conversation != null && !conversation.CanSendMessages())
            {
                return Result<MessageResponse>.Failure("Cannot edit messages in a closed conversation", ErrorCodeEnum.InvalidOperation);
            }

            message.Content = command.Content.Trim();
            message.IsEdited = true;
            message.EditedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
            message.UpdatedBy = currentUserId;
            message.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;

            messageRepo.Update(message);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new MessageResponse
            {
                Id = message.Id,
                ChatRoomId = message.ChatRoomId,
                SenderId = message.SenderId,
                SenderName = $"{message.Sender.FirstName} {message.Sender.LastName}".Trim(),
                SenderAvatar = _avatarUrlResolver.ResolveAvatarUrl(message.Sender.AvatarPath),
                Content = message.Content,
                MessageType = message.MessageType,
                FilePath = message.FilePath,
                FileName = message.FileName,
                FileSize = message.FileSize,
                ReplyToMessageId = message.ReplyToMessageId,
                IsEdited = message.IsEdited,
                EditedAt = message.EditedAt,
                IsDeleted = message.IsDeleted,
                CreatedAt = message.CreatedAt ?? CapstoneProject.Domain.Common.VietnamDateTime.DbNow
            };

            return Result<MessageResponse>.Success(response);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while updating message {MessageId}", command?.MessageId);
            return Result<MessageResponse>.Failure("Failed to update message due to database error", ErrorCodeEnum.DatabaseError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating message {MessageId}", command?.MessageId);
            return Result<MessageResponse>.Failure("An unexpected error occurred while updating the message", ErrorCodeEnum.InternalError);
        }
    }
}




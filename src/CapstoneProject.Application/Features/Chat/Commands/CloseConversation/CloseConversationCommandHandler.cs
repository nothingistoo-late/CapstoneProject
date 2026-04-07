using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;

namespace CapstoneProject.Application.Features.Chat.Commands.CloseConversation;

public class CloseConversationCommandHandler : IRequestHandler<CloseConversationCommand, Result<bool>>
{
    private readonly IConversationService _conversationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IChatBroadcastService _broadcastService;
    private readonly ILogger<CloseConversationCommandHandler> _logger;

    public CloseConversationCommandHandler(
        IConversationService conversationService,
        ICurrentUserService currentUserService,
        IChatBroadcastService broadcastService,
        ILogger<CloseConversationCommandHandler> logger)
    {
        _conversationService = conversationService;
        _currentUserService = currentUserService;
        _broadcastService = broadcastService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(CloseConversationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var userIdString = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
            {
                return Result<bool>.Failure("Người dùng chưa được xác thực", ErrorCodeEnum.Unauthorized);
            }

            if (command == null)
            {
                return Result<bool>.Failure("Lệnh không thể rỗng", ErrorCodeEnum.InvalidInput);
            }

            if (command.ConversationId == Guid.Empty)
            {
                return Result<bool>.Failure("ID cuộc trò chuyện là bắt buộc", ErrorCodeEnum.InvalidInput);
            }

            await _conversationService.CloseConversationAsync(command.ConversationId, currentUserId, cancellationToken);
            
            // Broadcast closure notification to all participants via SignalR
            try
            {
                await _broadcastService.BroadcastConversationClosedAsync(command.ConversationId, currentUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast conversation closed notification for {ConversationId}. Conversation was closed but notification failed.", command.ConversationId);
                // Don't fail the operation if broadcast fails - conversation is already closed
            }
            
            return Result<bool>.Success(true);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while closing conversation: {Message}", ex.Message);
            return Result<bool>.Failure(ex.Message, ErrorCodeEnum.InvalidInput);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business logic error while closing conversation: {Message}", ex.Message);
            return Result<bool>.Failure(ex.Message, ErrorCodeEnum.InvalidOperation);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while closing conversation {ConversationId}", command?.ConversationId);
            return Result<bool>.Failure("Không thể đóng cuộc trò chuyện do lỗi cơ sở dữ liệu", ErrorCodeEnum.DatabaseError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while closing conversation {ConversationId}", command?.ConversationId);
            return Result<bool>.Failure("Đã xảy ra lỗi không mong muốn khi kết thúc cuộc trò chuyện", ErrorCodeEnum.InternalError);
        }
    }
}

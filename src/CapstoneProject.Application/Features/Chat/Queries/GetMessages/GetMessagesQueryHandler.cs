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

namespace CapstoneProject.Application.Features.Chat.Queries.GetMessages;

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, Result<PaginationResult<MessageResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetMessagesQueryHandler> _logger;

    public GetMessagesQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetMessagesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<PaginationResult<MessageResponse>>> Handle(GetMessagesQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var userIdString = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
            {
                return Result<PaginationResult<MessageResponse>>.Failure("User not authenticated", ErrorCodeEnum.Unauthorized);
            }

            if (query == null || query.Request == null)
            {
                return Result<PaginationResult<MessageResponse>>.Failure("Query request cannot be null", ErrorCodeEnum.InvalidInput);
            }

            var request = query.Request;

            // Validate pagination parameters
            if (request.PageNumber < 1)
            {
                request.PageNumber = 1;
            }

            if (request.PageSize < 1 || request.PageSize > 100)
            {
                request.PageSize = Math.Clamp(request.PageSize, 1, 100);
            }

            if (request.ChatRoomId == Guid.Empty)
            {
                return Result<PaginationResult<MessageResponse>>.Failure("Chat room ID is required", ErrorCodeEnum.InvalidInput);
            }

            var memberRepo = _unitOfWork.Repository<ChatRoomMember>();
            var messageRepo = _unitOfWork.Repository<Message>();

            // Verify user is member of the chat room
            var isMember = await memberRepo.AnyAsync(
                m => m.ChatRoomId == request.ChatRoomId && 
                     m.UserId == currentUserId && 
                     m.LeftAt == null &&
                     m.Status == EntityStatusEnum.Active);

            if (!isMember)
            {
                return Result<PaginationResult<MessageResponse>>.Failure("You are not a member of this chat room", ErrorCodeEnum.Forbidden);
            }

        // Build query
        var messagesQuery = messageRepo.GetQueryable()
            .Where(m => m.ChatRoomId == request.ChatRoomId && !m.IsDeleted)
            .Include(m => m.Sender)
            .Include(m => m.ReplyToMessage)
            .ThenInclude(rm => rm!.Sender)
            .Include(m => m.MessageReads)
            .ThenInclude(mr => mr.User)
            .OrderByDescending(m => m.CreatedAt);

        // Apply pagination with before message id
        if (request.BeforeMessageId.HasValue)
        {
            var beforeMessage = await messagesQuery
                .FirstOrDefaultAsync(m => m.Id == request.BeforeMessageId.Value, cancellationToken);
            
            if (beforeMessage != null)
            {
                var beforeDate = beforeMessage.CreatedAt ?? DateTime.MinValue;
                messagesQuery = messagesQuery
                    .Where(m => m.Id != request.BeforeMessageId.Value && 
                        (m.CreatedAt ?? DateTime.MinValue) < beforeDate)
                    .OrderByDescending(m => m.CreatedAt);
            }
        }

        var totalCount = await messagesQuery.CountAsync(cancellationToken);

        var messages = await messagesQuery
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var messageResponses = messages.Select(m => new MessageResponse
        {
            Id = m.Id,
            ChatRoomId = m.ChatRoomId,
            SenderId = m.SenderId,
            SenderName = $"{m.Sender.FirstName} {m.Sender.LastName}".Trim(),
            SenderAvatar = m.Sender.AvatarPath,
            Content = m.Content,
            MessageType = m.MessageType,
            FilePath = m.FilePath,
            FileName = m.FileName,
            FileSize = m.FileSize,
            ReplyToMessageId = m.ReplyToMessageId,
            ReplyToMessage = m.ReplyToMessage != null ? new MessageResponse
            {
                Id = m.ReplyToMessage.Id,
                Content = m.ReplyToMessage.Content,
                SenderName = $"{m.ReplyToMessage.Sender.FirstName} {m.ReplyToMessage.Sender.LastName}".Trim()
            } : null,
            IsEdited = m.IsEdited,
            EditedAt = m.EditedAt,
            IsDeleted = m.IsDeleted,
            CreatedAt = m.CreatedAt ?? DateTime.UtcNow,
            ReadBy = m.MessageReads.Select(mr => new MessageReadResponse
            {
                UserId = mr.UserId,
                UserName = $"{mr.User.FirstName} {mr.User.LastName}".Trim(),
                ReadAt = mr.ReadAt
            }).ToList()
        }).ToList();

            var result = PaginationResult<MessageResponse>.Success(
                messageResponses.ToList(),
                request.PageNumber,
                request.PageSize,
                totalCount);

            return Result<PaginationResult<MessageResponse>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving messages for conversation {ConversationId}", query?.Request?.ChatRoomId);
            return Result<PaginationResult<MessageResponse>>.Failure("An unexpected error occurred while retrieving messages", ErrorCodeEnum.InternalError);
        }
    }
}

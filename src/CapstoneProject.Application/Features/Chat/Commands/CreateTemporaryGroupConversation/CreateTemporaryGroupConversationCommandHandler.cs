using AutoMapper;
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

namespace CapstoneProject.Application.Features.Chat.Commands.CreateTemporaryGroupConversation;

public class CreateTemporaryGroupConversationCommandHandler : IRequestHandler<CreateTemporaryGroupConversationCommand, Result<ChatRoomResponse>>
{
    private readonly IConversationService _conversationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAvatarUrlResolverService _avatarUrlResolver;
    private readonly ILogger<CreateTemporaryGroupConversationCommandHandler> _logger;

    public CreateTemporaryGroupConversationCommandHandler(
        IConversationService conversationService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAvatarUrlResolverService avatarUrlResolver,
        ILogger<CreateTemporaryGroupConversationCommandHandler> logger)
    {
        _conversationService = conversationService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _avatarUrlResolver = avatarUrlResolver;
        _logger = logger;
    }

    public async Task<Result<ChatRoomResponse>> Handle(CreateTemporaryGroupConversationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var userIdString = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
            {
                return Result<ChatRoomResponse>.Failure("User not authenticated", ErrorCodeEnum.Unauthorized);
            }

            if (command == null)
            {
                return Result<ChatRoomResponse>.Failure("Command cannot be null", ErrorCodeEnum.InvalidInput);
            }

            if (string.IsNullOrWhiteSpace(command.Name))
            {
                return Result<ChatRoomResponse>.Failure("Group name is required", ErrorCodeEnum.InvalidInput);
            }

            var conversation = await _conversationService.CreateTemporaryGroupConversationAsync(command.Name, currentUserId, cancellationToken);
            var response = await MapToResponseAsync(conversation, currentUserId, cancellationToken);
            
            return Result<ChatRoomResponse>.Success(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating temporary group conversation: {Message}", ex.Message);
            return Result<ChatRoomResponse>.Failure(ex.Message, ErrorCodeEnum.InvalidInput);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business logic error while creating temporary group conversation: {Message}", ex.Message);
            return Result<ChatRoomResponse>.Failure(ex.Message, ErrorCodeEnum.InvalidOperation);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while creating temporary group conversation");
            return Result<ChatRoomResponse>.Failure("Failed to create conversation due to database error", ErrorCodeEnum.DatabaseError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating temporary group conversation");
            return Result<ChatRoomResponse>.Failure("An unexpected error occurred while creating the conversation", ErrorCodeEnum.InternalError);
        }
    }

    private async Task<ChatRoomResponse> MapToResponseAsync(Domain.Entities.ChatRoom conversation, Guid currentUserId, CancellationToken cancellationToken)
    {
        var memberRepo = _unitOfWork.Repository<Domain.Entities.ChatRoomMember>();
        var members = await memberRepo.GetQueryable()
            .Where(m => m.ChatRoomId == conversation.Id && m.LeftAt == null)
            .Include(m => m.User)
            .ToListAsync(cancellationToken);

        return new ChatRoomResponse
        {
            Id = conversation.Id,
            Name = conversation.Name,
            RoomType = conversation.RoomType,
            IsClosed = conversation.IsClosed,
            ClosedAt = conversation.ClosedAt,
            LastMessageId = conversation.LastMessageId,
            LastMessageAt = conversation.LastMessageAt,
            Members = members.Select(m => new ChatRoomMemberResponse
            {
                Id = m.Id,
                UserId = m.UserId,
                UserName = $"{m.User.FirstName} {m.User.LastName}".Trim(),
                AvatarPath = _avatarUrlResolver.ResolveAvatarUrl(m.User.AvatarPath),
                JoinedAt = m.JoinedAt,
                LastReadAt = m.LastReadAt
            }).ToList(),
            CreatedAt = conversation.CreatedAt ?? DateTime.UtcNow
        };
    }
}

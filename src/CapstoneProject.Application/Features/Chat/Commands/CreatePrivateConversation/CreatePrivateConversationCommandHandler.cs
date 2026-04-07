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

namespace CapstoneProject.Application.Features.Chat.Commands.CreatePrivateConversation;

public class CreatePrivateConversationCommandHandler : IRequestHandler<CreatePrivateConversationCommand, Result<ChatRoomResponse>>
{
    private readonly IConversationService _conversationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAvatarUrlResolverService _avatarUrlResolver;
    private readonly ILogger<CreatePrivateConversationCommandHandler> _logger;

    public CreatePrivateConversationCommandHandler(
        IConversationService conversationService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAvatarUrlResolverService avatarUrlResolver,
        ILogger<CreatePrivateConversationCommandHandler> logger)
    {
        _conversationService = conversationService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _avatarUrlResolver = avatarUrlResolver;
        _logger = logger;
    }

    public async Task<Result<ChatRoomResponse>> Handle(CreatePrivateConversationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var userIdString = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
            {
                return Result<ChatRoomResponse>.Failure("Người dùng chưa được xác thực", ErrorCodeEnum.Unauthorized);
            }

            if (command.OtherUserId == Guid.Empty)
            {
                return Result<ChatRoomResponse>.Failure("Cần có ID người dùng khác", ErrorCodeEnum.InvalidInput);
            }

            if (command.OtherUserId == currentUserId)
            {
                return Result<ChatRoomResponse>.Failure("Không thể tạo cuộc trò chuyện với chính mình", ErrorCodeEnum.InvalidOperation);
            }

            var conversation = await _conversationService.GetOrCreatePrivateConversationAsync(currentUserId, command.OtherUserId, cancellationToken);
            var response = await MapToResponseAsync(conversation, currentUserId, cancellationToken);
            
            return Result<ChatRoomResponse>.Success(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating private conversation: {Message}", ex.Message);
            return Result<ChatRoomResponse>.Failure(ex.Message, ErrorCodeEnum.InvalidInput);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business logic error while creating private conversation: {Message}", ex.Message);
            return Result<ChatRoomResponse>.Failure(ex.Message, ErrorCodeEnum.InvalidOperation);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while creating private conversation");
            return Result<ChatRoomResponse>.Failure("Không tạo được cuộc trò chuyện do lỗi cơ sở dữ liệu", ErrorCodeEnum.DatabaseError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating private conversation");
            return Result<ChatRoomResponse>.Failure("Đã xảy ra lỗi không mong muốn khi tạo cuộc trò chuyện", ErrorCodeEnum.InternalError);
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
            CreatedAt = conversation.CreatedAt ?? CapstoneProject.Domain.Common.VietnamDateTime.DbNow
        };
    }
}




using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Commons.DTOs.Chat;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Chat.Commands.AddMemberToRoom;

/// <summary>
/// Handler for adding a participant to a temporary group conversation.
/// Uses ConversationService to handle the logic.
/// Note: User joined notifications are handled by SignalR Hub when client calls JoinConversation.
/// </summary>
public class AddMemberToRoomCommandHandler : IRequestHandler<AddMemberToRoomCommand, Result<ChatRoomMemberResponse>>
{
    private readonly IConversationService _conversationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAvatarUrlResolverService _avatarUrlResolver;
    private readonly ILogger<AddMemberToRoomCommandHandler> _logger;

    public AddMemberToRoomCommandHandler(
        IConversationService conversationService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IAvatarUrlResolverService avatarUrlResolver,
        ILogger<AddMemberToRoomCommandHandler> logger)
    {
        _conversationService = conversationService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _avatarUrlResolver = avatarUrlResolver;
        _logger = logger;
    }

    public async Task<Result<ChatRoomMemberResponse>> Handle(AddMemberToRoomCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var userIdString = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
            {
                return Result<ChatRoomMemberResponse>.Failure("Người dùng chưa được xác thực", ErrorCodeEnum.Unauthorized);
            }

            if (command == null)
            {
                return Result<ChatRoomMemberResponse>.Failure("Lệnh không thể rỗng", ErrorCodeEnum.InvalidInput);
            }

            if (command.ChatRoomId == Guid.Empty)
            {
                return Result<ChatRoomMemberResponse>.Failure("Cần có ID phòng trò chuyện", ErrorCodeEnum.InvalidInput);
            }

            if (command.UserId == Guid.Empty)
            {
                return Result<ChatRoomMemberResponse>.Failure("ID người dùng là bắt buộc", ErrorCodeEnum.InvalidInput);
            }

            var member = await _conversationService.AddParticipantAsync(command.ChatRoomId, command.UserId, cancellationToken);
            
            // Load member with user info for response
            var memberRepo = _unitOfWork.Repository<Domain.Entities.ChatRoomMember>();
            var memberWithUser = await memberRepo.GetQueryable()
                .Where(m => m.Id == member.Id)
                .Include(m => m.User)
                .FirstOrDefaultAsync(cancellationToken);

            if (memberWithUser == null)
            {
                _logger.LogError("Failed to retrieve member {MemberId} after adding to conversation {ConversationId}", member.Id, command.ChatRoomId);
                return Result<ChatRoomMemberResponse>.Failure("Không lấy được thông tin thành viên", ErrorCodeEnum.InternalError);
            }

            var response = new ChatRoomMemberResponse
            {
                Id = memberWithUser.Id,
                UserId = memberWithUser.UserId,
                UserName = $"{memberWithUser.User.FirstName} {memberWithUser.User.LastName}".Trim(),
                AvatarPath = _avatarUrlResolver.ResolveAvatarUrl(memberWithUser.User.AvatarPath),
                JoinedAt = memberWithUser.JoinedAt,
                LeftAt = memberWithUser.LeftAt,
                LastReadAt = memberWithUser.LastReadAt
            };

            return Result<ChatRoomMemberResponse>.Success(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while adding member to room: {Message}", ex.Message);
            return Result<ChatRoomMemberResponse>.Failure(ex.Message, ErrorCodeEnum.InvalidInput);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business logic error while adding member to room: {Message}", ex.Message);
            return Result<ChatRoomMemberResponse>.Failure(ex.Message, ErrorCodeEnum.InvalidOperation);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while adding member {UserId} to conversation {ConversationId}", command?.UserId, command?.ChatRoomId);
            return Result<ChatRoomMemberResponse>.Failure("Không thể thêm thành viên do lỗi cơ sở dữ liệu", ErrorCodeEnum.DatabaseError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding member {UserId} to conversation {ConversationId}", command?.UserId, command?.ChatRoomId);
            return Result<ChatRoomMemberResponse>.Failure("Đã xảy ra lỗi không mong muốn khi thêm thành viên", ErrorCodeEnum.InternalError);
        }
    }
}

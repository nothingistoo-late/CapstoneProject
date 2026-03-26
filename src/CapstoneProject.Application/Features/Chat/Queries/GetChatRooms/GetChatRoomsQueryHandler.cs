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

namespace CapstoneProject.Application.Features.Chat.Queries.GetChatRooms;

public class GetChatRoomsQueryHandler : IRequestHandler<GetChatRoomsQuery, Result<PaginationResult<ChatRoomResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAvatarUrlResolverService _avatarUrlResolver;
    private readonly ILogger<GetChatRoomsQueryHandler> _logger;

    public GetChatRoomsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAvatarUrlResolverService avatarUrlResolver,
        ILogger<GetChatRoomsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _avatarUrlResolver = avatarUrlResolver;
        _logger = logger;
    }

    public async Task<Result<PaginationResult<ChatRoomResponse>>> Handle(GetChatRoomsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var userIdString = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
            {
                return Result<PaginationResult<ChatRoomResponse>>.Failure("User not authenticated", ErrorCodeEnum.Unauthorized);
            }

            if (query == null)
            {
                return Result<PaginationResult<ChatRoomResponse>>.Failure("Query cannot be null", ErrorCodeEnum.InvalidInput);
            }

            // Validate pagination parameters
            if (query.PageNumber < 1)
            {
                query.PageNumber = 1;
            }

            if (query.PageSize < 1 || query.PageSize > 100)
            {
                query.PageSize = Math.Clamp(query.PageSize, 1, 100);
            }

            var chatRoomRepo = _unitOfWork.Repository<ChatRoom>();
            var messageRepo = _unitOfWork.Repository<Message>();

            // Get chat rooms where user is a member (active participants only)
            var roomsQuery = chatRoomRepo.GetQueryable()
                .Where(r => !r.IsDeleted && r.Members.Any(m => m.UserId == currentUserId && m.LeftAt == null && m.Status == EntityStatusEnum.Active))
                .Include(r => r.Members)
                .ThenInclude(m => m.User)
                .OrderByDescending(r => r.LastMessageAt ?? r.CreatedAt);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            roomsQuery = roomsQuery
                .Where(r => r.Name != null && r.Name.Contains(query.SearchTerm))
                .OrderByDescending(r => r.LastMessageAt ?? r.CreatedAt);
        }

        var totalCount = await roomsQuery.CountAsync(cancellationToken);

        var rooms = await roomsQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var roomResponses = new List<ChatRoomResponse>();

            foreach (var room in rooms)
            {
                var member = room.Members.FirstOrDefault(m => m.UserId == currentUserId && m.LeftAt == null);
                var unreadCount = 0;

                if (member != null && member.LastReadAt.HasValue)
                {
                    unreadCount = await messageRepo.GetQueryable()
                        .Where(m => m.ChatRoomId == room.Id && !m.IsDeleted && (m.CreatedAt ?? DateTime.MinValue) > member.LastReadAt.Value)
                        .CountAsync(cancellationToken);
                }
                else if (member != null)
                {
                    unreadCount = await messageRepo.GetQueryable()
                        .Where(m => m.ChatRoomId == room.Id && !m.IsDeleted)
                        .CountAsync(cancellationToken);
                }

            var lastMessage = room.LastMessageId.HasValue
                ? await messageRepo.GetQueryable()
                    .Where(m => m.Id == room.LastMessageId.Value)
                    .Include(m => m.Sender)
                    .FirstOrDefaultAsync(cancellationToken)
                : null;

                roomResponses.Add(new ChatRoomResponse
                {
                    Id = room.Id,
                    Name = room.Name,
                    RoomType = room.RoomType,
                    IsClosed = room.IsClosed,
                    ClosedAt = room.ClosedAt,
                    ClosedBy = room.ClosedBy,
                    LastMessageId = room.LastMessageId,
                    LastMessageAt = room.LastMessageAt,
                    UnreadCount = unreadCount,
                    Members = room.Members.Where(m => m.LeftAt == null).Select(m => new ChatRoomMemberResponse
                    {
                        Id = m.Id,
                        UserId = m.UserId,
                        UserName = $"{m.User.FirstName} {m.User.LastName}".Trim(),
                        AvatarPath = _avatarUrlResolver.ResolveAvatarUrl(m.User.AvatarPath),
                        JoinedAt = m.JoinedAt,
                        LeftAt = m.LeftAt,
                        LastReadAt = m.LastReadAt
                    }).ToList(),
                    LastMessage = lastMessage != null ? new MessageResponse
                    {
                        Id = lastMessage.Id,
                        Content = lastMessage.Content,
                        MessageType = lastMessage.MessageType,
                        SenderName = $"{lastMessage.Sender.FirstName} {lastMessage.Sender.LastName}".Trim(),
                        SenderAvatar = _avatarUrlResolver.ResolveAvatarUrl(lastMessage.Sender.AvatarPath),
                        CreatedAt = lastMessage.CreatedAt ?? CapstoneProject.Domain.Common.VietnamDateTime.Now
                    } : null,
                    CreatedAt = room.CreatedAt ?? CapstoneProject.Domain.Common.VietnamDateTime.Now
                });
            }

            var result = PaginationResult<ChatRoomResponse>.Success(
                roomResponses,
                query.PageNumber,
                query.PageSize,
                totalCount);

            return Result<PaginationResult<ChatRoomResponse>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving chat rooms for user");
            return Result<PaginationResult<ChatRoomResponse>>.Failure("An unexpected error occurred while retrieving chat rooms", ErrorCodeEnum.InternalError);
        }
    }
}


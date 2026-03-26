using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Chat.Services;

public class ConversationService : IConversationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConversationService> _logger;

    public ConversationService(
        IUnitOfWork unitOfWork,
        ILogger<ConversationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ChatRoom> GetOrCreatePrivateConversationAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validation
            if (user1Id == Guid.Empty || user2Id == Guid.Empty)
            {
                throw new ArgumentException("User IDs cannot be empty");
            }

            if (user1Id == user2Id)
            {
                throw new InvalidOperationException("Cannot create a conversation with yourself");
            }

            var conversationRepo = _unitOfWork.Repository<ChatRoom>();
            var memberRepo = _unitOfWork.Repository<ChatRoomMember>();

            // Check if a private conversation already exists between these two users
            var existingConversation = await conversationRepo.GetQueryable()
                .Where(c => c.RoomType == ChatRoomTypeEnum.Private && !c.IsDeleted)
                .Where(c => c.Members.Any(m => m.UserId == user1Id && m.LeftAt == null))
                .Where(c => c.Members.Any(m => m.UserId == user2Id && m.LeftAt == null))
                .Where(c => c.Members.Count(m => m.LeftAt == null) == 2)
                .Include(c => c.Members)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingConversation != null)
            {
                _logger.LogInformation("Found existing private conversation {ConversationId} between users {User1Id} and {User2Id}", 
                    existingConversation.Id, user1Id, user2Id);
                return existingConversation;
            }

            // Create new private conversation
            var conversation = new ChatRoom
            {
                RoomType = ChatRoomTypeEnum.Private,
                Name = null, // No name for private chats
                CreatedBy = user1Id,
                CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                Status = EntityStatusEnum.Active,
                IsClosed = false
            };

            await conversationRepo.AddAsync(conversation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Add both participants
            var member1 = new ChatRoomMember
            {
                ChatRoomId = conversation.Id,
                UserId = user1Id,
                JoinedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                CreatedBy = user1Id,
                CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                Status = EntityStatusEnum.Active
            };

            var member2 = new ChatRoomMember
            {
                ChatRoomId = conversation.Id,
                UserId = user2Id,
                JoinedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                CreatedBy = user1Id,
                CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                Status = EntityStatusEnum.Active
            };

            await memberRepo.AddAsync(member1);
            await memberRepo.AddAsync(member2);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created new private conversation {ConversationId} between users {User1Id} and {User2Id}", 
                conversation.Id, user1Id, user2Id);

            return conversation;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while getting or creating private conversation between users {User1Id} and {User2Id}", user1Id, user2Id);
            throw new InvalidOperationException("Failed to create or retrieve private conversation due to database error", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting or creating private conversation between users {User1Id} and {User2Id}", user1Id, user2Id);
            throw;
        }
    }

    public async Task<ChatRoom> CreateTemporaryGroupConversationAsync(string name, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Group name cannot be empty or whitespace");
            }

            if (createdByUserId == Guid.Empty)
            {
                throw new ArgumentException("Created by user ID cannot be empty");
            }

            if (name.Length > 200)
            {
                throw new ArgumentException("Group name must not exceed 200 characters");
            }

            var conversationRepo = _unitOfWork.Repository<ChatRoom>();

            var conversation = new ChatRoom
            {
                RoomType = ChatRoomTypeEnum.TemporaryGroup,
                Name = name.Trim(),
                CreatedBy = createdByUserId,
                CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                Status = EntityStatusEnum.Active,
                IsClosed = false
            };

            await conversationRepo.AddAsync(conversation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Add creator as first participant
            var memberRepo = _unitOfWork.Repository<ChatRoomMember>();
            var member = new ChatRoomMember
            {
                ChatRoomId = conversation.Id,
                UserId = createdByUserId,
                JoinedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                CreatedBy = createdByUserId,
                CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                Status = EntityStatusEnum.Active
            };

            await memberRepo.AddAsync(member);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created temporary group conversation {ConversationId} with name {Name} by user {UserId}", 
                conversation.Id, name, createdByUserId);

            return conversation;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while creating temporary group conversation with name {Name} by user {UserId}", name, createdByUserId);
            throw new InvalidOperationException("Failed to create temporary group conversation due to database error", ex);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            _logger.LogWarning(ex, "Validation error while creating temporary group conversation: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating temporary group conversation with name {Name} by user {UserId}", name, createdByUserId);
            throw;
        }
    }

    public async Task<ChatRoomMember> AddParticipantAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validation
            if (conversationId == Guid.Empty)
            {
                throw new ArgumentException("Conversation ID cannot be empty");
            }

            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty");
            }

            var conversation = await GetConversationAsync(conversationId, cancellationToken);
            if (conversation == null)
            {
                throw new InvalidOperationException($"Conversation {conversationId} not found");
            }

            if (!conversation.CanJoin())
            {
                throw new InvalidOperationException($"Conversation {conversationId} is closed and cannot accept new participants");
            }

            // Only temporary group conversations allow adding members
            if (conversation.RoomType != ChatRoomTypeEnum.TemporaryGroup)
            {
                throw new InvalidOperationException("Only temporary group conversations allow adding members dynamically");
            }

            // Check if user is already a participant
            var memberRepo = _unitOfWork.Repository<ChatRoomMember>();
            var existingMember = await memberRepo.GetQueryable()
                .Where(m => m.ChatRoomId == conversationId && m.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken);
            
            if (existingMember != null)
            {
                if (existingMember.LeftAt == null)
                {
                    throw new InvalidOperationException($"User {userId} is already a participant in conversation {conversationId}");
                }
                
                // Reactivate if previously left
                existingMember.LeftAt = null;
                existingMember.JoinedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
                existingMember.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
                memberRepo.Update(existingMember);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Re-activated user {UserId} in conversation {ConversationId}", userId, conversationId);
                return existingMember;
            }

            // Verify user exists and is active
            var userRepo = _unitOfWork.Repository<AppUser>();
            var user = await userRepo.GetFirstOrDefaultAsync(u => u.Id == userId && u.Status == EntityStatusEnum.Active);
            if (user == null)
            {
                throw new InvalidOperationException($"User {userId} does not exist or is not active");
            }

            var member = new ChatRoomMember
            {
                ChatRoomId = conversationId,
                UserId = userId,
                JoinedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                CreatedBy = userId,
                CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                Status = EntityStatusEnum.Active
            };

            await memberRepo.AddAsync(member);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Added user {UserId} to conversation {ConversationId}", userId, conversationId);

            return member;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while adding participant {UserId} to conversation {ConversationId}", userId, conversationId);
            throw new InvalidOperationException("Failed to add participant due to database error", ex);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            _logger.LogWarning(ex, "Validation error while adding participant: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding participant {UserId} to conversation {ConversationId}", userId, conversationId);
            throw;
        }
    }

    public async Task RemoveParticipantAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validation
            if (conversationId == Guid.Empty)
            {
                throw new ArgumentException("Conversation ID cannot be empty");
            }

            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty");
            }

            var memberRepo = _unitOfWork.Repository<ChatRoomMember>();
            var member = await memberRepo.GetQueryable()
                .Where(m => m.ChatRoomId == conversationId && m.UserId == userId && m.LeftAt == null)
                .FirstOrDefaultAsync(cancellationToken);

            if (member == null)
            {
                throw new InvalidOperationException($"User {userId} is not an active participant in conversation {conversationId}");
            }

            member.Leave();
            memberRepo.Update(member);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Removed user {UserId} from conversation {ConversationId}", userId, conversationId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while removing participant {UserId} from conversation {ConversationId}", userId, conversationId);
            throw new InvalidOperationException("Failed to remove participant due to database error", ex);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            _logger.LogWarning(ex, "Validation error while removing participant: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while removing participant {UserId} from conversation {ConversationId}", userId, conversationId);
            throw;
        }
    }

    public async Task CloseConversationAsync(Guid conversationId, Guid closedByUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validation
            if (conversationId == Guid.Empty)
            {
                throw new ArgumentException("Conversation ID cannot be empty");
            }

            if (closedByUserId == Guid.Empty)
            {
                throw new ArgumentException("Closed by user ID cannot be empty");
            }

            var conversation = await GetConversationAsync(conversationId, cancellationToken);
            if (conversation == null)
            {
                throw new InvalidOperationException($"Conversation {conversationId} not found");
            }

            if (conversation.RoomType != ChatRoomTypeEnum.TemporaryGroup)
            {
                throw new InvalidOperationException("Only temporary group conversations can be closed");
            }

            if (conversation.IsClosed)
            {
                throw new InvalidOperationException($"Conversation {conversationId} is already closed");
            }

            // Verify user closing the conversation is a participant
            var isParticipant = await IsParticipantAsync(conversationId, closedByUserId, cancellationToken);
            if (!isParticipant)
            {
                throw new InvalidOperationException($"User {closedByUserId} is not a participant in conversation {conversationId} and cannot close it");
            }

            conversation.Close(closedByUserId);
            var conversationRepo = _unitOfWork.Repository<ChatRoom>();
            conversationRepo.Update(conversation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Closed conversation {ConversationId} by user {UserId}", conversationId, closedByUserId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while closing conversation {ConversationId} by user {UserId}", conversationId, closedByUserId);
            throw new InvalidOperationException("Failed to close conversation due to database error", ex);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            _logger.LogWarning(ex, "Validation error while closing conversation: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while closing conversation {ConversationId} by user {UserId}", conversationId, closedByUserId);
            throw;
        }
    }

    public async Task<bool> IsParticipantAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (conversationId == Guid.Empty || userId == Guid.Empty)
            {
                return false;
            }

            var memberRepo = _unitOfWork.Repository<ChatRoomMember>();
            return await memberRepo.AnyAsync(m => 
                m.ChatRoomId == conversationId && 
                m.UserId == userId && 
                m.LeftAt == null &&
                m.Status == EntityStatusEnum.Active);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} is participant in conversation {ConversationId}", userId, conversationId);
            return false;
        }
    }

    public async Task<ChatRoom?> GetConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (conversationId == Guid.Empty)
            {
                return null;
            }

            var conversationRepo = _unitOfWork.Repository<ChatRoom>();
            return await conversationRepo.GetFirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted, 
                c => c.Members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation {ConversationId}", conversationId);
            throw new InvalidOperationException($"Failed to retrieve conversation {conversationId}", ex);
        }
    }
}




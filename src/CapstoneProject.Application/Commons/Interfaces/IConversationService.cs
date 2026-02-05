using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Commons.Interfaces;

/// <summary>
/// Service for managing conversations (private chats and temporary group chats).
/// </summary>
public interface IConversationService
{
    /// <summary>
    /// Get or create a private conversation between two users.
    /// Returns existing conversation if one already exists, otherwise creates a new one.
    /// </summary>
    Task<ChatRoom> GetOrCreatePrivateConversationAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new temporary competition group chat.
    /// </summary>
    Task<ChatRoom> CreateTemporaryGroupConversationAsync(string name, Guid createdByUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a participant to a conversation (for temporary group chats).
    /// Validates that the conversation is not closed and user is not already a participant.
    /// </summary>
    Task<ChatRoomMember> AddParticipantAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a participant from a conversation (for temporary group chats).
    /// </summary>
    Task RemoveParticipantAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Close a temporary group conversation.
    /// Prevents new messages and new participants.
    /// </summary>
    Task CloseConversationAsync(Guid conversationId, Guid closedByUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a user is a participant in a conversation.
    /// </summary>
    Task<bool> IsParticipantAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a conversation by ID.
    /// </summary>
    Task<ChatRoom?> GetConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);
}

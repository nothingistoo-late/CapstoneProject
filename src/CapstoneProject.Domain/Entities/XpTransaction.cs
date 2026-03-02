using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Giao dịch XP (cộng/trừ) cho người dùng - gắn với map hoặc hành động.
/// </summary>
public class XpTransaction : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? MapId { get; set; }
    public int Delta { get; set; }
    public string? Reason { get; set; }
}

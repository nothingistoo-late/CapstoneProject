using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Giao dịch XP (cộng/trừ) cho người dùng - gắn với game hoặc hành động.
/// </summary>
public class XpTransaction : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? GameId { get; set; }
    public Guid? SourceId { get; set; }
    public XpSourceTypeEnum SourceType { get; set; } = XpSourceTypeEnum.Unknown;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string? Metadata { get; set; }
    public int Delta { get; set; }
    public string? Reason { get; set; }
}

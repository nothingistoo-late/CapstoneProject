using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Gói mà user đã mua: còn lại (remaining), trạng thái.
/// </summary>
public class UserPackage : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid PackageId { get; set; }
    public int Remaining { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public virtual Package Package { get; set; } = null!;
}

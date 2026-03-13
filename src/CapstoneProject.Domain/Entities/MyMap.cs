using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Bảng lưu các map mà user sở hữu (tự tạo, mua, hoặc thêm map free vào bộ sưu tập).
/// </summary>
public class MyMap : BaseEntity
{
    public Guid MapId { get; set; }
    public Guid UserId { get; set; }
    /// <summary>true = user tự tạo map; false = mua hoặc thêm map free.</summary>
    public bool IsAuthor { get; set; }

    public virtual Map Map { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
}

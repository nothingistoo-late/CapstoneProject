using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Bảng lưu các game mà user sở hữu (tự tạo, mua, hoặc thêm game free vào bộ sưu tập).
/// </summary>
public class MyGame : BaseEntity
{
    public Guid GameId { get; set; }
    public Guid UserId { get; set; }
    /// <summary>true = user tự tạo game; false = mua hoặc thêm game free.</summary>
    public bool IsAuthor { get; set; }

    public virtual Game Game { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
}

using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>Ảnh / video mô tả game (gallery), lưu URL Cloudinary.</summary>
public class GameMedia : BaseEntity
{
    public Guid GameId { get; set; }
    /// <summary>URL đầy đủ (secure) từ Cloudinary.</summary>
    public string Url { get; set; } = string.Empty;
    public GameMediaKind Kind { get; set; }
    /// <summary>Thứ tự hiển thị trong gallery.</summary>
    public int SortOrder { get; set; }

    public virtual Game Game { get; set; } = null!;
}

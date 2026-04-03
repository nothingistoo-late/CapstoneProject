using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>Ảnh / video mô tả map (gallery), lưu URL Cloudinary.</summary>
public class MapMedia : BaseEntity
{
    public Guid MapId { get; set; }
    /// <summary>URL đầy đủ (secure) từ Cloudinary.</summary>
    public string Url { get; set; } = string.Empty;
    public MapMediaKind Kind { get; set; }
    /// <summary>Thứ tự hiển thị trong gallery.</summary>
    public int SortOrder { get; set; }

    public virtual Map Map { get; set; } = null!;
}

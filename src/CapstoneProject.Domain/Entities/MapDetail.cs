using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Lưu JSON map cho mỗi map (1-1 với Map).
/// </summary>
public class MapDetail : BaseEntity
{
    public Guid MapId { get; set; }

    /// <summary>Nội dung JSON của map.</summary>
    public string JsonContent { get; set; } = string.Empty;

    public virtual Map Map { get; set; } = null!;
}


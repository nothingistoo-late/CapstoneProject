using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Ràng buộc của map (ví dụ: số khối tối đa, bắt buộc dùng vòng lặp). Type + Payload (JSON).
/// </summary>
public class MapConstraint : BaseEntity
{
    public Guid MapId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;

    public virtual Map Map { get; set; } = null!;
}

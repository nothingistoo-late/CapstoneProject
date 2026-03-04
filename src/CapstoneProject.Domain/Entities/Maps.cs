using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Entity lưu trữ nội dung map/level dạng JSON (platform challenge, layers, metadata...).
/// Một bản ghi tương ứng một file JSON level (id, name, width, height, layers, startPosition, goalPosition, metadata).
/// </summary>
public class Maps : BaseEntity
{
    /// <summary>Mã định danh từ file JSON (vd: platform-01).</summary>
    public string? ExternalId { get; set; }
    /// <summary>Tên map từ JSON (vd: Platform Challenge).</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Toàn bộ nội dung file JSON (layers, startPosition, goalPosition, metadata...).</summary>
    public string JsonContent { get; set; } = string.Empty;
}

using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Lớp gợi ý (layered hint) cho map - hiển thị khi người chơi thất bại.
/// </summary>
public class Hint : BaseEntity
{
    public Guid MapDetailId { get; set; }
    public int OrderNo { get; set; }
    public string Content { get; set; } = string.Empty;

    public virtual MapDetail MapDetail { get; set; } = null!;
}

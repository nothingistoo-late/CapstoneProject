using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Lớp gợi ý (layered hint) cho game - hiển thị khi người chơi thất bại.
/// </summary>
public class Hint : BaseEntity
{
    public Guid GameDetailId { get; set; }
    public int OrderNo { get; set; }
    public string Content { get; set; } = string.Empty;

    public virtual GameDetail GameDetail { get; set; } = null!;
}

using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Maps;

public class MapListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public int TimeLimitMs { get; set; }
    public bool IsPublished { get; set; }
    public MapStatusEnum MapStatus { get; set; }
    public decimal? Price { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<string> TagNames { get; set; } = new();
    public int WinCondition { get; set; }
}

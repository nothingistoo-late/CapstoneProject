using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Challenge;

public class MapDetailDto
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
    public string? EditorialContent { get; set; }
    public int UnlockEditorialAfterStars { get; set; }
    public DateTime? CreatedAt { get; set; }
    public MapSpecDto? ActiveSpec { get; set; }
    public List<HintItemDto> Hints { get; set; } = new();
    public List<ConstraintItemDto> Constraints { get; set; } = new();
    public List<string> TagNames { get; set; } = new();
    public List<string> ConceptNames { get; set; } = new();
}

public class MapSpecDto
{
    public Guid Id { get; set; }
    public string GridSpec { get; set; } = string.Empty;
    public string InitialStateSpec { get; set; } = string.Empty;
    public string WinConditionSpec { get; set; } = string.Empty;
    public string FailConditionSpec { get; set; } = string.Empty;
    public int Version { get; set; }
}

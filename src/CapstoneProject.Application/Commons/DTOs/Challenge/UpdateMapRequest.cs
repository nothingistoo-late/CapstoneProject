namespace CapstoneProject.Application.Commons.DTOs.Challenge;

public class UpdateMapRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public int TimeLimitMs { get; set; }
    public decimal? Price { get; set; }
    public string? GridSpec { get; set; }
    public string? InitialStateSpec { get; set; }
    public string? WinConditionSpec { get; set; }
    public string? FailConditionSpec { get; set; }
    public string? EditorialContent { get; set; }
    public int? UnlockEditorialAfterStars { get; set; }
    public List<HintItemDto>? Hints { get; set; }
    public List<ConstraintItemDto>? Constraints { get; set; }
    public List<Guid>? TagIds { get; set; }
    public List<Guid>? ConceptIds { get; set; }
}

using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Challenge;

public class CreateMapRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public int TimeLimitMs { get; set; }
    public decimal? Price { get; set; }
    public string GridSpec { get; set; } = string.Empty;
    public string InitialStateSpec { get; set; } = string.Empty;
    public string WinConditionSpec { get; set; } = string.Empty;
    public string FailConditionSpec { get; set; } = string.Empty;
    public List<HintItemDto> Hints { get; set; } = new();
    public List<ConstraintItemDto> Constraints { get; set; } = new();
    public List<Guid> TagIds { get; set; } = new();
    public List<Guid> ConceptIds { get; set; } = new();
}

public class HintItemDto
{
    public int OrderNo { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class ConstraintItemDto
{
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
}

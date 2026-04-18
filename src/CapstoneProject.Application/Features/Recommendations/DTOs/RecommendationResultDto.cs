namespace CapstoneProject.Application.Features.Recommendations.DTOs;

public class RecommendationResultDto
{
    public List<RecommendationMapDto> RecommendedMaps { get; set; } = new();
    public List<RecommendationMapDto> ReviewMaps { get; set; } = new();
    public List<RecommendationMapDto> SuggestedPracticeMaps { get; set; } = new();
    public RecommendationConceptDto? NextConcept { get; set; }
}

public class RecommendationMapDto
{
    public Guid GameId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Difficulty { get; set; }

    public Guid? ConceptId { get; set; }
    public string? ConceptName { get; set; }

    public double? Score { get; set; }

    public int Attempts { get; set; }
    public int FailCount { get; set; }
    public double SuccessRate { get; set; }
}

public class RecommendationConceptDto
{
    public Guid ConceptId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}


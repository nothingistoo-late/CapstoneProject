namespace CapstoneProject.Application.Commons.DTOs.Games;

public class GameReviewCriterionDto
{
    public Guid Id { get; set; }
    public string CriterionKey { get; set; } = string.Empty;
    public string SectionKey { get; set; } = string.Empty;
    public string SectionTitle { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; }
}

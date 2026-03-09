namespace CapstoneProject.API.Models;

/// <summary>
/// Shared request for creating a map from an uploaded JSON file. Used by both Learner and CMS map endpoints.
/// </summary>
public class CreateMapFromJsonFileRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public int TimeLimitMs { get; set; }
    public int WinCondition { get; set; }
    public decimal? Price { get; set; }
    public string HintsJson { get; set; } = "[]";
    public string TagIdsCsv { get; set; } = string.Empty;
    public IFormFile MapDetailFile { get; set; } = null!;
}

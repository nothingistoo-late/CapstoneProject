using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Maps;

/// <summary>
/// Input for CreateMapFromJsonFileCommand. MapDetailJsonContent is the raw JSON string from the uploaded file.
/// </summary>
public class CreateMapFromJsonFileInput
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    /// <summary>Loại map: Topdown hoặc Platform. Mặc định Topdown.</summary>
    public MapTypeEnum? Type { get; set; }
    public int TimeLimitMs { get; set; }
    public int WinCondition { get; set; }
    public decimal? Price { get; set; }
    public string HintsJson { get; set; } = "[]";
    public string TagIdsCsv { get; set; } = string.Empty;
    public string LearnedTagsCsv { get; set; } = string.Empty;
    public string MapDetailJsonContent { get; set; } = string.Empty;
}

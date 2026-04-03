namespace CapstoneProject.Application.Commons.DTOs.Maps;

/// <summary>
/// Input for CreateMapFromJsonFileCommand. MapDetailJsonContent is the raw JSON string from the uploaded file.
/// </summary>
public class CreateMapFromJsonFileInput
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public decimal? Price { get; set; }
    /// <summary>Số lượt chơi thử miễn phí cho mỗi người chơi. Null/0 = không có trial.</summary>
    public int? FreeTrialAttemptLimit { get; set; }
    public string TagIdsCsv { get; set; } = string.Empty;
    public string LearnedTagsCsv { get; set; } = string.Empty;
    /// <summary>Nội dung một file JSON (map đơn hoặc nhiều level trong cùng file).</summary>
    public string MapDetailJsonContent { get; set; } = string.Empty;
    /// <summary>Mỗi phần tử = JSON một level; order = index. Nếu có thì bỏ qua <see cref="MapDetailJsonContent"/>.</summary>
    public List<string>? MapDetailJsonContents { get; set; }
}

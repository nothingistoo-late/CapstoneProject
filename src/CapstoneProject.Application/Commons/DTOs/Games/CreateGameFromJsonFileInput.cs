namespace CapstoneProject.Application.Commons.DTOs.Games;

/// <summary>
/// Input for CreateMapFromJsonFileCommand. GameDetailJsonContent is the raw JSON string from the uploaded file.
/// </summary>
public class CreateMapFromJsonFileInput
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public decimal? Price { get; set; }
    /// <summary>Loại game mặc định cho level nếu JSON không tự khai báo. Hỗ trợ Topdown | Platform | Snake.</summary>
    public string? Type { get; set; }
    /// <summary>Số lượt chơi thử miễn phí cho mỗi người chơi. Null/0 = không có trial.</summary>
    public int? FreeTrialAttemptLimit { get; set; }
    public string TagIdsCsv { get; set; } = string.Empty;
    public string LearnedTagsCsv { get; set; } = string.Empty;
    /// <summary>Nội dung một file JSON (game đơn hoặc nhiều level trong cùng file).</summary>
    public string GameDetailJsonContent { get; set; } = string.Empty;
    /// <summary>Mỗi phần tử = JSON một level; order = index. Nếu có thì bỏ qua <see cref="GameDetailJsonContent"/>.</summary>
    public List<string>? GameDetailJsonContents { get; set; }
}

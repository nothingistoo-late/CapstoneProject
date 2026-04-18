using System.Text.Json;

namespace CapstoneProject.Application.Commons.DTOs.Games;

/// <summary>
/// Request tạo nhiều bản ghi Games. Chấp nhận Levels (danh sách object) hoặc JsonContents (danh sách string).
/// </summary>
public class BatchCreateMapsRequest
{
    /// <summary>Danh sách level dạng object (không cần escape newline). Ưu tiên dùng nếu gửi cả hai.</summary>
    public List<JsonElement>? Levels { get; set; }

    /// <summary>Danh sách nội dung JSON dạng string (mỗi phần tử = một file level, phải escape đúng).</summary>
    public List<string>? JsonContents { get; set; }
}

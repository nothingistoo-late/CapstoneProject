using System.Text.Json;

namespace CapstoneProject.Application.Commons.DTOs.Maps;

/// <summary>
/// Request tạo một bản ghi Maps từ nội dung JSON (file level/platform).
/// Chấp nhận một trong hai: Level (object) hoặc JsonContent (string đã escape).
/// </summary>
public class CreateMapsRequest
{
    /// <summary>Level dạng object (gửi nested JSON, không cần escape newline). Ưu tiên dùng nếu gửi cả hai.</summary>
    public JsonElement? Level { get; set; }

    /// <summary>Toàn bộ nội dung JSON của level dạng string (phải escape đúng, ví dụ \\n cho xuống dòng).</summary>
    public string? JsonContent { get; set; }
}

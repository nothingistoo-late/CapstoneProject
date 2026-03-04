using System.Text.Json;

namespace CapstoneProject.Application.Commons.DTOs.Maps;

/// <summary>
/// Request tạo một level: gửi full level qua JSON body (Level) hoặc qua file upload (LevelJson).
/// </summary>
public class CreateMapsRequest
{
    /// <summary>Level dạng object (id, name, width, height, layers, startPosition, goalPosition, metadata...). Dùng khi gửi JSON body.</summary>
    public JsonElement? Level { get; set; }

    /// <summary>Nội dung JSON level dạng string (dùng khi upload file: backend gán từ file). Chỉ cần một trong hai: Level hoặc LevelJson.</summary>
    public string? LevelJson { get; set; }

    /// <summary>Tên level. Nếu không gửi sẽ lấy từ level.name.</summary>
    public string? Name { get; set; }

    /// <summary>Loại: platform | topdown. Nếu không gửi sẽ lấy từ level.type hoặc level.metadata.</summary>
    public string? Type { get; set; }

    /// <summary>Độ khó: easy | medium | hard. Nếu không gửi sẽ lấy từ level.metadata.difficulty.</summary>
    public string? Difficulty { get; set; }
}

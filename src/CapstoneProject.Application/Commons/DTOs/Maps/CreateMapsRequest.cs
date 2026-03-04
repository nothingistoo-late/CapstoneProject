using System.Text.Json;

namespace CapstoneProject.Application.Commons.DTOs.Maps;

/// <summary>
/// Request tạo một level: gửi full level (object) + optional name, type, difficulty để override.
/// </summary>
public class CreateMapsRequest
{
    /// <summary>Level dạng object (id, name, width, height, layers, startPosition, goalPosition, metadata...). Bắt buộc.</summary>
    public JsonElement? Level { get; set; }

    /// <summary>Tên level. Nếu không gửi sẽ lấy từ level.name.</summary>
    public string? Name { get; set; }

    /// <summary>Loại: platform | topdown. Nếu không gửi sẽ lấy từ level.type hoặc level.metadata.</summary>
    public string? Type { get; set; }

    /// <summary>Độ khó: easy | medium | hard. Nếu không gửi sẽ lấy từ level.metadata.difficulty.</summary>
    public string? Difficulty { get; set; }
}

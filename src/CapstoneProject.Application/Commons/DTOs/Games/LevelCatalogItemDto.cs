namespace CapstoneProject.Application.Commons.DTOs.Games;

/// <summary>
/// Một mục catalog level (id, file, name, type, difficulty) – dùng cho batch upsert catalog.
/// </summary>
public class LevelCatalogItemDto
{
    public string Id { get; set; } = string.Empty;
    public string? File { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Difficulty { get; set; }
}

namespace CapstoneProject.Application.Commons.DTOs.Maps;

/// <summary>
/// Request đồng bộ catalog levels từ FE: danh sách { id, file, name, type, difficulty }. Tạo mới hoặc cập nhật theo ExternalId (id).
/// </summary>
public class BatchUpsertCatalogRequest
{
    public List<LevelCatalogItemDto> Levels { get; set; } = new();
}

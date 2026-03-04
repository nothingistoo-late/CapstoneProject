namespace CapstoneProject.Application.Commons.DTOs.Maps;

public class MapsListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Difficulty { get; set; }
    public DateTime? CreatedAt { get; set; }
}

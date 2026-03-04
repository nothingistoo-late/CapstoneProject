namespace CapstoneProject.Application.Commons.DTOs.Maps;

public class MapsListItemDto
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}

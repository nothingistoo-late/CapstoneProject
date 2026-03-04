namespace CapstoneProject.Application.Commons.DTOs.Maps;

public class MapsResponseDto
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string JsonContent { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

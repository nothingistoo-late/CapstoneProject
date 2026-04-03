namespace CapstoneProject.Application.Commons.DTOs.Maps;

public class MapMediaItemDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    /// <summary>Image hoặc Video.</summary>
    public string Kind { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

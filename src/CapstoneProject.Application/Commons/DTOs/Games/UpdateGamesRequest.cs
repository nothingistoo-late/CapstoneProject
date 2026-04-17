namespace CapstoneProject.Application.Commons.DTOs.Games;

public class UpdateMapsRequest
{
    public string? Name { get; set; }
    public string? File { get; set; }
    public string? Type { get; set; }
    public string? Difficulty { get; set; }
    /// <summary>Nội dung JSON đầy đủ (nếu gửi sẽ ghi đè).</summary>
    public string? JsonContent { get; set; }
}

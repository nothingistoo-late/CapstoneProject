namespace CapstoneProject.Application.Commons.DTOs.Maps;

public class UpdateMapsRequest
{
    public string? Name { get; set; }
    /// <summary>Nội dung JSON đầy đủ (nếu gửi sẽ ghi đè).</summary>
    public string? JsonContent { get; set; }
}

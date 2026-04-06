using System.ComponentModel.DataAnnotations;

namespace CapstoneProject.Application.Commons.DTOs.Complaints;

public class UpdateComplaintCategoryConfigRequest
{
    [Required]
    [MaxLength(100)]
    public string CategoryKey { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsEnabled { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

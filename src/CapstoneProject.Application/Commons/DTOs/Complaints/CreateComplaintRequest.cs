using System.ComponentModel.DataAnnotations;

namespace CapstoneProject.Application.Commons.DTOs.Complaints;

public class CreateComplaintRequest
{
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;
}


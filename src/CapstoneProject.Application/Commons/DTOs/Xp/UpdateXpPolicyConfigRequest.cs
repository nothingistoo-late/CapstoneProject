using System.ComponentModel.DataAnnotations;

namespace CapstoneProject.Application.Commons.DTOs.Xp;

public class UpdateXpPolicyConfigRequest
{
    [Required]
    [MaxLength(100)]
    public string PolicyKey { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; } = 0;
    public string? ConfigJson { get; set; }
}


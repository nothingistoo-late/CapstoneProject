using System.ComponentModel.DataAnnotations;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Xp;

public class UpdateXpSourceConfigRequest
{
    [Required]
    public XpSourceTypeEnum SourceType { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int BaseXp { get; set; } = 0;
    public int DailyCap { get; set; } = 0;
    public double BonusMultiplier { get; set; } = 1.0;
    public string? ConfigJson { get; set; }
}


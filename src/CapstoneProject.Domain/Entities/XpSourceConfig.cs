using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

public class XpSourceConfig : BaseEntity
{
    public XpSourceTypeEnum SourceType { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int BaseXp { get; set; } = 0;
    public int DailyCap { get; set; } = 0;
    public double BonusMultiplier { get; set; } = 1.0;
    public string? ConfigJson { get; set; }
}


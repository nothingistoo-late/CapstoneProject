using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

public class LevelThreshold : BaseEntity
{
    public int Level { get; set; }
    public int RequiredTotalXp { get; set; }
    public string? Title { get; set; }
}


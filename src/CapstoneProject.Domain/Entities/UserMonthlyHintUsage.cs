using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

public class UserMonthlyHintUsage : BaseEntity
{
    public Guid UserId { get; set; }
    public int MonthKey { get; set; }
    public int UsedCount { get; set; }

    public AppUser User { get; set; } = null!;
}

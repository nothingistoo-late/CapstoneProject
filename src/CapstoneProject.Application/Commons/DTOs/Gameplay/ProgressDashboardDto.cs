namespace CapstoneProject.Application.Commons.DTOs.Gameplay;

public class ProgressDashboardDto
{
    public int TotalXp { get; set; }
    public int MapsCompleted { get; set; }
    public int TotalStars { get; set; }
    public List<BadgeDto> Badges { get; set; } = new();
    public List<string> ConceptsPracticed { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class BadgeDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime UnlockedAt { get; set; }
}

public class RecentActivityDto
{
    public Guid MapId { get; set; }
    public string MapTitle { get; set; } = string.Empty;
    public int Stars { get; set; }
    public DateTime At { get; set; }
}

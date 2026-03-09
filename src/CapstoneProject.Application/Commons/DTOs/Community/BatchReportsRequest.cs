namespace CapstoneProject.Application.Commons.DTOs.Community;

/// <summary>
/// Request body for CMS batch resolve or dismiss reports.
/// </summary>
public class BatchReportsRequest
{
    public List<Guid> ReportIds { get; set; } = new();
    public string? ReviewNote { get; set; }
}

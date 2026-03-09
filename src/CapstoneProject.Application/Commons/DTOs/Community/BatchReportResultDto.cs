namespace CapstoneProject.Application.Commons.DTOs.Community;

public class BatchReportResultDto
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<Guid> NotFoundIds { get; set; } = new();
}

using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetComplaintCategoryConfigs;

public record GetComplaintCategoryConfigsQuery() : IRequest<Result<List<ComplaintCategoryConfigDto>>>;

public class ComplaintCategoryConfigDto
{
    public string CategoryKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public int SortOrder { get; set; }
    public List<string> RequiredAnyContextFields { get; set; } = new();
    public bool AllowManualContextInput { get; set; } = true;
}

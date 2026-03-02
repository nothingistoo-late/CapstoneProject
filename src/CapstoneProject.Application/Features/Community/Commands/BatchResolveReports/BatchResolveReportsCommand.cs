using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Community.Commands.BatchResolveReports;

public record BatchResolveReportsCommand(List<Guid> ReportIds, string? ReviewNote = null) : IRequest<Result<BatchReportResultDto>>;

public class BatchReportResultDto
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<Guid> NotFoundIds { get; set; } = new();
}

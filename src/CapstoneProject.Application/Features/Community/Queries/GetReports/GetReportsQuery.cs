using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Community.Queries.GetReports;

public record GetReportsQuery(
    ReportStatusFilter? Status = null,
    int PageNumber = 1,
    int PageSize = 20,
    Guid? GameId = null,
    Guid? UserId = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null) : IRequest<Result<PaginationResult<ReportListItemDto>>>;

public enum ReportStatusFilter
{
    All,
    Pending,
    Reviewed,
    Resolved,
    Dismissed
}

public class ReportListItemDto
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public string MapTitle { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string ReportStatus { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}

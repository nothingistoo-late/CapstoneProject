using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetMyComplaints;

public record GetMyComplaintsQuery(
    ComplaintStatusEnum? Status = null,
    int PageNumber = 1,
    int PageSize = 20,
    DateTime? DateFrom = null,
    DateTime? DateTo = null) : IRequest<Result<PaginationResult<MyComplaintListItemDto>>>;

public class MyComplaintListItemDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ComplaintStatus { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}


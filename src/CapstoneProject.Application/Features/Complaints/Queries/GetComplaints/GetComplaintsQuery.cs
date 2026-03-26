using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetComplaints;

public record GetComplaintsQuery(
    ComplaintStatusEnum? Status = null,
    int PageNumber = 1,
    int PageSize = 20,
    Guid? UserId = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    string? Keyword = null) : IRequest<Result<PaginationResult<ComplaintListItemDto>>>;

public class ComplaintListItemDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ComplaintStatus { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}


using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Models.Complaints;
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
    public string CategoryKey { get; set; } = string.Empty;
    public string ComplaintStatus { get; set; } = string.Empty;
    public string? ContextType { get; set; }
    public Guid? ContextId { get; set; }
    public string? ContextKey { get; set; }
    public string? ContextDataJson { get; set; }
    public DateTime? OccurredAt { get; set; }
    public ComplaintContextResolvedDto? ContextResolved { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool RefundProcessed { get; set; }
    public Guid? RefundedPaymentRecordId { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime? RefundedAt { get; set; }
}


using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Models.Complaints;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Commands.ChangeComplaintStatus;

public record ChangeComplaintStatusCommand(
    Guid ComplaintId,
    ComplaintStatusEnum ToStatus,
    string? Note = null,
    bool IssueRefund = false) : IRequest<Result<ComplaintStatusUpdateDto>>;

public class ComplaintStatusUpdateDto
{
    public Guid ComplaintId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryKey { get; set; } = string.Empty;
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? Note { get; set; }
    public bool IssueRefund { get; set; }
    public bool RefundProcessed { get; set; }
    public Guid? RefundedPaymentRecordId { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ContextType { get; set; }
    public Guid? ContextId { get; set; }
    public string? ContextKey { get; set; }
    public string? ContextDataJson { get; set; }
    public DateTime? OccurredAt { get; set; }
    public ComplaintContextResolvedDto? ContextResolved { get; set; }
}


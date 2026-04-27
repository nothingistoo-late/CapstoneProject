using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Complaints;
using CapstoneProject.Application.Commons.Models.Complaints;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetComplaintDetail;

public record GetComplaintDetailQuery(Guid ComplaintId) : IRequest<Result<ComplaintDetailDto>>;

public class ComplaintDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BuyerUserId { get; set; }
    /// <summary>Display name of the complainant (same user as <see cref="BuyerUserId"/>).</summary>
    public string BuyerDisplayName { get; set; } = string.Empty;
    public Guid? SellerUserId { get; set; }
    /// <summary>Display name of the game author / seller when applicable.</summary>
    public string? SellerDisplayName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
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
    public string? RefundReason { get; set; }

    public List<ComplaintMessageDto> Messages { get; set; } = new();
    public List<ComplaintStatusHistoryDto> StatusHistories { get; set; } = new();
}

public class ComplaintMessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string SenderDisplayName { get; set; } = string.Empty;
    public string SenderParty { get; set; } = "Other";
    public string Content { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<ComplaintAttachmentDto> Attachments { get; set; } = new();
}

public class ComplaintStatusHistoryDto
{
    public Guid Id { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public Guid ChangedBy { get; set; }
    public string ChangedByDisplayName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? Note { get; set; }
}


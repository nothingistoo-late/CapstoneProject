using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetComplaintDetail;

public record GetComplaintDetailQuery(Guid ComplaintId) : IRequest<Result<ComplaintDetailDto>>;

public class ComplaintDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ComplaintStatus { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public List<ComplaintMessageDto> Messages { get; set; } = new();
    public List<ComplaintStatusHistoryDto> StatusHistories { get; set; } = new();
}

public class ComplaintMessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class ComplaintStatusHistoryDto
{
    public Guid Id { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Note { get; set; }
}


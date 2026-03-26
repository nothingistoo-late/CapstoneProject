using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetMyComplaintDetail;

public record GetMyComplaintDetailQuery(Guid ComplaintId) : IRequest<Result<MyComplaintDetailDto>>;

public class MyComplaintDetailDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ComplaintStatus { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public List<MyComplaintMessageDto> Messages { get; set; } = new();
    public List<MyComplaintStatusHistoryDto> StatusHistories { get; set; } = new();
}

public class MyComplaintMessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class MyComplaintStatusHistoryDto
{
    public Guid Id { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Note { get; set; }
}


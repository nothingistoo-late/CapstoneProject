using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Xp.Queries.GetMyXpHistory;

public record GetMyXpHistoryQuery(
    int PageNumber = 1,
    int PageSize = 20,
    XpSourceTypeEnum? SourceType = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null) : IRequest<Result<PaginationResult<XpHistoryItemDto>>>;

public class XpHistoryItemDto
{
    public Guid Id { get; set; }
    public int Delta { get; set; }
    public string? Reason { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public Guid? SourceId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string? Metadata { get; set; }
    public DateTime? CreatedAt { get; set; }
}


using CapstoneProject.Application.Commons.DTOs.Challenge;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Challenge.Queries.GetMaps;

public class GetMapsQuery : IRequest<Result<PaginationResult<MapListItemDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? Difficulty { get; set; }
    public Guid? ConceptId { get; set; }
    public Guid? TagId { get; set; }
    /// <summary>When true, only published maps (catalog). When false/null, includes draft/pending (for admin/author).</summary>
    public bool? PublishedOnly { get; set; } = true;
    /// <summary>Filter by map status (Draft, PendingReview, Approved, Rejected, Published).</summary>
    public MapStatusEnum? MapStatus { get; set; }
    /// <summary>Search in title and description.</summary>
    public string? Search { get; set; }
    /// <summary>Filter by creator (for "my maps").</summary>
    public Guid? CreatedByUserId { get; set; }
    /// <summary>Sort by: CreatedAt, Title, Difficulty, TimeLimitMs.</summary>
    public string? SortBy { get; set; }
    public bool SortAscending { get; set; }
}

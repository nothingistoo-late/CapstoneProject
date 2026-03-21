using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Queries.GetMaps;

public class GetMapsQuery : IRequest<Result<PaginationResult<MapListItemDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    /// <summary>
    /// When 1–3: difficulty <b>tier</b> (learner + CMS): 1 = Easy (DB 1), 2 = Medium (DB 2 or 4), 3 = Hard (DB 3 or 5).
    /// Any other value: exact match on <c>Difficulty</c>.
    /// </summary>
    public int? Difficulty { get; set; }
    public Guid? TagId { get; set; }
    /// <summary>Filter by map type: 0 = Topdown, 1 = Platform. Null = all.</summary>
    public MapTypeEnum? Type { get; set; }
    public bool? PublishedOnly { get; set; } = true;
    public MapStatusEnum? MapStatus { get; set; }
    public string? Search { get; set; }
    /// <summary>Filter by creator (user) ID. Admin/CMS: lọc map theo user tạo.</summary>
    public Guid? CreatedByUserId { get; set; }
    /// <summary>Filter maps with Price >= MinPrice (null/0 = free).</summary>
    public decimal? MinPrice { get; set; }
    /// <summary>Filter maps with Price <= MaxPrice.</summary>
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }
    public bool SortAscending { get; set; }
}

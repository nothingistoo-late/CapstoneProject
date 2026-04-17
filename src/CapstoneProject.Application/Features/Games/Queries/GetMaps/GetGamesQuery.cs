using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Queries.GetMaps;

public class GetMapsQuery : IRequest<Result<PaginationResult<MapListItemDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    /// <summary>
    /// Difficulty level on 5-point scale (1..5). Filters by exact value.
    /// </summary>
    public int? Difficulty { get; set; }
    public Guid? TagId { get; set; }
    /// <summary>Filter by game type: 0 = Topdown, 1 = Platform, 2 = Snake. Null = all.</summary>
    public GameTypeEnum? Type { get; set; }
    public bool? PublishedOnly { get; set; } = true;
    public GameStatusEnum? GameStatus { get; set; }
    public string? Search { get; set; }
    /// <summary>Filter by creator (user) ID. Admin/CMS: lọc game theo user tạo.</summary>
    public Guid? CreatedByUserId { get; set; }
    /// <summary>Filter games with Price >= MinPrice (null/0 = free).</summary>
    public decimal? MinPrice { get; set; }
    /// <summary>Filter games with Price <= MaxPrice.</summary>
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }
    public bool SortAscending { get; set; }
}

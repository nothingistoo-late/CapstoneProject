using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Queries.GetLockedMaps;

public class GetLockedMapsQuery : IRequest<Result<PaginationResult<MapListItemDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? Difficulty { get; set; }
    public Guid? TagId { get; set; }
    public GameTypeEnum? Type { get; set; }
    public GameStatusEnum? GameStatus { get; set; }
    public string? Search { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }
    public bool SortAscending { get; set; }
}

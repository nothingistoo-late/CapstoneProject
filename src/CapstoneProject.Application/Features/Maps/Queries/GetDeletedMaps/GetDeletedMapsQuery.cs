using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Queries.GetDeletedMaps;

public class GetDeletedMapsQuery : IRequest<Result<PaginationResult<MapListItemDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? Difficulty { get; set; }
    public Guid? TagId { get; set; }
    public MapTypeEnum? Type { get; set; }
    public MapStatusEnum? MapStatus { get; set; }
    public string? Search { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public DateTime? DeletedFrom { get; set; }
    public DateTime? DeletedTo { get; set; }
    public string? SortBy { get; set; }
    public bool SortAscending { get; set; }
}

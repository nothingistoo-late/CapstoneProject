using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Queries.GetMaps;

public class GetMapsQuery : IRequest<Result<PaginationResult<MapListItemDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? Difficulty { get; set; }
    public Guid? TagId { get; set; }
    public bool? PublishedOnly { get; set; } = true;
    public MapStatusEnum? MapStatus { get; set; }
    public string? Search { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string? SortBy { get; set; }
    public bool SortAscending { get; set; }
}

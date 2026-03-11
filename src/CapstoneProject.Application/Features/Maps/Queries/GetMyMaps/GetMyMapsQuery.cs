using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Queries.GetMyMaps;

/// <summary>
/// Lấy danh sách map mà user sở hữu: map tự tạo + map đã mua (OrbitCoin).
/// Yêu cầu đăng nhập.
/// </summary>
public class GetMyMapsQuery : IRequest<Result<PaginationResult<MapListItemDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortAscending { get; set; }
}

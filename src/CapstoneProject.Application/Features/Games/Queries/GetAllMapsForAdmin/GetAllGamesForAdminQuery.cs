using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Queries.GetAllMapsForAdmin;

/// <summary>
/// Lấy tất cả game cho Admin/CMS - không lọc theo status hay bất kỳ điều kiện nào, chỉ phân trang và sắp xếp.
/// </summary>
public class GetAllMapsForAdminQuery : IRequest<Result<PaginationResult<MapListItemDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortAscending { get; set; }
}

using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Queries.GetAllMapsForAdmin;

/// <summary>
/// Lấy tất cả map cho Admin/CMS - không lọc theo status hay bất kỳ điều kiện nào, chỉ phân trang và sắp xếp.
/// </summary>
public class GetAllMapsForAdminQuery : IRequest<Result<PaginationResult<MapListItemDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortAscending { get; set; }
}

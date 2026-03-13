using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Queries.GetMyMapList;

/// <summary>
/// Lấy danh sách map từ bảng MyMap (map user sở hữu: tự tạo, mua, thêm free). Có filter isAuthor.
/// </summary>
public class GetMyMapListQuery : IRequest<Result<PaginationResult<MapListItemDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortAscending { get; set; }
    /// <summary>
    /// null = lấy hết; true = chỉ map tự tạo (author); false = chỉ map đã mua / thêm vào.
    /// </summary>
    public bool? IsAuthor { get; set; }
}

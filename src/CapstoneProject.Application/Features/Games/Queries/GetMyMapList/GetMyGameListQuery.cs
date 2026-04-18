using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Queries.GetMyGameList;

/// <summary>
/// Lấy danh sách game từ bảng MyGame (game user sở hữu: tự tạo, mua, thêm free). Có filter isAuthor.
/// </summary>
public class GetMyGameListQuery : IRequest<Result<PaginationResult<MapListItemDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortAscending { get; set; }
    /// <summary>
    /// null = lấy hết; true = chỉ game tự tạo (author); false = chỉ game đã mua / thêm vào.
    /// </summary>
    public bool? IsAuthor { get; set; }
}

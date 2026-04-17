using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Queries.GetMyGames;

/// <summary>
/// Lấy danh sách game mà user sở hữu: game tự tạo + game đã mua (OrbitCoin).
/// Yêu cầu đăng nhập.
/// </summary>
public class GetMyGamesQuery : IRequest<Result<PaginationResult<MapListItemDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortAscending { get; set; }
    /// <summary>
    /// Chỉ lấy game do chính user hiện tại tạo (author). Mặc định false = bao gồm cả game đã mua.
    /// </summary>
    public bool IsAuthorOnly { get; set; } = false;
}

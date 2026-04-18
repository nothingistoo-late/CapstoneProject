using CapstoneProject.Application.Commons.DTOs.Gameplay;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Gameplay.Queries.GetMyGamePlayHistory;

/// <summary>
/// Lịch sử chơi game của user hiện tại (phân trang, mặc định sort StartTime giảm dần).
/// </summary>
public class GetMyGamePlayHistoryQuery : IRequest<Result<PaginationResult<MapPlayHistoryItemDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    /// <summary>Lọc theo game cụ thể (optional).</summary>
    public Guid? GameId { get; set; }

    /// <summary>Lọc theo chế độ (optional).</summary>
    public PlayModeEnum? PlayMode { get; set; }
}

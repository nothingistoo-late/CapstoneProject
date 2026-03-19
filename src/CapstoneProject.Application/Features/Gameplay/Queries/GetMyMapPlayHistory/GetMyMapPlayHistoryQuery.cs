using CapstoneProject.Application.Commons.DTOs.Gameplay;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Gameplay.Queries.GetMyMapPlayHistory;

/// <summary>
/// Lịch sử chơi map của user hiện tại (phân trang, mặc định sort StartTime giảm dần).
/// </summary>
public class GetMyMapPlayHistoryQuery : IRequest<Result<PaginationResult<MapPlayHistoryItemDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    /// <summary>Lọc theo map cụ thể (optional).</summary>
    public Guid? MapId { get; set; }

    /// <summary>Lọc theo chế độ (optional).</summary>
    public PlayModeEnum? PlayMode { get; set; }
}

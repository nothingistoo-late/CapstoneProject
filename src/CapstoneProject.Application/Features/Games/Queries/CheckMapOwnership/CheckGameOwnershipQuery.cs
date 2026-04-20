using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Queries.CheckMapOwnership;

public record CheckMapOwnershipQuery(Guid GameId) : IRequest<Result<CheckMapOwnershipDto>>;

public class CheckMapOwnershipDto
{
    /// <summary>Game có tồn tại và active.</summary>
    public bool MapExists { get; set; }
    /// <summary>User hiện tại có sở hữu game (tự tạo hoặc đã mua).</summary>
    public bool IsOwned { get; set; }
    /// <summary>True nếu user là tác giả (CreatedBy); false nếu chỉ mua.</summary>
    public bool IsAuthor { get; set; }
    /// <summary>True nếu game được sở hữu thông qua giao dịch mua game (escrow Pending hoặc Completed).</summary>
    public bool IsPurchased { get; set; }
    /// <summary>Thời điểm giao dịch mua game gần nhất trong line game.</summary>
    public DateTime? PurchasedAt { get; set; }
}

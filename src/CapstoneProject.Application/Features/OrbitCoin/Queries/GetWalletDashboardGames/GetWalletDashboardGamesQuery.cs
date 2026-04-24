using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetWalletDashboardGames;

public record GetWalletDashboardGamesQuery(
    DateTime? From = null,
    DateTime? To = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<WalletDashboardGamesResultDto>>;

public class WalletDashboardGamesResultDto
{
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public List<WalletDashboardGameItemDto> Items { get; set; } = new();
}

public class WalletDashboardGameItemDto
{
    public Guid GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public int BuyersCount { get; set; }
    public int OrdersCount { get; set; }
    public int PendingOrdersCount { get; set; }
    public int RefundedOrdersCount { get; set; }
    public decimal Gross { get; set; }
    public decimal Fee { get; set; }
    public decimal Net { get; set; }
    public decimal AverageOrderValue { get; set; }
    public DateTime? LastSoldAt { get; set; }
}

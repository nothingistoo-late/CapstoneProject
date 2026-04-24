using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetWalletDashboardTrend;

public record GetWalletDashboardTrendQuery(
    string Role = "Buyer",
    DateTime? From = null,
    DateTime? To = null,
    string Bucket = "Day"
) : IRequest<Result<WalletDashboardTrendDto>>;

public class WalletDashboardTrendDto
{
    public string Role { get; set; } = "Buyer";
    public string Bucket { get; set; } = "Day";
    public List<WalletDashboardTrendItemDto> Items { get; set; } = new();
}

public class WalletDashboardTrendItemDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Inflow { get; set; }
    public decimal Outflow { get; set; }
    public decimal Net { get; set; }
    public decimal GrossRevenue { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal NetRevenue { get; set; }
}

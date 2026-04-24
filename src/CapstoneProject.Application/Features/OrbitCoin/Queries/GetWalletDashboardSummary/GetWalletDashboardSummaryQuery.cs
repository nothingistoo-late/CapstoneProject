using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetWalletDashboardSummary;

public record GetWalletDashboardSummaryQuery(
    string Role = "Buyer",
    DateTime? From = null,
    DateTime? To = null
) : IRequest<Result<WalletDashboardSummaryDto>>;

public class WalletDashboardSummaryDto
{
    public string Role { get; set; } = "Buyer";
    public string Currency { get; set; } = "VND";
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal CurrentBalanceVnd { get; set; }
    public decimal CurrentBalanceOc { get; set; }
    public decimal PendingBalance { get; set; }
    public decimal TotalIn { get; set; }
    public decimal TotalOut { get; set; }
    public decimal NetFlow { get; set; }
    public decimal IncomeThisMonth { get; set; }
    public decimal SpendingThisMonth { get; set; }
    public decimal NetProfitThisMonth { get; set; }
    public int TotalTransactions { get; set; }
    public int InflowTransactions { get; set; }
    public int OutflowTransactions { get; set; }
    public decimal GrossRevenue { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal NetRevenue { get; set; }
    public int UniqueBuyers { get; set; }
    public int UnitsSold { get; set; }
    public decimal AverageOrderValue { get; set; }
}

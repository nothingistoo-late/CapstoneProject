using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Marketplace.Queries.GetPaymentReport;

public record GetPaymentReportQuery(DateTime? From, DateTime? To, string? GroupBy = "Day") : IRequest<Result<PaymentReportDto>>;

public class PaymentReportDto
{
    /// <summary>
    /// Aggregated completed payment amount in OrbitCoin-equivalent unit.
    /// </summary>
    public decimal TotalAmount { get; set; }
    /// <summary>
    /// Aggregated completed payment amount in VND.
    /// </summary>
    public long TotalAmountVnd { get; set; }
    public int TotalCount { get; set; }
    public List<PaymentReportItemDto> Items { get; set; } = new();
}

public class PaymentReportItemDto
{
    public string Period { get; set; } = string.Empty;
    /// <summary>
    /// Grouped payment amount in OrbitCoin-equivalent unit.
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// Grouped payment amount in VND.
    /// </summary>
    public long AmountVnd { get; set; }
    public int Count { get; set; }
}

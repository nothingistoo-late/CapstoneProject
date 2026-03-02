using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Marketplace.Queries.GetPaymentReport;

public record GetPaymentReportQuery(DateTime? From, DateTime? To, string? GroupBy = "Day") : IRequest<Result<PaymentReportDto>>;

public class PaymentReportDto
{
    public decimal TotalAmount { get; set; }
    public int TotalCount { get; set; }
    public List<PaymentReportItemDto> Items { get; set; } = new();
}

public class PaymentReportItemDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

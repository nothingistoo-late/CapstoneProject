using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Marketplace.Queries.GetCmsEscrowPendingTransactions;

public record GetCmsEscrowPendingTransactionsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    DateTime? From = null,
    DateTime? To = null,
    string? Search = null
) : IRequest<Result<CmsEscrowPendingResultDto>>;

public class CmsEscrowPendingResultDto
{
    public decimal TotalPendingAmount { get; set; }
    public PaginationResult<CmsEscrowPendingTransactionDto> Transactions { get; set; } = new();
}

public class CmsEscrowPendingTransactionDto
{
    public Guid PaymentRecordId { get; set; }
    public Guid GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public Guid BuyerId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string SellerEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal FeeAmount { get; set; }
    public decimal SellerReceives { get; set; }
    public DateTime? PaidAt { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}

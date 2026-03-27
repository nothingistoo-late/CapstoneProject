using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.Interfaces;

/// <summary>
/// OrbitCoin virtual currency: balance, ledger, credit/debit/transfer with optional platform fee.
/// </summary>
public interface IOrbitCoinService
{
    Task<decimal> GetBalanceAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<OrbitCoinTransactionDto> Items, int TotalCount)> GetTransactionHistoryAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> CreditAsync(
        Guid userId,
        decimal amount,
        CoinTransactionTypeEnum transactionType,
        string? relatedEntityType,
        Guid? relatedEntityId,
        decimal feeAmount,
        string? note,
        Guid? createdBy,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> DebitAsync(
        Guid userId,
        decimal amount,
        CoinTransactionTypeEnum transactionType,
        string? relatedEntityType,
        Guid? relatedEntityId,
        decimal feeAmount,
        string? note,
        Guid? createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transfer coins from buyer to seller; platform fee is deducted from buyer. Buyer pays amount + feeTotal; seller receives amount.
    /// </summary>
    Task<(bool Success, string? Error)> TransferAsync(
        Guid buyerUserId,
        Guid sellerUserId,
        decimal amount,
        decimal feeAmount,
        CoinTransactionTypeEnum buyerTransactionType,
        CoinTransactionTypeEnum sellerTransactionType,
        string? relatedEntityType,
        Guid? relatedEntityId,
        string? note,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transfer with fee borne by seller: buyer pays amount (full price), seller receives amount - feeAmount.
    /// </summary>
    Task<(bool Success, string? Error)> TransferWithSellerFeeAsync(
        Guid buyerUserId,
        Guid sellerUserId,
        decimal amount,
        decimal feeAmount,
        CoinTransactionTypeEnum buyerTransactionType,
        CoinTransactionTypeEnum sellerTransactionType,
        string? relatedEntityType,
        Guid? relatedEntityId,
        string? note,
        CancellationToken cancellationToken = default);
}

public class OrbitCoinTransactionDto
{
    public Guid Id { get; set; }
    /// <summary>
    /// OrbitCoin amount of the transaction.
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// Real-money value in VND when this transaction is linked to a payment; otherwise null.
    /// </summary>
    public long? AmountVnd { get; set; }
    public CoinTransactionTypeEnum TransactionType { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public decimal FeeAmount { get; set; }
    public decimal? BalanceAfter { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

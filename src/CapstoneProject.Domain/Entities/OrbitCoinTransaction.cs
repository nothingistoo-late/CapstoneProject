using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Immutable ledger record for OrbitCoin: credit (+Amount) or debit (-Amount).
/// FeeAmount is the platform fee applied to this transaction.
/// </summary>
public class OrbitCoinTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public CoinTransactionTypeEnum TransactionType { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public decimal FeeAmount { get; set; }
    public decimal? BalanceAfter { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
}

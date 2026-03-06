namespace CapstoneProject.Domain.Enums;

/// <summary>
/// Type of OrbitCoin transaction. Positive amount = credit, negative = debit.
/// Coin is only added when: (1) user deposits real money (EarnDeposit), (2) user's map is purchased by another (EarnMapSold).
/// </summary>
public enum CoinTransactionTypeEnum
{
    /// <summary>Credited when user tops up / deposits real money into the system.</summary>
    EarnDeposit = 0,

    /// <summary>Credited when another user purchases the creator's map (with optional platform fee).</summary>
    EarnMapSold = 1,

    /// <summary>Debited when user purchases a map from another user.</summary>
    SpendMapPurchase = 2,

    /// <summary>Debited when user purchases a package (membership) with OrbitCoin.</summary>
    SpendPackagePurchase = 3,

    /// <summary>Platform fee deducted from a transaction.</summary>
    SystemFee = 4,

    /// <summary>Refund (e.g. failed purchase, admin).</summary>
    Refund = 5,

    /// <summary>Admin adjustment (credit or debit).</summary>
    AdminAdjustment = 6
}

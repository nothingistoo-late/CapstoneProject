using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// User's OrbitCoin wallet. One row per user; Balance is current coin amount.
/// </summary>
public class UserWallet : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }

    /// <summary>Concurrency token to prevent double-spend when updating balance.</summary>
    public byte[]? RowVersion { get; set; }
}

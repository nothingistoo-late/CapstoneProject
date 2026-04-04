using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Exchange rate between OrbitCoin and VND. Single row per currency pair.
/// Rate is stored as: 1 OrbitCoin = VndPerOrbitCoin VND
/// </summary>
public class ExchangeRate : BaseEntity
{
    /// <summary>Source currency. Always "OrbitCoin"</summary>
    public string FromCurrency { get; set; } = "OrbitCoin";

    /// <summary>Target currency. E.g. "VND" for Vietnamese Dong</summary>
    public string ToCurrency { get; set; } = "VND";

    /// <summary>Exchange rate: 1 unit of FromCurrency = Rate units of ToCurrency</summary>
    public decimal Rate { get; set; }

    /// <summary>When this rate becomes effective (UTC)</summary>
    public DateTime? EffectiveFrom { get; set; }

    /// <summary>When this rate expires (UTC), null = no expiration</summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>Is this the currently active rate?</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Comment about rate change reason</summary>
    public string? Reason { get; set; }
}

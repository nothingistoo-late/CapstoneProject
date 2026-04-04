namespace CapstoneProject.Application.Commons.DTOs.OrbitCoin;

/// <summary>DTO for exchange rate between OrbitCoin and another currency</summary>
public class ExchangeRateDto
{
    public Guid Id { get; set; }
    public string FromCurrency { get; set; } = "OrbitCoin";
    public string ToCurrency { get; set; } = "VND";
    public decimal Rate { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public string? Reason { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>Request to update exchange rate (admin only)</summary>
public class UpdateExchangeRateRequest
{
    public decimal Rate { get; set; }
    public string? Reason { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

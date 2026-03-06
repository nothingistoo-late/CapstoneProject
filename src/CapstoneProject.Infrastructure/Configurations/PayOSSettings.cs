namespace CapstoneProject.Infrastructure.Configurations;

/// <summary>
/// PayOS gateway configuration (ClientId, ApiKey, ChecksumKey from payos.vn).
/// Exchange rate: 1 OrbitCoin = VndPerOrbitCoin VND (e.g. 1000).
/// </summary>
public class PayOSSettings
{
    public const string SectionName = "PayOS";

    public string ClientId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ChecksumKey { get; set; } = string.Empty;
    /// <summary>1 OrbitCoin = this many VND. Default 1000.</summary>
    public decimal VndPerOrbitCoin { get; set; } = 1000;
    /// <summary>Base URL for redirect after success (e.g. https://yourapp.com/deposit/return). OrderId will be appended as query.</summary>
    public string ReturnUrlBase { get; set; } = "https://localhost:3000/deposit/return";
    /// <summary>Base URL for redirect when user cancels.</summary>
    public string CancelUrlBase { get; set; } = "https://localhost:3000/deposit/cancel";
}

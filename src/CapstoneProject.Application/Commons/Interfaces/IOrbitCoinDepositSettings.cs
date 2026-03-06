namespace CapstoneProject.Application.Commons.Interfaces;

/// <summary>
/// Settings for user OrbitCoin deposit (PayOS): exchange rate and redirect URLs.
/// Implemented in Infrastructure from PayOSSettings.
/// </summary>
public interface IOrbitCoinDepositSettings
{
    decimal VndPerOrbitCoin { get; }
    string ReturnUrlBase { get; }
    string CancelUrlBase { get; }
}

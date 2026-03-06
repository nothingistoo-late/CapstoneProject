using CapstoneProject.Application.Commons.Interfaces;
using Microsoft.Extensions.Options;

namespace CapstoneProject.Infrastructure.Configurations;

public class OrbitCoinDepositSettingsAdapter : IOrbitCoinDepositSettings
{
    private readonly PayOSSettings _settings;

    public OrbitCoinDepositSettingsAdapter(IOptions<PayOSSettings> options)
    {
        _settings = options.Value;
    }

    public decimal VndPerOrbitCoin => _settings.VndPerOrbitCoin;
    public string ReturnUrlBase => _settings.ReturnUrlBase;
    public string CancelUrlBase => _settings.CancelUrlBase;
}

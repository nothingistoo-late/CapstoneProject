using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CapstoneProject.Infrastructure.Configurations;

public class OrbitCoinDepositSettingsAdapter : IOrbitCoinDepositSettings
{
    private readonly PayOSSettings _settings;
    private readonly IUnitOfWork? _unitOfWork;
    private decimal? _cachedVndPerOrbitCoin;
    private DateTime _cacheTime = DateTime.MinValue;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1); // Cache for 1 hour

    public OrbitCoinDepositSettingsAdapter(IOptions<PayOSSettings> options, IUnitOfWork? unitOfWork = null)
    {
        _settings = options.Value;
        _unitOfWork = unitOfWork;
    }

    public decimal VndPerOrbitCoin
    {
        get
        {
            // Use cached value if still valid
            if (_cachedVndPerOrbitCoin.HasValue && DateTime.UtcNow - _cacheTime < _cacheDuration)
                return _cachedVndPerOrbitCoin.Value;

            // Try to fetch from database
            if (_unitOfWork != null)
            {
                try
                {
                    var exchangeRate = _unitOfWork.Repository<ExchangeRate>()
                        .GetQueryable()
                        .Where(er => er.FromCurrency == "OrbitCoin"
                            && er.ToCurrency == "VND"
                            && er.IsActive
                            && !er.IsDeleted)
                        .OrderByDescending(er => er.CreatedAt ?? DateTime.MinValue)
                        .FirstOrDefault();

                    if (exchangeRate != null)
                    {
                        _cachedVndPerOrbitCoin = exchangeRate.Rate;
                        _cacheTime = DateTime.UtcNow;
                        return exchangeRate.Rate;
                    }
                }
                catch // Fallback to settings if database access fails
                {
                    // Log error if logging is available
                }
            }

            // Fallback to appsettings value
            _cachedVndPerOrbitCoin = _settings.VndPerOrbitCoin;
            _cacheTime = DateTime.UtcNow;
            return _settings.VndPerOrbitCoin;
        }
    }

    public string ReturnUrlBase => _settings.ReturnUrlBase;
    public string CancelUrlBase => _settings.CancelUrlBase;

    /// <summary>
    /// Invalidate cache when exchange rate is updated (called from controller after update)
    /// </summary>
    public void InvalidateCache()
    {
        _cachedVndPerOrbitCoin = null;
        _cacheTime = DateTime.MinValue;
    }
}

using System.Text.Json;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.Models.Xp;

namespace CapstoneProject.Infrastructure.Services.XpPolicies;

public class BonusPolicy : IXpPolicy
{
    public string PolicyKey => "BonusPolicy";

    public Task<int> ApplyAsync(XpPolicyContext context, int currentXpValue, CancellationToken cancellationToken = default)
    {
        if (currentXpValue <= 0)
            return Task.FromResult(0);

        var multiplier = context.SourceConfig?.BonusMultiplier ?? 1.0;
        if (context.PolicyConfigs.TryGetValue(PolicyKey, out var cfg) && !string.IsNullOrWhiteSpace(cfg.ConfigJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(cfg.ConfigJson);
                if (doc.RootElement.TryGetProperty("weekendMultiplier", out var weekend) &&
                    weekend.ValueKind == JsonValueKind.Number &&
                    (context.CurrentTime.DayOfWeek == DayOfWeek.Saturday || context.CurrentTime.DayOfWeek == DayOfWeek.Sunday))
                {
                    multiplier *= weekend.GetDouble();
                }
            }
            catch
            {
                // Ignore invalid json and fall back to default multiplier.
            }
        }

        var finalValue = (int)Math.Floor(currentXpValue * multiplier);
        return Task.FromResult(Math.Max(0, finalValue));
    }
}


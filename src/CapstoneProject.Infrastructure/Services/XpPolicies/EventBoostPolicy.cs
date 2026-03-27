using System.Text.Json;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.Models.Xp;

namespace CapstoneProject.Infrastructure.Services.XpPolicies;

public class EventBoostPolicy : IXpPolicy
{
    public string PolicyKey => "EventBoostPolicy";

    public Task<int> ApplyAsync(XpPolicyContext context, int currentXpValue, CancellationToken cancellationToken = default)
    {
        if (currentXpValue <= 0)
            return Task.FromResult(0);

        var multiplier = 1.0;
        if (context.PolicyConfigs.TryGetValue(PolicyKey, out var cfg) && !string.IsNullOrWhiteSpace(cfg.ConfigJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(cfg.ConfigJson);
                if (doc.RootElement.TryGetProperty("multiplier", out var mulNode) && mulNode.ValueKind == JsonValueKind.Number)
                    multiplier = Math.Max(0.0, mulNode.GetDouble());
            }
            catch
            {
                // Ignore invalid json and keep default.
            }
        }

        var finalValue = (int)Math.Floor(currentXpValue * multiplier);
        return Task.FromResult(Math.Max(0, finalValue));
    }
}


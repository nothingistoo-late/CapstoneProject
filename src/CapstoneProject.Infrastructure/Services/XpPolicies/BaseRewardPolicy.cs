using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.Models.Xp;

namespace CapstoneProject.Infrastructure.Services.XpPolicies;

public class BaseRewardPolicy : IXpPolicy
{
    public string PolicyKey => "BaseRewardPolicy";

    public Task<int> ApplyAsync(XpPolicyContext context, int currentXpValue, CancellationToken cancellationToken = default)
    {
        if (context.SourceConfig is { IsEnabled: false })
            return Task.FromResult(0);

        var candidate = currentXpValue;
        if (candidate <= 0 && context.SourceConfig != null)
            candidate = context.SourceConfig.BaseXp;

        if (candidate < 0)
            candidate = 0;

        return Task.FromResult(candidate);
    }
}


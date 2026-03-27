using CapstoneProject.Application.Commons.Models.Xp;

namespace CapstoneProject.Application.Common.Interfaces;

public interface IXpPolicy
{
    string PolicyKey { get; }
    Task<int> ApplyAsync(XpPolicyContext context, int currentXpValue, CancellationToken cancellationToken = default);
}


using System.Text.Json;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.Models.Xp;
using CapstoneProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Infrastructure.Services.XpPolicies;

public class FirstWinOfDayPolicy : IXpPolicy
{
    private readonly IUnitOfWork _unitOfWork;
    public string PolicyKey => "FirstWinOfDayPolicy";

    public FirstWinOfDayPolicy(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> ApplyAsync(XpPolicyContext context, int currentXpValue, CancellationToken cancellationToken = default)
    {
        if (currentXpValue <= 0)
            return 0;

        var bonusXp = 15;
        if (context.PolicyConfigs.TryGetValue(PolicyKey, out var cfg) && !string.IsNullOrWhiteSpace(cfg.ConfigJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(cfg.ConfigJson);
                if (doc.RootElement.TryGetProperty("bonusXp", out var bonusNode) && bonusNode.ValueKind == JsonValueKind.Number)
                    bonusXp = Math.Max(0, bonusNode.GetInt32());
            }
            catch
            {
                // Ignore invalid json and keep default.
            }
        }

        if (bonusXp <= 0)
            return currentXpValue;

        var dayStart = context.CurrentTime.Date;
        var dayEnd = dayStart.AddDays(1);

        var hasAnyXpToday = await _unitOfWork.Repository<XpTransaction>().GetQueryable()
            .Where(x => x.UserId == context.UserId && !x.IsDeleted && x.Delta > 0 && x.CreatedAt >= dayStart && x.CreatedAt < dayEnd)
            .AnyAsync(cancellationToken);

        return hasAnyXpToday ? currentXpValue : currentXpValue + bonusXp;
    }
}


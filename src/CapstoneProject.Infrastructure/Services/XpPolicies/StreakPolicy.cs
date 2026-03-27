using System.Text.Json;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.Models.Xp;
using CapstoneProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Infrastructure.Services.XpPolicies;

public class StreakPolicy : IXpPolicy
{
    private readonly IUnitOfWork _unitOfWork;
    public string PolicyKey => "StreakPolicy";

    public StreakPolicy(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> ApplyAsync(XpPolicyContext context, int currentXpValue, CancellationToken cancellationToken = default)
    {
        if (currentXpValue <= 0)
            return 0;

        var minDays = 3;
        var bonusPerDay = 20;
        var maxBonusXp = 100;

        if (context.PolicyConfigs.TryGetValue(PolicyKey, out var cfg) && !string.IsNullOrWhiteSpace(cfg.ConfigJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(cfg.ConfigJson);
                var root = doc.RootElement;
                if (root.TryGetProperty("minDays", out var minDaysNode) && minDaysNode.ValueKind == JsonValueKind.Number)
                    minDays = Math.Max(1, minDaysNode.GetInt32());
                if (root.TryGetProperty("bonusXp", out var bonusNode) && bonusNode.ValueKind == JsonValueKind.Number)
                    bonusPerDay = Math.Max(0, bonusNode.GetInt32());
                if (root.TryGetProperty("maxBonusXp", out var maxNode) && maxNode.ValueKind == JsonValueKind.Number)
                    maxBonusXp = Math.Max(0, maxNode.GetInt32());
            }
            catch
            {
                // Keep defaults when config is invalid.
            }
        }

        if (bonusPerDay <= 0 || maxBonusXp <= 0)
            return currentXpValue;

        // Streak is counted by consecutive prior days with any positive XP transaction, then +1 for current grant day.
        var repo = _unitOfWork.Repository<XpTransaction>();
        var streakDays = 1;
        var cursorDay = context.CurrentTime.Date.AddDays(-1);

        for (var i = 0; i < 30; i++)
        {
            var dayStart = cursorDay;
            var dayEnd = dayStart.AddDays(1);
            var hadXp = await repo.GetQueryable()
                .Where(x => x.UserId == context.UserId && !x.IsDeleted && x.Delta > 0 && x.CreatedAt >= dayStart && x.CreatedAt < dayEnd)
                .AnyAsync(cancellationToken);

            if (!hadXp)
                break;

            streakDays++;
            cursorDay = cursorDay.AddDays(-1);
        }

        if (streakDays < minDays)
            return currentXpValue;

        var bonus = Math.Min(maxBonusXp, streakDays * bonusPerDay);
        return currentXpValue + bonus;
    }
}


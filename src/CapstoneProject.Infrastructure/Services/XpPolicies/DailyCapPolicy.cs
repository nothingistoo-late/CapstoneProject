using System.Text.Json;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.Models.Xp;
using CapstoneProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Infrastructure.Services.XpPolicies;

public class DailyCapPolicy : IXpPolicy
{
    private readonly IUnitOfWork _unitOfWork;
    public string PolicyKey => "DailyCapPolicy";

    public DailyCapPolicy(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> ApplyAsync(XpPolicyContext context, int currentXpValue, CancellationToken cancellationToken = default)
    {
        if (currentXpValue <= 0)
            return 0;

        var globalCap = 0;
        if (context.PolicyConfigs.TryGetValue(PolicyKey, out var cfg) && !string.IsNullOrWhiteSpace(cfg.ConfigJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(cfg.ConfigJson);
                if (doc.RootElement.TryGetProperty("globalDailyCap", out var capNode) && capNode.ValueKind == JsonValueKind.Number)
                    globalCap = capNode.GetInt32();
            }
            catch
            {
                globalCap = 0;
            }
        }

        var sourceCap = context.SourceConfig?.DailyCap ?? 0;
        var effectiveCap = sourceCap > 0 && globalCap > 0 ? Math.Min(sourceCap, globalCap) : Math.Max(sourceCap, globalCap);
        if (effectiveCap <= 0)
            return currentXpValue;

        var dayStart = context.CurrentTime.Date;
        var dayEnd = dayStart.AddDays(1);

        var earnedToday = await _unitOfWork.Repository<XpTransaction>().GetQueryable()
            .Where(x => x.UserId == context.UserId && !x.IsDeleted && x.CreatedAt >= dayStart && x.CreatedAt < dayEnd)
            .SumAsync(x => x.Delta, cancellationToken);

        var remain = Math.Max(0, effectiveCap - earnedToday);
        return Math.Min(currentXpValue, remain);
    }
}


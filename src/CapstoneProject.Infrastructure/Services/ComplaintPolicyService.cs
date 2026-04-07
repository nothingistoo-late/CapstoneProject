using System.Text.Json;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.Models.Complaints;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using CapstoneProject.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Infrastructure.Services;

public class ComplaintPolicyService : IComplaintPolicyService
{
    private const string OtherCategoryKey = "Other";

    private readonly IUnitOfWork _unitOfWork;
    private readonly CapstoneProjectDbContext _dbContext;

    public ComplaintPolicyService(IUnitOfWork unitOfWork, CapstoneProjectDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public async Task<ComplaintCreatePolicyResult> ValidateCreateAsync(ComplaintCreatePolicyInput input, CancellationToken cancellationToken)
    {
        var categoryKey = (input.CategoryKey ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(categoryKey))
            return Fail("CategoryKey is required.");

        var category = await _unitOfWork.Repository<ComplaintCategoryCatalog>().GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.IsEnabled && x.CategoryKey == categoryKey, cancellationToken);
        if (category == null)
            return Fail($"Complaint category not found or disabled: {categoryKey}.");

        var rules = await _unitOfWork.Repository<ComplaintPolicyRuleConfig>().GetQueryable()
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsEnabled && x.CategoryKey == categoryKey)
            .OrderBy(x => x.Priority)
            .ToListAsync(cancellationToken);

        var ctx = input.Context ?? new ComplaintCreateContextInput();

        var requiredContextRule = rules.FirstOrDefault(x => x.RuleKey == "required_context");
        if (requiredContextRule != null && !HasAnyRequiredContext(ctx, requiredContextRule.ConfigJson))
            return Fail("Missing required context for this complaint category.");

        if (!await ValidateOwnershipAsync(input.UserId, ctx, cancellationToken))
            return Fail("You do not have permission to create complaint for the provided context.");

        var timeWindowRule = rules.FirstOrDefault(x => x.RuleKey == "time_window");
        var anchorTime = await ResolveAnchorTimeAsync(input.UserId, ctx, cancellationToken);
        if (timeWindowRule != null && !anchorTime.HasValue)
            return Fail("Unable to determine event time for this complaint category.");
        if (timeWindowRule != null && !IsWithinTimeWindow(anchorTime, timeWindowRule.ConfigJson))
            return Fail("Complaint is outside configured time window.");

        var contextKey = BuildContextKey(categoryKey, input.UserId, ctx);

        var duplicateRule = rules.FirstOrDefault(x => x.RuleKey == "duplicate_window");
        if (duplicateRule != null)
        {
            var duplicateHours = ReadIntConfig(duplicateRule.ConfigJson, "hours", 72);
            var duplicateSince = VietnamDateTime.DbNow.AddHours(-duplicateHours);
            var hasDuplicate = await _unitOfWork.Repository<Complaint>().GetQueryable()
                .AnyAsync(c => !c.IsDeleted
                               && c.UserId == input.UserId
                               && (c.ComplaintStatus == ComplaintStatusEnum.Open || c.ComplaintStatus == ComplaintStatusEnum.InProgress)
                               && c.CategoryKey == categoryKey
                               && c.ContextKey == contextKey
                               && c.CreatedAt.HasValue
                               && c.CreatedAt.Value >= duplicateSince,
                    cancellationToken);
            if (hasDuplicate)
                return Fail("A similar complaint is already open or in progress.");
        }

        var rateRule = rules.FirstOrDefault(x => x.RuleKey == "rate_limit");
        if (rateRule != null)
        {
            var maxPerDay = ReadIntConfig(rateRule.ConfigJson, "maxPerDay", 3);
            var dayStart = VietnamDateTime.DbNow.Date;
            var dayEnd = dayStart.AddDays(1);
            var complaintQuery = _unitOfWork.Repository<Complaint>().GetQueryable()
                .Where(c => !c.IsDeleted
                            && c.UserId == input.UserId
                            && c.CreatedAt.HasValue
                            && c.CreatedAt.Value >= dayStart
                            && c.CreatedAt.Value < dayEnd);

            if (string.Equals(categoryKey, OtherCategoryKey, StringComparison.OrdinalIgnoreCase))
                complaintQuery = complaintQuery.Where(c => c.CategoryKey == categoryKey);

            var countToday = await complaintQuery.CountAsync(cancellationToken);
            if (countToday >= maxPerDay)
                return Fail("Daily complaint limit reached.");
        }

        var (contextType, contextId) = ResolveContextTypeAndId(ctx);
        return new ComplaintCreatePolicyResult
        {
            IsSuccess = true,
            CategoryKey = categoryKey,
            CategoryDisplayName = category.DisplayName,
            ContextType = contextType,
            ContextId = contextId,
            ContextKey = contextKey,
            OccurredAt = VietnamDateTime.ToDbDateTime(anchorTime),
            NormalizedContextJson = JsonSerializer.Serialize(ctx)
        };
    }

    private static ComplaintCreatePolicyResult Fail(string message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message
    };

    private static bool HasAnyRequiredContext(ComplaintCreateContextInput ctx, string? configJson)
    {
        var requiredKeys = ReadStringArrayConfig(configJson, "anyOf");
        if (requiredKeys.Count == 0)
            return HasAnyContext(ctx);

        foreach (var key in requiredKeys)
        {
            if (key == "paymentRecordId" && ctx.PaymentRecordId.HasValue) return true;
            if (key == "mapId" && ctx.MapId.HasValue) return true;
            if (key == "packageId" && ctx.PackageId.HasValue) return true;
            if (key == "submissionId" && ctx.SubmissionId.HasValue) return true;
            if (key == "playHistoryId" && ctx.PlayHistoryId.HasValue) return true;
            if (key == "xpTransactionId" && ctx.XpTransactionId.HasValue) return true;
            if (key == "orbitCoinTransactionId" && ctx.OrbitCoinTransactionId.HasValue) return true;
        }

        return false;
    }

    private static bool HasAnyContext(ComplaintCreateContextInput ctx)
    {
        return ctx.PaymentRecordId.HasValue
            || ctx.MapId.HasValue
            || ctx.PackageId.HasValue
            || ctx.SubmissionId.HasValue
            || ctx.PlayHistoryId.HasValue
            || ctx.XpTransactionId.HasValue
            || ctx.OrbitCoinTransactionId.HasValue;
    }

    private async Task<bool> ValidateOwnershipAsync(Guid userId, ComplaintCreateContextInput ctx, CancellationToken cancellationToken)
    {
        if (ctx.PaymentRecordId.HasValue)
        {
            var ok = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                .AnyAsync(x => !x.IsDeleted && x.Id == ctx.PaymentRecordId.Value && x.UserId == userId, cancellationToken);
            if (!ok) return false;
        }

        if (ctx.SubmissionId.HasValue)
        {
            var ok = await _unitOfWork.Repository<Submission>().GetQueryable()
                .AnyAsync(x => !x.IsDeleted && x.Id == ctx.SubmissionId.Value && x.UserId == userId, cancellationToken);
            if (!ok) return false;
        }

        if (ctx.PlayHistoryId.HasValue)
        {
            var ok = await _unitOfWork.Repository<UserMapPlayHistory>().GetQueryable()
                .AnyAsync(x => !x.IsDeleted && x.Id == ctx.PlayHistoryId.Value && x.UserId == userId, cancellationToken);
            if (!ok) return false;
        }

        if (ctx.XpTransactionId.HasValue)
        {
            var ok = await _unitOfWork.Repository<XpTransaction>().GetQueryable()
                .AnyAsync(x => !x.IsDeleted && x.Id == ctx.XpTransactionId.Value && x.UserId == userId, cancellationToken);
            if (!ok) return false;
        }

        if (ctx.MapId.HasValue)
        {
            var mapId = ctx.MapId.Value;
            var isCreator = await _unitOfWork.Repository<Map>().GetQueryable()
                .AnyAsync(x => !x.IsDeleted && x.Id == mapId && x.CreatedBy == userId, cancellationToken);
            if (!isCreator)
            {
                var purchased = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                    .AnyAsync(x => !x.IsDeleted && x.UserId == userId && x.MapId == mapId && x.PaymentStatus == PaymentStatusEnum.Completed, cancellationToken);
                var inMyMap = await _unitOfWork.Repository<MyMap>().GetQueryable()
                    .AnyAsync(x => !x.IsDeleted && x.UserId == userId && x.MapId == mapId, cancellationToken);
                var hasPurchaseAttempt = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                    .AnyAsync(x => !x.IsDeleted && x.UserId == userId && x.MapId == mapId && x.PaymentStatus != PaymentStatusEnum.Completed, cancellationToken);
                if (!purchased && !inMyMap && !hasPurchaseAttempt)
                    return false;
            }
        }

        if (ctx.PackageId.HasValue)
        {
            var packageId = ctx.PackageId.Value;
            var hasUserPackage = await _unitOfWork.Repository<UserPackage>().GetQueryable()
                .AnyAsync(x => !x.IsDeleted && x.UserId == userId && x.PackageId == packageId, cancellationToken);
            var hasPurchaseAttempt = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                .AnyAsync(x => !x.IsDeleted && x.UserId == userId && x.PackageId == packageId && x.PaymentStatus != PaymentStatusEnum.Completed, cancellationToken);

            if (!hasUserPackage && !hasPurchaseAttempt)
                return false;
        }

        return true;
    }

    private async Task<DateTime?> ResolveAnchorTimeAsync(Guid userId, ComplaintCreateContextInput ctx, CancellationToken cancellationToken)
    {
        if (ctx.PaymentRecordId.HasValue)
        {
            var paidAt = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                .Where(x => !x.IsDeleted && x.Id == ctx.PaymentRecordId.Value)
                .Select(x => x.PaidAt)
                .FirstOrDefaultAsync(cancellationToken);
            if (paidAt.HasValue) return paidAt.Value;
        }

        if (ctx.PlayHistoryId.HasValue)
        {
            var playedAt = await _unitOfWork.Repository<UserMapPlayHistory>().GetQueryable()
                .Where(x => !x.IsDeleted && x.Id == ctx.PlayHistoryId.Value)
                .Select(x => x.EndTime ?? x.StartTime)
                .FirstOrDefaultAsync(cancellationToken);
            if (playedAt != default) return playedAt;
        }

        if (ctx.SubmissionId.HasValue)
        {
            var submittedAt = await _unitOfWork.Repository<Submission>().GetQueryable()
                .Where(x => !x.IsDeleted && x.Id == ctx.SubmissionId.Value)
                .Select(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
            if (submittedAt.HasValue) return submittedAt.Value;
        }

        if (ctx.XpTransactionId.HasValue)
        {
            var xpAt = await _unitOfWork.Repository<XpTransaction>().GetQueryable()
                .Where(x => !x.IsDeleted && x.Id == ctx.XpTransactionId.Value)
                .Select(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
            if (xpAt.HasValue) return xpAt.Value;
        }

        if (ctx.OrbitCoinTransactionId.HasValue)
        {
            var orbitAt = await _dbContext.OrbitCoinTransactions
                .AsNoTracking()
                .Where(x => x.Id == ctx.OrbitCoinTransactionId.Value && x.UserId == userId)
                .Select(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
            if (orbitAt != default)
                return VietnamDateTime.ToDbDateTime(orbitAt);
        }

        if (ctx.MapId.HasValue)
        {
            var mapPaymentAt = await ResolveLatestPaymentTimeForMapAsync(userId, ctx.MapId.Value, cancellationToken);
            if (mapPaymentAt.HasValue)
                return mapPaymentAt.Value;

            var mapCreatedAt = await _unitOfWork.Repository<Map>().GetQueryable()
                .Where(x => !x.IsDeleted && x.Id == ctx.MapId.Value)
                .Select(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
            if (mapCreatedAt.HasValue)
                return VietnamDateTime.ToDbDateTime(mapCreatedAt.Value);
        }

        if (ctx.PackageId.HasValue)
        {
            var packagePaymentAt = await ResolveLatestPaymentTimeForPackageAsync(userId, ctx.PackageId.Value, cancellationToken);
            if (packagePaymentAt.HasValue)
                return packagePaymentAt.Value;

            var packageCreatedAt = await _unitOfWork.Repository<Package>().GetQueryable()
                .Where(x => !x.IsDeleted && x.Id == ctx.PackageId.Value)
                .Select(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
            if (packageCreatedAt.HasValue)
                return VietnamDateTime.ToDbDateTime(packageCreatedAt.Value);
        }

        return null;
    }

    private static bool IsWithinTimeWindow(DateTime? anchorTime, string? configJson)
    {
        if (!anchorTime.HasValue)
            return true;

        var hours = ReadIntConfig(configJson, "hours", 0);
        if (hours <= 0)
            return true;

        return VietnamDateTime.ToDbDateTime(anchorTime.Value) >= VietnamDateTime.DbNow.AddHours(-hours);
    }

    private async Task<DateTime?> ResolveLatestPaymentTimeForMapAsync(Guid userId, Guid mapId, CancellationToken cancellationToken)
    {
        var paymentAt = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .Where(x => !x.IsDeleted && x.UserId == userId && x.MapId == mapId)
            .OrderByDescending(x => x.PaidAt ?? x.CreatedAt)
            .Select(x => x.PaidAt ?? x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return paymentAt.HasValue ? VietnamDateTime.ToDbDateTime(paymentAt.Value) : null;
    }

    private async Task<DateTime?> ResolveLatestPaymentTimeForPackageAsync(Guid userId, Guid packageId, CancellationToken cancellationToken)
    {
        var paymentAt = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .Where(x => !x.IsDeleted && x.UserId == userId && x.PackageId == packageId)
            .OrderByDescending(x => x.PaidAt ?? x.CreatedAt)
            .Select(x => x.PaidAt ?? x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return paymentAt.HasValue ? VietnamDateTime.ToDbDateTime(paymentAt.Value) : null;
    }

    private static (string? ContextType, Guid? ContextId) ResolveContextTypeAndId(ComplaintCreateContextInput ctx)
    {
        if (ctx.PaymentRecordId.HasValue) return ("PaymentRecord", ctx.PaymentRecordId.Value);
        if (ctx.SubmissionId.HasValue) return ("Submission", ctx.SubmissionId.Value);
        if (ctx.PlayHistoryId.HasValue) return ("PlayHistory", ctx.PlayHistoryId.Value);
        if (ctx.XpTransactionId.HasValue) return ("XpTransaction", ctx.XpTransactionId.Value);
        if (ctx.OrbitCoinTransactionId.HasValue) return ("OrbitCoinTransaction", ctx.OrbitCoinTransactionId.Value);
        if (ctx.MapId.HasValue) return ("Map", ctx.MapId.Value);
        if (ctx.PackageId.HasValue) return ("Package", ctx.PackageId.Value);
        return (null, null);
    }

    private static string BuildContextKey(string categoryKey, Guid userId, ComplaintCreateContextInput ctx)
    {
        var strongest = ctx.PaymentRecordId?.ToString()
            ?? ctx.SubmissionId?.ToString()
            ?? ctx.PlayHistoryId?.ToString()
            ?? ctx.XpTransactionId?.ToString()
            ?? ctx.OrbitCoinTransactionId?.ToString()
            ?? ctx.MapId?.ToString()
            ?? ctx.PackageId?.ToString()
            ?? "none";

        return $"{categoryKey}:{strongest}:{userId}";
    }

    private static int ReadIntConfig(string? configJson, string propertyName, int fallback)
    {
        if (string.IsNullOrWhiteSpace(configJson))
            return fallback;

        try
        {
            using var doc = JsonDocument.Parse(configJson);
            if (doc.RootElement.ValueKind == JsonValueKind.Object
                && doc.RootElement.TryGetProperty(propertyName, out var p)
                && p.ValueKind == JsonValueKind.Number
                && p.TryGetInt32(out var value))
            {
                return value;
            }
        }
        catch
        {
            // ignore malformed config and use fallback
        }

        return fallback;
    }

    private static List<string> ReadStringArrayConfig(string? configJson, string propertyName)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(configJson))
            return result;

        try
        {
            using var doc = JsonDocument.Parse(configJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return result;

            if (!doc.RootElement.TryGetProperty(propertyName, out var p) || p.ValueKind != JsonValueKind.Array)
                return result;

            foreach (var item in p.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var val = item.GetString();
                    if (!string.IsNullOrWhiteSpace(val))
                        result.Add(val.Trim());
                }
            }
        }
        catch
        {
            // ignore malformed config
        }

        return result;
    }
}

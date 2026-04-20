using System.Text.Json;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.Models.Complaints;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using CapstoneProject.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CapstoneProject.Infrastructure.Services;

public class ComplaintPolicyService : IComplaintPolicyService
{
    private const string OtherCategoryKey = "Other";
    private static readonly HashSet<string> RefundableCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "PaymentIssue",
        "AccessIssue",
        "GameplayScoringIssue",
        "TrialIssue"
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly CapstoneProjectDbContext _dbContext;
    private readonly ComplaintWorkflowOptions _workflowOptions;

    public ComplaintPolicyService(IUnitOfWork unitOfWork, CapstoneProjectDbContext dbContext, IOptions<ComplaintWorkflowOptions> workflowOptions)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _workflowOptions = workflowOptions.Value;
    }

    public async Task<ComplaintCreatePolicyResult> ValidateCreateAsync(ComplaintCreatePolicyInput input, CancellationToken cancellationToken)
    {
        var categoryKey = (input.CategoryKey ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(categoryKey))
            return Fail("CategoryKey là bắt buộc.");

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

        if (_workflowOptions.EnableDailyComplaintLimit)
        {
            var rateRule = rules.FirstOrDefault(x => x.RuleKey == "rate_limit");
            var maxPerDay = Math.Max(1, _workflowOptions.MaxReportsPerBuyerPerDay);
            if (rateRule != null)
                maxPerDay = ReadIntConfig(rateRule.ConfigJson, "maxPerDay", maxPerDay);

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

    public async Task<ComplaintRefundPolicyResult> ValidateRefundAsync(ComplaintRefundPolicyInput input, CancellationToken cancellationToken)
    {
        if (input.ComplaintId == Guid.Empty)
            return RefundFail("ComplaintId là bắt buộc.");

        if (!RefundableCategories.Contains(input.CategoryKey))
            return RefundFail("Danh mục khiếu nại này không hỗ trợ hoàn tiền.");

        var complaint = await _unitOfWork.Repository<Complaint>().GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == input.ComplaintId && x.UserId == input.UserId, cancellationToken);
        if (complaint == null)
            return RefundFail("Không tìm thấy khiếu nại hợp lệ của người dùng.");

        if (complaint.ComplaintStatus != ComplaintStatusEnum.Resolved
            && complaint.ComplaintStatus != ComplaintStatusEnum.ResolvedRefund)
            return RefundFail("Chỉ khiếu nại đã được giải quyết mới có thể hoàn tiền.");

        if (!complaint.CreatedAt.HasValue)
            return RefundFail("Không xác định được thời điểm tạo khiếu nại.");

        var anchorTime = complaint.OccurredAt ?? complaint.CreatedAt;
        if (!anchorTime.HasValue)
            return RefundFail("Không xác định được thời điểm sự cố.");

        var refundableWindowHours = ReadIntConfig("{\"hours\":168}", "hours", 168);
        var refundBefore = VietnamDateTime.DbNow.AddHours(-refundableWindowHours);
        if (VietnamDateTime.ToDbDateTime(anchorTime.Value) < refundBefore)
            return RefundFail("Khiếu nại đã quá thời hạn hoàn tiền.");

        var paymentRecordId = input.PaymentRecordId;
        if (!paymentRecordId.HasValue && input.ContextType != null && input.ContextId.HasValue)
        {
            paymentRecordId = await ResolvePaymentRecordIdAsync(input.UserId, input.ContextType, input.ContextId.Value, cancellationToken);
        }

        if (!paymentRecordId.HasValue)
            return RefundFail("Không tìm thấy giao dịch thanh toán phù hợp để hoàn tiền.");

        var payment = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == paymentRecordId.Value && x.UserId == input.UserId, cancellationToken);
        if (payment == null)
            return RefundFail("Giao dịch thanh toán không hợp lệ.");

        if (payment.PaymentStatus == PaymentStatusEnum.Refunded)
            return RefundFail("Giao dịch này đã được hoàn tiền trước đó.");

        if (payment.PaymentStatus != PaymentStatusEnum.Completed && payment.PaymentStatus != PaymentStatusEnum.Pending)
            return RefundFail("Chỉ giao dịch hợp lệ (Pending/Completed) mới được hoàn tiền.");

        var refundAmount = payment.Amount;
        if (refundAmount <= 0)
            return RefundFail("Số tiền hoàn không hợp lệ.");

        return new ComplaintRefundPolicyResult
        {
            IsSuccess = true,
            PaymentRecordId = payment.Id,
            RefundAmount = refundAmount,
            RefundReason = string.IsNullOrWhiteSpace(input.Note) ? $"Refund for complaint {input.ComplaintId}" : input.Note.Trim()
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
            if (key == "gameId" && ctx.GameId.HasValue) return true;
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
            || ctx.GameId.HasValue
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
            var ok = await _unitOfWork.Repository<UserGamePlayHistory>().GetQueryable()
                .AnyAsync(x => !x.IsDeleted && x.Id == ctx.PlayHistoryId.Value && x.UserId == userId, cancellationToken);
            if (!ok) return false;
        }

        if (ctx.XpTransactionId.HasValue)
        {
            var ok = await _unitOfWork.Repository<XpTransaction>().GetQueryable()
                .AnyAsync(x => !x.IsDeleted && x.Id == ctx.XpTransactionId.Value && x.UserId == userId, cancellationToken);
            if (!ok) return false;
        }

        if (ctx.GameId.HasValue)
        {
            var gameId = ctx.GameId.Value;
            var game = await _unitOfWork.Repository<Game>().GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == gameId, cancellationToken);
            if (game == null)
                return false;

            var purchased = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                .AnyAsync(x => !x.IsDeleted
                               && x.UserId == userId
                               && x.GameId == gameId
                               && (x.PaymentStatus == PaymentStatusEnum.Pending || x.PaymentStatus == PaymentStatusEnum.Completed),
                    cancellationToken);
            if (game.Price.HasValue && game.Price.Value > 0)
                return purchased;

            var inMyGame = await _unitOfWork.Repository<MyGame>().GetQueryable()
                .AnyAsync(x => !x.IsDeleted && x.UserId == userId && x.GameId == gameId, cancellationToken);
            return inMyGame || purchased;
        }

        if (ctx.PackageId.HasValue)
        {
            var packageId = ctx.PackageId.Value;
            var hasUserPackage = await _unitOfWork.Repository<UserPackage>().GetQueryable()
                .AnyAsync(x => !x.IsDeleted && x.UserId == userId && x.PackageId == packageId, cancellationToken);
            if (!hasUserPackage)
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
            var playedAt = await _unitOfWork.Repository<UserGamePlayHistory>().GetQueryable()
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

        if (ctx.GameId.HasValue)
        {
            var mapPaymentAt = await ResolveLatestPaymentTimeForMapAsync(userId, ctx.GameId.Value, cancellationToken);
            if (mapPaymentAt.HasValue)
                return mapPaymentAt.Value;

            var mapCreatedAt = await _unitOfWork.Repository<Game>().GetQueryable()
                .Where(x => !x.IsDeleted && x.Id == ctx.GameId.Value)
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

    private async Task<DateTime?> ResolveLatestPaymentTimeForMapAsync(Guid userId, Guid gameId, CancellationToken cancellationToken)
    {
        var paymentAt = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .Where(x => !x.IsDeleted && x.UserId == userId && x.GameId == gameId)
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

    private async Task<Guid?> ResolvePaymentRecordIdAsync(Guid userId, string contextType, Guid contextId, CancellationToken cancellationToken)
    {
        if (string.Equals(contextType, "PaymentRecord", StringComparison.OrdinalIgnoreCase))
        {
            var direct = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                .Where(x => !x.IsDeleted && x.Id == contextId && x.UserId == userId)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (direct.HasValue)
                return direct;
        }

        if (string.Equals(contextType, "Game", StringComparison.OrdinalIgnoreCase))
        {
            return await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                .Where(x => !x.IsDeleted
                            && x.UserId == userId
                            && x.GameId == contextId
                            && (x.PaymentStatus == PaymentStatusEnum.Pending || x.PaymentStatus == PaymentStatusEnum.Completed))
                .OrderByDescending(x => x.PaidAt ?? x.CreatedAt)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (string.Equals(contextType, "Package", StringComparison.OrdinalIgnoreCase))
        {
            return await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                .Where(x => !x.IsDeleted
                            && x.UserId == userId
                            && x.PackageId == contextId
                            && (x.PaymentStatus == PaymentStatusEnum.Pending || x.PaymentStatus == PaymentStatusEnum.Completed))
                .OrderByDescending(x => x.PaidAt ?? x.CreatedAt)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return null;
    }

    private static ComplaintRefundPolicyResult RefundFail(string message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message
    };

    private static (string? ContextType, Guid? ContextId) ResolveContextTypeAndId(ComplaintCreateContextInput ctx)
    {
        if (ctx.PaymentRecordId.HasValue) return ("PaymentRecord", ctx.PaymentRecordId.Value);
        if (ctx.SubmissionId.HasValue) return ("Submission", ctx.SubmissionId.Value);
        if (ctx.PlayHistoryId.HasValue) return ("PlayHistory", ctx.PlayHistoryId.Value);
        if (ctx.XpTransactionId.HasValue) return ("XpTransaction", ctx.XpTransactionId.Value);
        if (ctx.OrbitCoinTransactionId.HasValue) return ("OrbitCoinTransaction", ctx.OrbitCoinTransactionId.Value);
        if (ctx.GameId.HasValue) return ("Game", ctx.GameId.Value);
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
            ?? ctx.GameId?.ToString()
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

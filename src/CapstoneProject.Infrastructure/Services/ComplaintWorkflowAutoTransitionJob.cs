using System.Text.Json;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Commons.Models.Complaints;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using CapstoneProject.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CapstoneProject.Infrastructure.Services;

public class ComplaintWorkflowAutoTransitionJob
{
    private const decimal PlatformFeePercent = 5m;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CapstoneProjectDbContext _dbContext;
    private readonly INotificationPersistenceService _notificationPersistenceService;
    private readonly IOrbitCoinService _orbitCoinService;
    private readonly ComplaintWorkflowOptions _options;

    public ComplaintWorkflowAutoTransitionJob(
        IUnitOfWork unitOfWork,
        CapstoneProjectDbContext dbContext,
        INotificationPersistenceService notificationPersistenceService,
        IOrbitCoinService orbitCoinService,
        IOptions<ComplaintWorkflowOptions> options)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _notificationPersistenceService = notificationPersistenceService;
        _orbitCoinService = orbitCoinService;
        _options = options.Value;
    }

    public Task ExecuteAsync() => ExecuteAsync(CancellationToken.None);

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.EnableAutoTransitions)
            return;

        await ReleaseEscrowPaymentsWithoutComplaintsAsync(cancellationToken);

        var now = VietnamDateTime.DbNow;
        var complaints = await _unitOfWork.Repository<Complaint>().GetQueryable()
            .Where(x => !x.IsDeleted && (
                x.ComplaintStatus == ComplaintStatusEnum.Open ||
                x.ComplaintStatus == ComplaintStatusEnum.SellerPending ||
                x.ComplaintStatus == ComplaintStatusEnum.FixInProgress ||
                x.ComplaintStatus == ComplaintStatusEnum.Verified ||
                x.ComplaintStatus == ComplaintStatusEnum.ResolvedReject ||
                x.ComplaintStatus == ComplaintStatusEnum.ResolvedRefund))
            .ToListAsync(cancellationToken);

        if (complaints.Count == 0)
            return;

        var changed = false;
        foreach (var complaint in complaints)
        {
            var sellerId = await ResolveSellerUserIdAsync(complaint, cancellationToken);

            if (complaint.ComplaintStatus == ComplaintStatusEnum.Open || complaint.ComplaintStatus == ComplaintStatusEnum.SellerPending)
            {
                var cutoff = now.AddMinutes(-Math.Max(1, _options.SellerResponseTimeoutMinutes));
                if (complaint.CreatedAt.HasValue && complaint.CreatedAt.Value <= cutoff && sellerId.HasValue)
                {
                    var sellerResponded = await _unitOfWork.Repository<ComplaintMessage>().GetQueryable()
                        .AnyAsync(m => !m.IsDeleted
                                       && m.ComplaintId == complaint.Id
                                       && m.SenderId == sellerId.Value
                                       && m.CreatedAt.HasValue
                                       && m.CreatedAt.Value >= complaint.CreatedAt.Value,
                            cancellationToken);
                    if (!sellerResponded)
                    {
                        await TransitionAsync(
                            complaint,
                            ComplaintStatusEnum.SellerNoResponse,
                            complaint.UserId,
                            "Auto transition: seller did not respond in time.",
                            cancellationToken);
                        changed = true;
                        continue;
                    }
                }
            }

            if (complaint.ComplaintStatus == ComplaintStatusEnum.FixInProgress)
            {
                var enteredAt = await ResolveEnteredStatusAtAsync(complaint.Id, ComplaintStatusEnum.FixInProgress, complaint.UpdatedAt ?? complaint.CreatedAt, cancellationToken);
                var cutoff = now.AddMinutes(-Math.Max(1, _options.SellerFixTimeoutMinutes));
                if (enteredAt.HasValue && enteredAt.Value <= cutoff)
                {
                    await TransitionAsync(
                        complaint,
                        ComplaintStatusEnum.SellerNoResponse,
                        complaint.UserId,
                        "Auto transition: seller did not submit fix in time.",
                        cancellationToken);
                    changed = true;
                    continue;
                }
            }

            if (complaint.ComplaintStatus == ComplaintStatusEnum.Verified)
            {
                var enteredAt = await ResolveEnteredStatusAtAsync(complaint.Id, ComplaintStatusEnum.Verified, complaint.UpdatedAt ?? complaint.CreatedAt, cancellationToken);
                var cutoff = now.AddMinutes(-Math.Max(1, _options.BuyerVerificationTimeoutMinutes));
                if (enteredAt.HasValue && enteredAt.Value <= cutoff)
                {
                    await TransitionAsync(
                        complaint,
                        ComplaintStatusEnum.ResolvedReject,
                        complaint.UserId,
                        "Auto transition: buyer did not verify in time.",
                        cancellationToken);
                    changed = true;
                }
            }

            if (complaint.ComplaintStatus == ComplaintStatusEnum.ResolvedReject || complaint.ComplaintStatus == ComplaintStatusEnum.ResolvedRefund)
            {
                var enteredAt = await ResolveEnteredStatusAtAsync(complaint.Id, complaint.ComplaintStatus, complaint.ResolvedAt ?? complaint.UpdatedAt ?? complaint.CreatedAt, cancellationToken);
                var cutoff = now.AddMinutes(-Math.Max(1, _options.ResolvedAutoCloseMinutes));
                if (enteredAt.HasValue && enteredAt.Value <= cutoff)
                {
                    await TransitionAsync(
                        complaint,
                        ComplaintStatusEnum.Closed,
                        complaint.UserId,
                        "Auto transition: resolved ticket auto-closed.",
                        cancellationToken);
                    changed = true;
                }
            }
        }

        if (changed)
            await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task TransitionAsync(
        Complaint complaint,
        ComplaintStatusEnum toStatus,
        Guid actorUserId,
        string note,
        CancellationToken cancellationToken)
    {
        var fromStatus = complaint.ComplaintStatus;
        complaint.ComplaintStatus = toStatus;
        if (toStatus == ComplaintStatusEnum.ResolvedReject || toStatus == ComplaintStatusEnum.ResolvedRefund)
            complaint.ResolvedAt = VietnamDateTime.DbNow;

        if (toStatus == ComplaintStatusEnum.ResolvedReject)
        {
            var payoutResult = await TryReleaseEscrowToSellerIfPendingAsync(complaint, actorUserId, cancellationToken);
            if (!payoutResult.Success)
                return;
        }

        complaint.UpdateEntity(actorUserId);
        _unitOfWork.Repository<Complaint>().Update(complaint);

        var history = new ComplaintStatusHistory
        {
            ComplaintId = complaint.Id,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedBy = actorUserId,
            ChangedAt = VietnamDateTime.DbNow,
            Note = note
        };
        history.InitializeEntity(actorUserId);
        await _unitOfWork.Repository<ComplaintStatusHistory>().AddAsync(history);

        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                complaintId = complaint.Id,
                fromStatus = fromStatus.ToString(),
                toStatus = toStatus.ToString(),
                auto = true
            });

            await _notificationPersistenceService.CreateNotificationAsync(
                NotificationTypeEnum.ComplaintStatusChanged,
                "Cap nhat trang thai khieu nai tu dong",
                $"Khieu nai \"{complaint.Subject}\" da tu dong chuyen tu {fromStatus} sang {toStatus}.",
                new List<Guid> { complaint.UserId },
                actorUserId,
                payload,
                $"/learner/complaints/{complaint.Id}",
                cancellationToken);
        }
        catch
        {
        }
    }

    private async Task<DateTime?> ResolveEnteredStatusAtAsync(
        Guid complaintId,
        ComplaintStatusEnum status,
        DateTime? fallback,
        CancellationToken cancellationToken)
    {
        var changedAt = await _unitOfWork.Repository<ComplaintStatusHistory>().GetQueryable()
            .Where(x => !x.IsDeleted && x.ComplaintId == complaintId && x.ToStatus == status)
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => (DateTime?)x.ChangedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return changedAt ?? fallback;
    }

    private async Task<Guid?> ResolveSellerUserIdAsync(Complaint complaint, CancellationToken cancellationToken)
    {
        Guid? gameId = null;

        if (string.Equals(complaint.ContextType, "Game", StringComparison.OrdinalIgnoreCase) && complaint.ContextId.HasValue)
            gameId = complaint.ContextId.Value;

        if (gameId == null
            && string.Equals(complaint.ContextType, "PaymentRecord", StringComparison.OrdinalIgnoreCase)
            && complaint.ContextId.HasValue)
        {
            gameId = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                .Where(x => !x.IsDeleted && x.Id == complaint.ContextId.Value)
                .Select(x => x.GameId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (!gameId.HasValue)
            return null;

        return await _unitOfWork.Repository<Game>().GetQueryable()
            .Where(x => !x.IsDeleted && x.Id == gameId.Value)
            .Select(x => x.CreatedBy)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task ReleaseEscrowPaymentsWithoutComplaintsAsync(CancellationToken cancellationToken)
    {
        var now = VietnamDateTime.DbNow;
        var cutoff = now.AddMinutes(-Math.Max(1, _options.PriorityReportWindowMinutes));

        var pendingPayments = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .Where(x => !x.IsDeleted
                        && x.GameId.HasValue
                        && x.PaymentStatus == PaymentStatusEnum.Pending
                        && x.PaidAt.HasValue
                        && x.PaidAt.Value <= cutoff)
            .ToListAsync(cancellationToken);

        if (pendingPayments.Count == 0)
            return;

        foreach (var payment in pendingPayments)
        {
            var hasOpenComplaint = await _unitOfWork.Repository<Complaint>().GetQueryable()
                .AnyAsync(c => !c.IsDeleted
                               && c.UserId == payment.UserId
                               && c.ContextType == "PaymentRecord"
                               && c.ContextId == payment.Id
                               && c.ComplaintStatus != ComplaintStatusEnum.ResolvedReject
                               && c.ComplaintStatus != ComplaintStatusEnum.ResolvedRefund
                               && c.ComplaintStatus != ComplaintStatusEnum.Closed,
                    cancellationToken);

            if (hasOpenComplaint)
                continue;

            var game = await _unitOfWork.Repository<Game>().GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(g => !g.IsDeleted && g.Id == payment.GameId.Value, cancellationToken);
            if (game?.CreatedBy == null || game.CreatedBy == Guid.Empty)
                continue;

            var feeAmount = Math.Round(payment.Amount * (PlatformFeePercent / 100m), 4);
            var sellerReceive = payment.Amount - feeAmount;
            if (sellerReceive < 0)
                continue;

            var payout = await _orbitCoinService.CreditAsync(
                game.CreatedBy.Value,
                sellerReceive,
                CoinTransactionTypeEnum.EarnMapSold,
                "PaymentRecord",
                payment.Id,
                feeAmount,
                $"Escrow auto-release for payment {payment.Id}",
                payment.UserId,
                cancellationToken);

            if (!payout.Success)
                continue;

            payment.PaymentStatus = PaymentStatusEnum.Completed;
            payment.UpdateEntity(payment.UserId);
            _unitOfWork.Repository<PaymentRecord>().Update(payment);

            await TryNotifyEscrowReleasedAsync(
                payment.UserId,
                game.CreatedBy.Value,
                payment.Id,
                payment.Amount,
                sellerReceive,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<(bool Success, string? ErrorMessage)> TryReleaseEscrowToSellerIfPendingAsync(
        Complaint complaint,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var payment = await ResolveEscrowPaymentAsync(complaint, cancellationToken);
        if (payment == null || payment.PaymentStatus != PaymentStatusEnum.Pending || !payment.GameId.HasValue)
            return (true, null);

        var sellerUserId = await _unitOfWork.Repository<Game>().GetQueryable()
            .Where(g => !g.IsDeleted && g.Id == payment.GameId.Value)
            .Select(g => g.CreatedBy)
            .FirstOrDefaultAsync(cancellationToken);

        if (!sellerUserId.HasValue || sellerUserId.Value == Guid.Empty)
            return (false, "Could not resolve seller.");

        var feeAmount = Math.Round(payment.Amount * (PlatformFeePercent / 100m), 4);
        var sellerReceive = payment.Amount - feeAmount;
        if (sellerReceive < 0)
            return (false, "Invalid fee.");

        var payout = await _orbitCoinService.CreditAsync(
            sellerUserId.Value,
            sellerReceive,
            CoinTransactionTypeEnum.EarnMapSold,
            "PaymentRecord",
            payment.Id,
            feeAmount,
            $"Escrow release for payment {payment.Id}",
            actorUserId,
            cancellationToken);

        if (!payout.Success)
            return (false, payout.Error ?? "Escrow release failed.");

        payment.PaymentStatus = PaymentStatusEnum.Completed;
        payment.UpdateEntity(actorUserId);
        _unitOfWork.Repository<PaymentRecord>().Update(payment);

        await TryNotifyEscrowReleasedAsync(
            payment.UserId,
            sellerUserId.Value,
            payment.Id,
            payment.Amount,
            sellerReceive,
            cancellationToken);

        return (true, null);
    }

    private async Task TryNotifyEscrowReleasedAsync(
        Guid buyerUserId,
        Guid sellerUserId,
        Guid paymentRecordId,
        decimal grossAmount,
        decimal sellerReceive,
        CancellationToken cancellationToken)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                paymentRecordId,
                escrowReleased = true,
                grossAmount,
                sellerReceive
            });

            await _notificationPersistenceService.CreateNotificationAsync(
                NotificationTypeEnum.PaymentSucceeded,
                "Escrow da duoc giai ngan",
                $"Khoan thanh toan {grossAmount:0.##} OrbitCoin da duoc giai ngan cho nguoi ban.",
                new List<Guid> { buyerUserId },
                sellerUserId,
                payload,
                "/learner/wallet",
                cancellationToken);

            await _notificationPersistenceService.CreateNotificationAsync(
                NotificationTypeEnum.MapPurchased,
                "Da nhan tien tu escrow",
                $"Ban da nhan {sellerReceive:0.##} OrbitCoin tu escrow cho giao dich ban tro choi.",
                new List<Guid> { sellerUserId },
                buyerUserId,
                payload,
                "/learner/wallet",
                cancellationToken);
        }
        catch
        {
        }
    }

    private async Task<PaymentRecord?> ResolveEscrowPaymentAsync(Complaint complaint, CancellationToken cancellationToken)
    {
        if (string.Equals(complaint.ContextType, "PaymentRecord", StringComparison.OrdinalIgnoreCase) && complaint.ContextId.HasValue)
        {
            return await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == complaint.ContextId.Value, cancellationToken);
        }

        if (string.Equals(complaint.ContextType, "Game", StringComparison.OrdinalIgnoreCase) && complaint.ContextId.HasValue)
        {
            return await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                .Where(x => !x.IsDeleted
                            && x.UserId == complaint.UserId
                            && x.GameId == complaint.ContextId.Value)
                .OrderByDescending(x => x.PaidAt ?? x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return null;
    }
}

using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using CapstoneProject.Application.Commons.Models.Complaints;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CapstoneProject.Application.Features.Complaints.Commands.ChangeComplaintStatus;

public class ChangeComplaintStatusCommandHandler : IRequestHandler<ChangeComplaintStatusCommand, Result<ComplaintStatusUpdateDto>>
{
    private const decimal PlatformFeePercent = 5m;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IComplaintContextResolver _complaintContextResolver;
    private readonly IComplaintPolicyService _complaintPolicyService;
    private readonly IOrbitCoinService _orbitCoinService;
    private readonly INotificationPersistenceService _notificationPersistenceService;

    public ChangeComplaintStatusCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IComplaintContextResolver complaintContextResolver,
        IComplaintPolicyService complaintPolicyService,
        IOrbitCoinService orbitCoinService,
        INotificationPersistenceService notificationPersistenceService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _complaintContextResolver = complaintContextResolver;
        _complaintPolicyService = complaintPolicyService;
        _orbitCoinService = orbitCoinService;
        _notificationPersistenceService = notificationPersistenceService;
    }

    public async Task<Result<ComplaintStatusUpdateDto>> Handle(ChangeComplaintStatusCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<ComplaintStatusUpdateDto>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để thay đổi trạng thái khiếu nại.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var roles = await _currentUserService.GetCurrentRolesAsync();
        var isModerator = roles.Contains(RoleEnum.Admin) || roles.Contains(RoleEnum.Moderator);

        if (command.ComplaintId == Guid.Empty)
            return Result<ComplaintStatusUpdateDto>.Failure("Khiếu nạiId là bắt buộc.", ErrorCodeEnum.ValidationFailed);

        var complaintRepo = _unitOfWork.Repository<Complaint>();
        var complaint = await complaintRepo.GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == command.ComplaintId && !c.IsDeleted, cancellationToken);
        if (complaint == null)
            return Result<ComplaintStatusUpdateDto>.Failure($"Không tìm thấy khiếu nại với Id: {command.ComplaintId}.", ErrorCodeEnum.NotFound);

        var fromStatus = complaint.ComplaintStatus;
        var toStatus = NormalizeRequestedStatus(command.ToStatus, command.IssueRefund);
        var sellerId = await ResolveSellerUserIdAsync(complaint, cancellationToken);
        var isSeller = sellerId.HasValue && sellerId.Value == userId;
        var isBuyer = complaint.UserId == userId;

        if (!isModerator && !isSeller && !isBuyer)
            return Result<ComplaintStatusUpdateDto>.Failure("Bạn không có quyền thay đổi trạng thái khiếu nại này.", ErrorCodeEnum.Forbidden);

        if (isSeller && toStatus == ComplaintStatusEnum.FixSubmitted)
        {
            var complaintGameId = await ResolveComplaintGameIdAsync(complaint, cancellationToken);
            if (!complaintGameId.HasValue)
                return Result<ComplaintStatusUpdateDto>.Failure("Game chưa được gửi hoặc chưa được sửa.", ErrorCodeEnum.ValidationFailed);

            var hasPendingReviewInLine = await HasSellerSubmittedFixForComplaintGameAsync(
                complaintGameId.Value,
                userId,
                cancellationToken);

            if (!hasPendingReviewInLine)
                return Result<ComplaintStatusUpdateDto>.Failure("Game chưa được gửi hoặc chưa được sửa.", ErrorCodeEnum.ValidationFailed);
        }

        if (fromStatus == toStatus)
        {
            var noChange = await BuildStatusDtoAsync(complaint, fromStatus, toStatus, command.Note, false, null, null, cancellationToken);
            return Result<ComplaintStatusUpdateDto>.Success(noChange, "Không có thay đổi trạng thái.");
        }

        var allowed = IsTransitionAllowed(fromStatus, toStatus, isModerator, isSeller, isBuyer);
        if (!allowed)
            return Result<ComplaintStatusUpdateDto>.Failure($"Chuyển đổi trạng thái không hợp lệ: {fromStatus} -> {toStatus}.", ErrorCodeEnum.ValidationFailed);

        complaint.ComplaintStatus = toStatus;
        var refundProcessed = false;
        Guid? refundedPaymentRecordId = null;
        decimal? refundAmount = null;

        if (toStatus == ComplaintStatusEnum.ResolvedRefund)
        {
            complaint.ResolvedAt = VietnamDateTime.DbNow;
            var refundPolicy = await _complaintPolicyService.ValidateRefundAsync(new ComplaintRefundPolicyInput
            {
                ComplaintId = complaint.Id,
                UserId = complaint.UserId,
                TargetStatus = toStatus,
                CategoryKey = complaint.CategoryKey,
                ContextType = complaint.ContextType,
                ContextId = complaint.ContextId,
                OccurredAt = complaint.OccurredAt,
                Note = command.Note
            }, cancellationToken);

            if (!refundPolicy.IsSuccess)
                return Result<ComplaintStatusUpdateDto>.Failure(refundPolicy.ErrorMessage ?? "Không đủ điều kiện hoàn tiền.", ErrorCodeEnum.ValidationFailed);

            var refundResult = await _orbitCoinService.CreditAsync(
                complaint.UserId,
                refundPolicy.RefundAmount,
                CoinTransactionTypeEnum.Refund,
                "PaymentRecord",
                refundPolicy.PaymentRecordId,
                0,
                refundPolicy.RefundReason,
                userId,
                cancellationToken);
            if (!refundResult.Success)
                return Result<ComplaintStatusUpdateDto>.Failure(refundResult.Error ?? "Không thể thực hiện hoàn tiền.", ErrorCodeEnum.InvalidOperation);

            var payment = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                .FirstAsync(x => x.Id == refundPolicy.PaymentRecordId && !x.IsDeleted, cancellationToken);
            payment.PaymentStatus = PaymentStatusEnum.Refunded;
            payment.UpdateEntity(userId);
            _unitOfWork.Repository<PaymentRecord>().Update(payment);

            await RevokeBuyerGameOwnershipAfterRefundAsync(complaint.UserId, payment, userId, cancellationToken);

            refundProcessed = true;
            refundedPaymentRecordId = payment.Id;
            refundAmount = refundPolicy.RefundAmount;

            complaint.RefundProcessed = true;
            complaint.RefundedPaymentRecordId = payment.Id;
            complaint.RefundAmount = refundPolicy.RefundAmount;
            complaint.RefundedAt = VietnamDateTime.DbNow;
            complaint.RefundReason = refundPolicy.RefundReason;
        }
        else if (toStatus == ComplaintStatusEnum.ResolvedReject || toStatus == ComplaintStatusEnum.Resolved)
        {
            complaint.ResolvedAt = VietnamDateTime.DbNow;

            var escrowReleased = await TryReleaseEscrowToSellerIfPendingAsync(complaint, userId, cancellationToken);
            if (!escrowReleased.Success)
                return Result<ComplaintStatusUpdateDto>.Failure(escrowReleased.ErrorMessage ?? "Không thể giải ngân escrow cho người bán.", ErrorCodeEnum.InvalidOperation);
        }

        complaint.UpdateEntity(userId);
        complaintRepo.Update(complaint);

        var history = new ComplaintStatusHistory
        {
            ComplaintId = complaint.Id,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedBy = userId,
            ChangedAt = VietnamDateTime.DbNow,
            Note = string.IsNullOrWhiteSpace(command.Note) ? null : command.Note.Trim()
        };
        history.InitializeEntity(userId);
        await _unitOfWork.Repository<ComplaintStatusHistory>().AddAsync(history);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await CreateComplaintStatusNotificationsAsync(
            complaint,
            fromStatus,
            toStatus,
            refundProcessed,
            refundAmount,
            sellerId,
            userId,
            cancellationToken);

        var response = await BuildStatusDtoAsync(complaint, fromStatus, toStatus, history.Note, refundProcessed, refundedPaymentRecordId, refundAmount, cancellationToken);
        return Result<ComplaintStatusUpdateDto>.Success(response, "Đã cập nhật trạng thái khiếu nại.");
    }

    private static ComplaintStatusEnum NormalizeRequestedStatus(ComplaintStatusEnum requested, bool issueRefund)
    {
        if (requested == ComplaintStatusEnum.InProgress)
            return ComplaintStatusEnum.SellerPending;

        if (requested == ComplaintStatusEnum.Resolved)
            return issueRefund ? ComplaintStatusEnum.ResolvedRefund : ComplaintStatusEnum.ResolvedReject;

        return requested;
    }

    private static bool IsTransitionAllowed(
        ComplaintStatusEnum from,
        ComplaintStatusEnum to,
        bool isModerator,
        bool isSeller,
        bool isBuyer)
    {
        if (isModerator)
        {
            return from switch
            {
                ComplaintStatusEnum.Open => to is ComplaintStatusEnum.SellerPending or ComplaintStatusEnum.SellerNoResponse,
                ComplaintStatusEnum.InProgress => to is ComplaintStatusEnum.FixInProgress or ComplaintStatusEnum.SellerRejected or ComplaintStatusEnum.SellerNoResponse,
                ComplaintStatusEnum.SellerPending => to is ComplaintStatusEnum.FixInProgress or ComplaintStatusEnum.SellerRejected or ComplaintStatusEnum.SellerNoResponse,
                ComplaintStatusEnum.FixInProgress => to is ComplaintStatusEnum.FixSubmitted or ComplaintStatusEnum.SellerNoResponse,
                ComplaintStatusEnum.FixSubmitted => to == ComplaintStatusEnum.Verified,
                ComplaintStatusEnum.Verified => to is ComplaintStatusEnum.ResolvedReject or ComplaintStatusEnum.ResolvedRefund,
                ComplaintStatusEnum.SellerRejected => to is ComplaintStatusEnum.ResolvedReject or ComplaintStatusEnum.ResolvedRefund,
                ComplaintStatusEnum.SellerNoResponse => to is ComplaintStatusEnum.ResolvedReject or ComplaintStatusEnum.ResolvedRefund,
                ComplaintStatusEnum.ResolvedReject => to == ComplaintStatusEnum.Closed,
                ComplaintStatusEnum.ResolvedRefund => to == ComplaintStatusEnum.Closed,
                ComplaintStatusEnum.Resolved => to == ComplaintStatusEnum.Closed,
                ComplaintStatusEnum.Closed => false,
                _ => false
            };
        }

        if (isSeller)
        {
            return from switch
            {
                ComplaintStatusEnum.Open => to is ComplaintStatusEnum.FixInProgress or ComplaintStatusEnum.SellerRejected,
                ComplaintStatusEnum.InProgress => to is ComplaintStatusEnum.FixInProgress or ComplaintStatusEnum.SellerRejected,
                ComplaintStatusEnum.SellerPending => to is ComplaintStatusEnum.FixInProgress or ComplaintStatusEnum.SellerRejected,
                ComplaintStatusEnum.FixInProgress => to == ComplaintStatusEnum.FixSubmitted,
                _ => false
            };
        }

        if (isBuyer)
        {
            return from switch
            {
                ComplaintStatusEnum.Verified => to is ComplaintStatusEnum.ResolvedReject or ComplaintStatusEnum.SellerRejected,
                _ => false
            };
        }

        return false;
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

    private async Task<Guid?> ResolveComplaintGameIdAsync(Complaint complaint, CancellationToken cancellationToken)
    {
        if (string.Equals(complaint.ContextType, "Game", StringComparison.OrdinalIgnoreCase) && complaint.ContextId.HasValue)
            return complaint.ContextId.Value;

        if (string.Equals(complaint.ContextType, "PaymentRecord", StringComparison.OrdinalIgnoreCase) && complaint.ContextId.HasValue)
        {
            return await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                .Where(x => !x.IsDeleted && x.Id == complaint.ContextId.Value)
                .Select(x => x.GameId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return null;
    }

    private async Task<bool> HasSellerSubmittedFixForComplaintGameAsync(
        Guid complaintGameId,
        Guid sellerUserId,
        CancellationToken cancellationToken)
    {
        var gameLineRootId = await _unitOfWork.Repository<Game>().GetQueryable()
            .Where(g => g.Id == complaintGameId)
            .Select(g => g.RootGameId ?? g.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (gameLineRootId == Guid.Empty)
            return false;

        return await _unitOfWork.Repository<Game>().GetQueryable()
            .AnyAsync(g => !g.IsDeleted
                           && g.CreatedBy == sellerUserId
                           && (g.RootGameId ?? g.Id) == gameLineRootId
                           && g.GameStatus == GameStatusEnum.PendingReview,
                cancellationToken);
    }

    private async Task<(bool Success, string? ErrorMessage)> TryReleaseEscrowToSellerIfPendingAsync(
        Complaint complaint,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var payment = await ResolveEscrowPaymentAsync(complaint, cancellationToken);
        if (payment == null)
            return (true, null);

        if (payment.PaymentStatus != PaymentStatusEnum.Pending)
            return (true, null);

        if (!payment.GameId.HasValue)
            return (false, "Giao dịch escrow không xác định được game.");

        var sellerUserId = await _unitOfWork.Repository<Game>().GetQueryable()
            .Where(g => !g.IsDeleted && g.Id == payment.GameId.Value)
            .Select(g => g.CreatedBy)
            .FirstOrDefaultAsync(cancellationToken);

        if (!sellerUserId.HasValue || sellerUserId.Value == Guid.Empty)
            return (false, "Không xác định được người bán để giải ngân escrow.");

        var feeAmount = Math.Round(payment.Amount * (PlatformFeePercent / 100m), 4);
        var sellerReceive = payment.Amount - feeAmount;
        if (sellerReceive < 0)
            return (false, "Phí nền tảng không hợp lệ khi giải ngân escrow.");

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
            return (false, payout.Error ?? "Giải ngân escrow thất bại.");

        payment.PaymentStatus = PaymentStatusEnum.Completed;
        payment.UpdateEntity(actorUserId);
        _unitOfWork.Repository<PaymentRecord>().Update(payment);

        await TryNotifyEscrowReleasedAsync(
            payment.UserId,
            sellerUserId.Value,
            payment.Id,
            payment.Amount,
            sellerReceive,
            actorUserId,
            cancellationToken);

        return (true, null);
    }

    private async Task RevokeBuyerGameOwnershipAfterRefundAsync(
        Guid buyerUserId,
        PaymentRecord refundedPayment,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (!refundedPayment.GameId.HasValue)
            return;

        var game = await _unitOfWork.Repository<Game>().GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(g => !g.IsDeleted && g.Id == refundedPayment.GameId.Value, cancellationToken);
        if (game == null)
            return;

        var rootGameId = game.RootGameId ?? game.Id;
        var lineGameIds = await _unitOfWork.Repository<Game>().GetQueryable()
            .Where(g => !g.IsDeleted && (g.RootGameId ?? g.Id) == rootGameId)
            .Select(g => g.Id)
            .ToListAsync(cancellationToken);
        if (lineGameIds.Count == 0)
            return;

        var myGameRepo = _unitOfWork.Repository<MyGame>();
        var ownedRows = await myGameRepo.GetQueryable()
            .Where(mg => !mg.IsDeleted
                         && mg.UserId == buyerUserId
                         && !mg.IsAuthor
                         && lineGameIds.Contains(mg.GameId))
            .ToListAsync(cancellationToken);
        if (ownedRows.Count == 0)
            return;

        foreach (var ownedRow in ownedRows)
        {
            ownedRow.SoftDeleteEntity(actorUserId);
            ownedRow.UpdateEntity(actorUserId);
        }

        myGameRepo.UpdateRange(ownedRows);
    }

    private async Task TryNotifyEscrowReleasedAsync(
        Guid buyerUserId,
        Guid sellerUserId,
        Guid paymentRecordId,
        decimal grossAmount,
        decimal sellerReceive,
        Guid actorUserId,
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
                "Escrow đã được giải ngân",
                $"Khoản thanh toán {grossAmount:0.##} OrbitCoin đã được giải ngân cho người bán.",
                new List<Guid> { buyerUserId },
                actorUserId,
                payload,
                "/learner/wallet",
                cancellationToken);

            await _notificationPersistenceService.CreateNotificationAsync(
                NotificationTypeEnum.MapPurchased,
                "Đã nhận tiền từ escrow",
                $"Bạn đã nhận {sellerReceive:0.##} OrbitCoin từ escrow cho giao dịch bán game.",
                new List<Guid> { sellerUserId },
                actorUserId,
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

    private async Task CreateComplaintStatusNotificationsAsync(
        Complaint complaint,
        ComplaintStatusEnum fromStatus,
        ComplaintStatusEnum toStatus,
        bool refundProcessed,
        decimal? refundAmount,
        Guid? sellerUserId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        try
        {
            var statusPayload = JsonSerializer.Serialize(new
            {
                complaintId = complaint.Id,
                fromStatus = fromStatus.ToString(),
                toStatus = toStatus.ToString(),
                refundProcessed,
                refundAmount
            });

            var recipients = new HashSet<Guid> { complaint.UserId };
            if (sellerUserId.HasValue && sellerUserId.Value != complaint.UserId)
                recipients.Add(sellerUserId.Value);

            await _notificationPersistenceService.CreateNotificationAsync(
                NotificationTypeEnum.ComplaintStatusChanged,
                "Cập nhật trạng thái khiếu nại",
                $"Khiếu nại \"{complaint.Subject}\" đã chuyển từ {fromStatus} sang {toStatus}.",
                recipients.ToList(),
                actorUserId,
                statusPayload,
                $"/learner/complaints/{complaint.Id}",
                cancellationToken);

            if (!refundProcessed)
                return;

            var refundPayload = JsonSerializer.Serialize(new
            {
                complaintId = complaint.Id,
                refundAmount
            });

            await _notificationPersistenceService.CreateNotificationAsync(
                NotificationTypeEnum.ComplaintRefunded,
                "Khiếu nại đã được hoàn tiền",
                refundAmount.HasValue
                    ? $"Bạn đã được hoàn {refundAmount.Value:0.##} OrbitCoin cho khiếu nại \"{complaint.Subject}\"."
                    : $"Khiếu nại \"{complaint.Subject}\" đã được hoàn tiền.",
                new List<Guid> { complaint.UserId },
                actorUserId,
                refundPayload,
                $"/learner/complaints/{complaint.Id}",
                cancellationToken);
        }
        catch
        {
            // Notification failure must not break complaint workflow.
        }
    }

    private async Task<ComplaintStatusUpdateDto> BuildStatusDtoAsync(
        Complaint complaint,
        ComplaintStatusEnum fromStatus,
        ComplaintStatusEnum toStatus,
        string? note,
        bool refundProcessed,
        Guid? refundedPaymentRecordId,
        decimal? refundAmount,
        CancellationToken cancellationToken)
    {
        return new ComplaintStatusUpdateDto
        {
            ComplaintId = complaint.Id,
            Subject = complaint.Subject,
            Category = complaint.Category,
            CategoryKey = complaint.CategoryKey,
            FromStatus = fromStatus.ToString(),
            ToStatus = toStatus.ToString(),
            CurrentStatus = complaint.ComplaintStatus.ToString(),
            ChangedAt = VietnamDateTime.DbNow,
            Note = note,
            IssueRefund = refundProcessed,
            RefundProcessed = refundProcessed,
            RefundedPaymentRecordId = refundedPaymentRecordId,
            RefundAmount = refundAmount,
            ResolvedAt = complaint.ResolvedAt,
            ContextType = complaint.ContextType,
            ContextId = complaint.ContextId,
            ContextKey = complaint.ContextKey,
            ContextDataJson = complaint.ContextDataJson,
            OccurredAt = complaint.OccurredAt,
            ContextResolved = await _complaintContextResolver.ResolveAsync(
                complaint.ContextType,
                complaint.ContextId,
                complaint.ContextDataJson,
                complaint.UserId,
                cancellationToken)
        };
    }
}


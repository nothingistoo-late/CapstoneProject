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
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<ComplaintStatusUpdateDto>.Failure("Bạn không có quyền thay đổi trạng thái khiếu nại. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện hành động này.", ErrorCodeEnum.Forbidden);

        if (command.ComplaintId == Guid.Empty)
            return Result<ComplaintStatusUpdateDto>.Failure("Khiếu nạiId là bắt buộc.", ErrorCodeEnum.ValidationFailed);

        var complaintRepo = _unitOfWork.Repository<Complaint>();
        var complaint = await complaintRepo.GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == command.ComplaintId && !c.IsDeleted, cancellationToken);
        if (complaint == null)
            return Result<ComplaintStatusUpdateDto>.Failure($"Không tìm thấy khiếu nại với Id: {command.ComplaintId}.", ErrorCodeEnum.NotFound);

        var fromStatus = complaint.ComplaintStatus;
        var toStatus = command.ToStatus;

        if (fromStatus == toStatus)
        {
            var noChange = await BuildStatusDtoAsync(complaint, fromStatus, toStatus, command.Note, false, null, null, cancellationToken);
            return Result<ComplaintStatusUpdateDto>.Success(noChange, "Không có thay đổi trạng thái.");
        }

        var allowed = fromStatus switch
        {
            ComplaintStatusEnum.Open => toStatus == ComplaintStatusEnum.InProgress,
            ComplaintStatusEnum.InProgress => toStatus == ComplaintStatusEnum.Resolved,
            ComplaintStatusEnum.Resolved => false,
            _ => false
        };
        if (!allowed)
            return Result<ComplaintStatusUpdateDto>.Failure($"Chuyển đổi trạng thái không hợp lệ: {fromStatus} -> {toStatus}.", ErrorCodeEnum.ValidationFailed);

        complaint.ComplaintStatus = toStatus;
        var refundProcessed = false;
        Guid? refundedPaymentRecordId = null;
        decimal? refundAmount = null;

        if (toStatus == ComplaintStatusEnum.Resolved)
        {
            complaint.ResolvedAt = VietnamDateTime.DbNow;
            if (command.IssueRefund)
            {
                var refundPolicy = await _complaintPolicyService.ValidateRefundAsync(new ComplaintRefundPolicyInput
                {
                    ComplaintId = complaint.Id,
                    UserId = complaint.UserId,
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

                refundProcessed = true;
                refundedPaymentRecordId = payment.Id;
                refundAmount = refundPolicy.RefundAmount;

                complaint.RefundProcessed = true;
                complaint.RefundedPaymentRecordId = payment.Id;
                complaint.RefundAmount = refundPolicy.RefundAmount;
                complaint.RefundedAt = VietnamDateTime.DbNow;
                complaint.RefundReason = refundPolicy.RefundReason;
            }
        }
        else
        {
            complaint.RefundProcessed = false;
            complaint.RefundedPaymentRecordId = null;
            complaint.RefundAmount = null;
            complaint.RefundedAt = null;
            complaint.RefundReason = null;
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
            userId,
            cancellationToken);

        var response = await BuildStatusDtoAsync(complaint, fromStatus, toStatus, history.Note, refundProcessed, refundedPaymentRecordId, refundAmount, cancellationToken);
        return Result<ComplaintStatusUpdateDto>.Success(response, "Đã cập nhật trạng thái khiếu nại.");
    }

    private async Task CreateComplaintStatusNotificationsAsync(
        Complaint complaint,
        ComplaintStatusEnum fromStatus,
        ComplaintStatusEnum toStatus,
        bool refundProcessed,
        decimal? refundAmount,
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

            await _notificationPersistenceService.CreateNotificationAsync(
                NotificationTypeEnum.ComplaintStatusChanged,
                "Cập nhật trạng thái khiếu nại",
                $"Khiếu nại \"{complaint.Subject}\" đã chuyển từ {fromStatus} sang {toStatus}.",
                new List<Guid> { complaint.UserId },
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


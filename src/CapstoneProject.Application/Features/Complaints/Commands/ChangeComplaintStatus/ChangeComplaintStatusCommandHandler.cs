using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Commands.ChangeComplaintStatus;

public class ChangeComplaintStatusCommandHandler : IRequestHandler<ChangeComplaintStatusCommand, Result<ComplaintStatusUpdateDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IComplaintContextResolver _complaintContextResolver;

    public ChangeComplaintStatusCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IComplaintContextResolver complaintContextResolver)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _complaintContextResolver = complaintContextResolver;
    }

    public async Task<Result<ComplaintStatusUpdateDto>> Handle(ChangeComplaintStatusCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<ComplaintStatusUpdateDto>.Failure("Authentication required. Please log in to change complaint status.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<ComplaintStatusUpdateDto>.Failure("You do not have permission to change complaint status. Only Admin or Moderator can perform this action.", ErrorCodeEnum.Forbidden);

        if (command.ComplaintId == Guid.Empty)
            return Result<ComplaintStatusUpdateDto>.Failure("ComplaintId is required.", ErrorCodeEnum.ValidationFailed);

        var complaintRepo = _unitOfWork.Repository<Complaint>();
        var complaint = await complaintRepo.GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == command.ComplaintId && !c.IsDeleted, cancellationToken);
        if (complaint == null)
            return Result<ComplaintStatusUpdateDto>.Failure($"Complaint not found with Id: {command.ComplaintId}.", ErrorCodeEnum.NotFound);

        var fromStatus = complaint.ComplaintStatus;
        var toStatus = command.ToStatus;

        if (fromStatus == toStatus)
        {
            var noChange = await BuildStatusDtoAsync(complaint, fromStatus, toStatus, command.Note, cancellationToken);
            return Result<ComplaintStatusUpdateDto>.Success(noChange, "No status change.");
        }

        var allowed = fromStatus switch
        {
            ComplaintStatusEnum.Open => toStatus == ComplaintStatusEnum.InProgress,
            ComplaintStatusEnum.InProgress => toStatus == ComplaintStatusEnum.Resolved,
            ComplaintStatusEnum.Resolved => false,
            _ => false
        };
        if (!allowed)
            return Result<ComplaintStatusUpdateDto>.Failure($"Invalid status transition: {fromStatus} -> {toStatus}.", ErrorCodeEnum.ValidationFailed);

        complaint.ComplaintStatus = toStatus;
        if (toStatus == ComplaintStatusEnum.Resolved)
            complaint.ResolvedAt = VietnamDateTime.DbNow;

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

        var response = await BuildStatusDtoAsync(complaint, fromStatus, toStatus, history.Note, cancellationToken);
        return Result<ComplaintStatusUpdateDto>.Success(response, "Complaint status updated.");
    }

    private async Task<ComplaintStatusUpdateDto> BuildStatusDtoAsync(
        Complaint complaint,
        ComplaintStatusEnum fromStatus,
        ComplaintStatusEnum toStatus,
        string? note,
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


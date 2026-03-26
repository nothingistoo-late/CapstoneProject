using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Commands.ChangeComplaintStatus;

public class ChangeComplaintStatusCommandHandler : IRequestHandler<ChangeComplaintStatusCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ChangeComplaintStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ChangeComplaintStatusCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Authentication required. Please log in to change complaint status.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result.Failure("You do not have permission to change complaint status. Only Admin or Moderator can perform this action.", ErrorCodeEnum.Forbidden);

        if (command.ComplaintId == Guid.Empty)
            return Result.Failure("ComplaintId is required.", ErrorCodeEnum.ValidationFailed);

        var complaintRepo = _unitOfWork.Repository<Complaint>();
        var complaint = await complaintRepo.GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == command.ComplaintId && !c.IsDeleted, cancellationToken);
        if (complaint == null)
            return Result.Failure($"Complaint not found with Id: {command.ComplaintId}.", ErrorCodeEnum.NotFound);

        var fromStatus = complaint.ComplaintStatus;
        var toStatus = command.ToStatus;

        if (fromStatus == toStatus)
            return Result.Success("No status change.");

        var allowed = fromStatus switch
        {
            ComplaintStatusEnum.Open => toStatus == ComplaintStatusEnum.InProgress,
            ComplaintStatusEnum.InProgress => toStatus == ComplaintStatusEnum.Resolved,
            ComplaintStatusEnum.Resolved => false,
            _ => false
        };
        if (!allowed)
            return Result.Failure($"Invalid status transition: {fromStatus} -> {toStatus}.", ErrorCodeEnum.ValidationFailed);

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
        return Result.Success("Complaint status updated.");
    }
}


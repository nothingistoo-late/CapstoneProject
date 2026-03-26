using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Commands.SendComplaintMessageAsStaff;

public class SendComplaintMessageAsStaffCommandHandler : IRequestHandler<SendComplaintMessageAsStaffCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SendComplaintMessageAsStaffCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(SendComplaintMessageAsStaffCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<Guid>.Failure("Authentication required. Please log in to send staff message.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<Guid>.Failure("You do not have permission to send staff messages. Only Admin or Moderator can perform this action.", ErrorCodeEnum.Forbidden);

        if (command.ComplaintId == Guid.Empty)
            return Result<Guid>.Failure("ComplaintId is required.", ErrorCodeEnum.ValidationFailed);
        if (string.IsNullOrWhiteSpace(command.Content))
            return Result<Guid>.Failure("Message content is required.", ErrorCodeEnum.ValidationFailed);

        var complaint = await _unitOfWork.Repository<Complaint>().GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == command.ComplaintId && !c.IsDeleted, cancellationToken);
        if (complaint == null)
            return Result<Guid>.Failure($"Complaint not found with Id: {command.ComplaintId}.", ErrorCodeEnum.NotFound);

        var message = new ComplaintMessage
        {
            ComplaintId = complaint.Id,
            SenderId = userId,
            Content = command.Content.Trim(),
            IsInternal = command.IsInternal
        };
        message.InitializeEntity(userId);

        await _unitOfWork.Repository<ComplaintMessage>().AddAsync(message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(message.Id, "Message sent.");
    }
}


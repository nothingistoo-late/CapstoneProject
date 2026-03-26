using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Commands.SendComplaintMessage;

public class SendComplaintMessageCommandHandler : IRequestHandler<SendComplaintMessageCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SendComplaintMessageCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(SendComplaintMessageCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<Guid>.Failure("Authentication required. Please log in to send a message.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        if (command.ComplaintId == Guid.Empty)
            return Result<Guid>.Failure("ComplaintId is required.", ErrorCodeEnum.ValidationFailed);
        if (string.IsNullOrWhiteSpace(command.Content))
            return Result<Guid>.Failure("Message content is required.", ErrorCodeEnum.ValidationFailed);

        var complaintRepo = _unitOfWork.Repository<Complaint>();
        var complaint = await complaintRepo.GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == command.ComplaintId && !c.IsDeleted, cancellationToken);
        if (complaint == null)
            return Result<Guid>.Failure($"Complaint not found with Id: {command.ComplaintId}.", ErrorCodeEnum.NotFound);
        if (complaint.UserId != userId)
            return Result<Guid>.Failure("You do not have permission to send messages for this complaint.", ErrorCodeEnum.Forbidden);
        if (complaint.ComplaintStatus == ComplaintStatusEnum.Resolved)
            return Result<Guid>.Failure("This complaint is already resolved. You cannot send new messages.", ErrorCodeEnum.Forbidden);

        var message = new ComplaintMessage
        {
            ComplaintId = complaint.Id,
            SenderId = userId,
            Content = command.Content.Trim(),
            IsInternal = false
        };
        message.InitializeEntity(userId);

        await _unitOfWork.Repository<ComplaintMessage>().AddAsync(message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(message.Id, "Message sent.");
    }
}


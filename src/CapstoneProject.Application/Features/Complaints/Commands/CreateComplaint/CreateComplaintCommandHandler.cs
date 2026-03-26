using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Commands.CreateComplaint;

public class CreateComplaintCommandHandler : IRequestHandler<CreateComplaintCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateComplaintCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateComplaintCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<Guid>.Failure("Authentication required. Please log in to submit a complaint.", ErrorCodeEnum.Unauthorized);

        var userId = userIdNullable.Value;
        if (string.IsNullOrWhiteSpace(command.Subject))
            return Result<Guid>.Failure("Subject is required.", ErrorCodeEnum.ValidationFailed);
        if (string.IsNullOrWhiteSpace(command.Category))
            return Result<Guid>.Failure("Category is required.", ErrorCodeEnum.ValidationFailed);
        if (string.IsNullOrWhiteSpace(command.Description))
            return Result<Guid>.Failure("Description is required.", ErrorCodeEnum.ValidationFailed);

        var complaint = new Complaint
        {
            UserId = userId,
            Subject = command.Subject.Trim(),
            Category = command.Category.Trim(),
            Description = command.Description.Trim(),
            ComplaintStatus = ComplaintStatusEnum.Open
        };
        complaint.InitializeEntity(userId);

        var firstMessage = new ComplaintMessage
        {
            ComplaintId = complaint.Id,
            SenderId = userId,
            Content = complaint.Description,
            IsInternal = false
        };
        firstMessage.InitializeEntity(userId);

        await _unitOfWork.Repository<Complaint>().AddAsync(complaint);
        await _unitOfWork.Repository<ComplaintMessage>().AddAsync(firstMessage);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(complaint.Id, "Complaint submitted.");
    }
}


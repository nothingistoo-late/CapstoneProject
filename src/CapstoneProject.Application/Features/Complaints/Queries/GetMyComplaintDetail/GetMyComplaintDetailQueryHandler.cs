using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetMyComplaintDetail;

public class GetMyComplaintDetailQueryHandler : IRequestHandler<GetMyComplaintDetailQuery, Result<MyComplaintDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyComplaintDetailQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MyComplaintDetailDto>> Handle(GetMyComplaintDetailQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<MyComplaintDetailDto>.Failure("Authentication required. Please log in to view complaint detail.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        if (request.ComplaintId == Guid.Empty)
            return Result<MyComplaintDetailDto>.Failure("ComplaintId is required.", ErrorCodeEnum.ValidationFailed);

        var complaint = await _unitOfWork.Repository<Complaint>().GetQueryable()
            .Include(c => c.Messages)
            .Include(c => c.StatusHistories)
            .FirstOrDefaultAsync(c => c.Id == request.ComplaintId && !c.IsDeleted, cancellationToken);

        if (complaint == null)
            return Result<MyComplaintDetailDto>.Failure($"Complaint not found with Id: {request.ComplaintId}.", ErrorCodeEnum.NotFound);
        if (complaint.UserId != userId)
            return Result<MyComplaintDetailDto>.Failure("You do not have permission to view this complaint.", ErrorCodeEnum.Forbidden);

        var dto = new MyComplaintDetailDto
        {
            Id = complaint.Id,
            Subject = complaint.Subject,
            Category = complaint.Category,
            Description = complaint.Description,
            ComplaintStatus = complaint.ComplaintStatus.ToString(),
            CreatedAt = complaint.CreatedAt,
            ResolvedAt = complaint.ResolvedAt,
            Messages = complaint.Messages
                .Where(m => !m.IsDeleted && !m.IsInternal)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MyComplaintMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    Content = m.Content,
                    IsInternal = m.IsInternal,
                    CreatedAt = m.CreatedAt
                })
                .ToList(),
            StatusHistories = complaint.StatusHistories
                .Where(h => !h.IsDeleted)
                .OrderBy(h => h.ChangedAt)
                .Select(h => new MyComplaintStatusHistoryDto
                {
                    Id = h.Id,
                    FromStatus = h.FromStatus.ToString(),
                    ToStatus = h.ToStatus.ToString(),
                    ChangedBy = h.ChangedBy,
                    ChangedAt = h.ChangedAt,
                    Note = h.Note
                })
                .ToList()
        };

        return Result<MyComplaintDetailDto>.Success(dto);
    }
}


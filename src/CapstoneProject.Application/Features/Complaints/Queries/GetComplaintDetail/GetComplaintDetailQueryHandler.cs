using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetComplaintDetail;

public class GetComplaintDetailQueryHandler : IRequestHandler<GetComplaintDetailQuery, Result<ComplaintDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetComplaintDetailQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ComplaintDetailDto>> Handle(GetComplaintDetailQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<ComplaintDetailDto>.Failure("Authentication required. Please log in to view complaint detail.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<ComplaintDetailDto>.Failure("You do not have permission to view complaint detail. Only Admin or Moderator can access.", ErrorCodeEnum.Forbidden);

        if (request.ComplaintId == Guid.Empty)
            return Result<ComplaintDetailDto>.Failure("ComplaintId is required.", ErrorCodeEnum.ValidationFailed);

        var complaint = await _unitOfWork.Repository<Complaint>().GetQueryable()
            .Include(c => c.Messages)
            .Include(c => c.StatusHistories)
            .FirstOrDefaultAsync(c => c.Id == request.ComplaintId && !c.IsDeleted, cancellationToken);

        if (complaint == null)
            return Result<ComplaintDetailDto>.Failure($"Complaint not found with Id: {request.ComplaintId}.", ErrorCodeEnum.NotFound);

        var dto = new ComplaintDetailDto
        {
            Id = complaint.Id,
            UserId = complaint.UserId,
            Subject = complaint.Subject,
            Category = complaint.Category,
            Description = complaint.Description,
            ComplaintStatus = complaint.ComplaintStatus.ToString(),
            CreatedAt = complaint.CreatedAt,
            ResolvedAt = complaint.ResolvedAt,
            Messages = complaint.Messages
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ComplaintMessageDto
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
                .Select(h => new ComplaintStatusHistoryDto
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

        return Result<ComplaintDetailDto>.Success(dto);
    }
}


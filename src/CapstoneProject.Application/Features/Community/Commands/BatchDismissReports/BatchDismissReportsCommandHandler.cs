using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Community;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Community.Commands.BatchDismissReports;

public class BatchDismissReportsCommandHandler : IRequestHandler<BatchDismissReportsCommand, Result<BatchReportResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public BatchDismissReportsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<BatchReportResultDto>> Handle(BatchDismissReportsCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<BatchReportResultDto>.Failure("Authentication required. Please log in to dismiss reports.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<BatchReportResultDto>.Failure("You do not have permission to dismiss reports. Only Admin or Moderator can perform batch dismiss.", ErrorCodeEnum.Forbidden);

        var repo = _unitOfWork.Repository<MapReport>();
        var reports = await repo.GetQueryable()
            .Where(r => command.ReportIds.Contains(r.Id) && !r.IsDeleted)
            .ToListAsync(cancellationToken);
        var foundIds = reports.Select(r => r.Id).ToHashSet();
        var notFoundIds = command.ReportIds.Where(id => !foundIds.Contains(id)).ToList();

        foreach (var r in reports)
        {
            r.ReportStatus = ReportStatusEnum.Dismissed;
            r.ReviewedBy = userIdNullable!.Value;
            r.ReviewedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
            r.ReviewNote = command.ReviewNote;
            r.UpdateEntity(userIdNullable.Value);
            repo.Update(r);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new BatchReportResultDto { SuccessCount = reports.Count, FailedCount = notFoundIds.Count, NotFoundIds = notFoundIds };
        return Result<BatchReportResultDto>.Success(dto, $"Dismissed {dto.SuccessCount} report(s).");
    }
}




using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Community.Commands.BatchResolveReports;

public class BatchResolveReportsCommandHandler : IRequestHandler<BatchResolveReportsCommand, Result<BatchReportResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public BatchResolveReportsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<BatchReportResultDto>> Handle(BatchResolveReportsCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<BatchReportResultDto>.Failure("Authentication required. Please log in to resolve reports.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<BatchReportResultDto>.Failure("You do not have permission to resolve reports. Only Admin or Moderator can perform batch resolve.", ErrorCodeEnum.Forbidden);

        var repo = _unitOfWork.Repository<ChallengeReport>();
        var reports = await repo.GetQueryable()
            .Where(r => command.ReportIds.Contains(r.Id) && !r.IsDeleted)
            .ToListAsync(cancellationToken);
        var foundIds = reports.Select(r => r.Id).ToHashSet();
        var notFoundIds = command.ReportIds.Where(id => !foundIds.Contains(id)).ToList();

        foreach (var r in reports)
        {
            r.ReportStatus = ReportStatusEnum.Resolved;
            r.UpdateEntity(userIdNullable.Value);
            repo.Update(r);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new BatchReportResultDto { SuccessCount = reports.Count, FailedCount = notFoundIds.Count, NotFoundIds = notFoundIds };
        return Result<BatchReportResultDto>.Success(dto, $"Resolved {dto.SuccessCount} report(s).");
    }
}

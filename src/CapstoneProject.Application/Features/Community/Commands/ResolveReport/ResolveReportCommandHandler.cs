using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Community.Commands.ResolveReport;

public class ResolveReportCommandHandler : IRequestHandler<ResolveReportCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ResolveReportCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ResolveReportCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Authentication required. Please log in to resolve a report.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result.Failure("You do not have permission to resolve reports. Only Admin or Moderator can perform this action.", ErrorCodeEnum.Forbidden);

        var report = await _unitOfWork.Repository<MapReport>().GetQueryable().FirstOrDefaultAsync(r => r.Id == command.ReportId && !r.IsDeleted, cancellationToken);
        if (report == null)
            return Result.Failure($"Report not found with Id: {command.ReportId}. The report may have been deleted or does not exist.", ErrorCodeEnum.NotFound);

        report.ReportStatus = ReportStatusEnum.Resolved;
        report.ReviewedBy = userIdNullable!.Value;
        report.ReviewedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now;
        report.ReviewNote = command.ReviewNote;
        report.UpdateEntity(userIdNullable.Value);
        _unitOfWork.Repository<MapReport>().Update(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Report resolved.");
    }
}


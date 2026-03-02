using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Community.Commands.DismissReport;

public class DismissReportCommandHandler : IRequestHandler<DismissReportCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DismissReportCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DismissReportCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Authentication required. Please log in to dismiss a report.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result.Failure("You do not have permission to dismiss reports. Only Admin or Moderator can perform this action.", ErrorCodeEnum.Forbidden);

        var report = await _unitOfWork.Repository<ChallengeReport>().GetQueryable().FirstOrDefaultAsync(r => r.Id == command.ReportId && !r.IsDeleted, cancellationToken);
        if (report == null)
            return Result.Failure($"Report not found with Id: {command.ReportId}. The report may have been deleted or does not exist.", ErrorCodeEnum.NotFound);

        report.ReportStatus = ReportStatusEnum.Dismissed;
        report.ReviewedBy = userIdNullable!.Value;
        report.ReviewedAt = DateTime.UtcNow;
        report.ReviewNote = command.ReviewNote;
        report.UpdateEntity(userIdNullable.Value);
        _unitOfWork.Repository<ChallengeReport>().Update(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Report dismissed.");
    }
}

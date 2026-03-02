using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Community.Commands.ReportMap;

public class ReportMapCommandHandler : IRequestHandler<ReportMapCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ReportMapCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(ReportMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<Guid>.Failure("Authentication required. Please log in to report a map.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;
        if (string.IsNullOrWhiteSpace(command.Reason))
            return Result<Guid>.Failure("Report reason is required. Please provide a reason for reporting this content.", ErrorCodeEnum.ValidationFailed);

        var mapExists = await _unitOfWork.Repository<Map>().GetQueryable().AnyAsync(m => m.Id == command.MapId && !m.IsDeleted, cancellationToken);
        if (!mapExists)
            return Result<Guid>.Failure($"Map not found with Id: {command.MapId}. The map may have been deleted or does not exist.", ErrorCodeEnum.NotFound);

        var report = new ChallengeReport
        {
            UserId = userId,
            MapId = command.MapId,
            Reason = command.Reason,
            Details = command.Details,
            ReportStatus = ReportStatusEnum.Pending
        };
        report.InitializeEntity(userId);
        await _unitOfWork.Repository<ChallengeReport>().AddAsync(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(report.Id, "Report submitted.");
    }
}

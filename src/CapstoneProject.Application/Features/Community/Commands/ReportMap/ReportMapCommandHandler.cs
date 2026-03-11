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

        var mapRepo = _unitOfWork.Repository<Map>();
        var map = await mapRepo.GetQueryable()
            .FirstOrDefaultAsync(g => g.Id == command.MapId && !g.IsDeleted && g.Status == EntityStatusEnum.Active, cancellationToken);
        if (map == null)
            return Result<Guid>.Failure($"Map not found with Id: {command.MapId}. The map may have been deleted or does not exist.", ErrorCodeEnum.NotFound);
        if (map.CreatedBy.HasValue && map.CreatedBy.Value == userId)
            return Result<Guid>.Failure("You cannot report your own map.", ErrorCodeEnum.Forbidden);

        // Only allow reporting maps the user can actually play:
        // - Free maps (Price null or <= 0)
        // - OR paid maps that the user has already purchased (PaymentRecord Completed for this map)
        var isFreeMap = !map.Price.HasValue || map.Price <= 0;
        if (!isFreeMap)
        {
            var paymentRepo = _unitOfWork.Repository<PaymentRecord>();
            var hasPurchased = await paymentRepo.GetQueryable()
                .AnyAsync(p => !p.IsDeleted
                               && p.UserId == userId
                               && p.MapId == map.Id
                               && p.PaymentStatus == PaymentStatusEnum.Completed,
                    cancellationToken);
            if (!hasPurchased)
                return Result<Guid>.Failure("You can only report maps you have access to (free maps or maps you have purchased).", ErrorCodeEnum.Forbidden);
        }

        var report = new MapReport
        {
            UserId = userId,
            MapId = command.MapId,
            Reason = command.Reason,
            Details = command.Details,
            ReportStatus = ReportStatusEnum.Pending
        };
        report.InitializeEntity(userId);
        await _unitOfWork.Repository<MapReport>().AddAsync(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(report.Id, "Report submitted.");
    }
}

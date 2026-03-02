using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Challenge.Commands.RejectMap;

public class RejectMapCommandHandler : IRequestHandler<RejectMapCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RejectMapCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(RejectMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Authentication required. Please log in to perform this action.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result.Failure("You do not have permission to reject maps. Only Admin or Moderator can perform this action.", ErrorCodeEnum.Forbidden);

        var mapRepo = _unitOfWork.Repository<Map>();
        var map = await mapRepo.GetQueryable().FirstOrDefaultAsync(m => m.Id == command.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result.Failure($"Map not found with Id: {command.MapId}. The map may have been deleted or does not exist.", ErrorCodeEnum.NotFound);
        if (map.MapStatus != MapStatusEnum.PendingReview)
            return Result.Failure($"Map cannot be rejected. Expected status: PendingReview. Current status: {map.MapStatus}. Only maps awaiting review can be rejected.", ErrorCodeEnum.InvalidOperation);

        map.MapStatus = MapStatusEnum.Rejected;
        map.UpdateEntity(userIdNullable!.Value);
        mapRepo.Update(map);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Map rejected successfully.");
    }
}

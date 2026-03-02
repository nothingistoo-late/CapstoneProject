using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Challenge.Commands.SubmitMapForReview;

public class SubmitMapForReviewCommandHandler : IRequestHandler<SubmitMapForReviewCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SubmitMapForReviewCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(SubmitMapForReviewCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Authentication required. Please log in to submit a map for review.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var mapRepo = _unitOfWork.Repository<Map>();
        var map = await mapRepo.GetQueryable().FirstOrDefaultAsync(m => m.Id == command.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result.Failure($"Map not found with Id: {command.MapId}. The map may have been deleted or does not exist.", ErrorCodeEnum.NotFound);
        if (map.CreatedByUserId != userId)
            return Result.Failure("You can only submit your own maps for review. This map was created by another user.", ErrorCodeEnum.Forbidden);
        if (map.MapStatus != MapStatusEnum.Draft)
            return Result.Failure($"Map cannot be submitted for review. Expected status: Draft. Current status: {map.MapStatus}. Only draft maps can be submitted.", ErrorCodeEnum.InvalidOperation);

        map.MapStatus = MapStatusEnum.PendingReview;
        map.UpdateEntity(userId);
        mapRepo.Update(map);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Map submitted for review successfully.");
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Community.Commands.RateMap;

public class RateMapCommandHandler : IRequestHandler<RateMapCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RateMapCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(RateMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Authentication required. Please log in to rate a map.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;
        if (command.Rating < 1 || command.Rating > 5)
            return Result.Failure("Rating must be between 1 and 5 stars. Please provide a valid rating.", ErrorCodeEnum.ValidationFailed);

        var mapExists = await _unitOfWork.Repository<Map>().GetQueryable().AnyAsync(g => g.Id == command.MapId && !g.IsDeleted, cancellationToken);
        if (!mapExists)
            return Result.Failure($"Map not found with Id: {command.MapId}. The map may have been deleted or does not exist.", ErrorCodeEnum.NotFound);

        var repo = _unitOfWork.Repository<MapRating>();
        var existing = await repo.GetQueryable().FirstOrDefaultAsync(r => r.UserId == userId && r.MapId == command.MapId && !r.IsDeleted, cancellationToken);
        if (existing != null)
        {
            existing.Rating = command.Rating;
            existing.Comment = command.Comment;
            existing.UpdateEntity(userId);
            repo.Update(existing);
        }
        else
        {
            var rating = new MapRating { UserId = userId, MapId = command.MapId, Rating = command.Rating, Comment = command.Comment };
            rating.InitializeEntity(userId);
            await repo.AddAsync(rating);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Rating saved.");
    }
}

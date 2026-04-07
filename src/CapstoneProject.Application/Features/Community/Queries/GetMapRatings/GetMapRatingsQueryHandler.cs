using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Community.Queries.GetMapRatings;

public class GetMapRatingsQueryHandler : IRequestHandler<GetMapRatingsQuery, Result<List<MapRatingDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMapRatingsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<MapRatingDto>>> Handle(GetMapRatingsQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<List<MapRatingDto>>.Failure("Authentication required. Please log in to view map ratings.", ErrorCodeEnum.Unauthorized);
        var currentUserId = userIdNullable.Value;

        var mapExists = await _unitOfWork.Repository<Map>().GetQueryable()
            .AnyAsync(m => m.Id == request.MapId && !m.IsDeleted, cancellationToken);
        if (!mapExists)
            return Result<List<MapRatingDto>>.Failure($"Không tìm thấy bản đồ có Id: {request.MapId}.", ErrorCodeEnum.NotFound);

        var ratingRepo = _unitOfWork.Repository<MapRating>();
        var ratingsQuery = ratingRepo.GetQueryable()
            .Where(r => !r.IsDeleted && r.MapId == request.MapId);

        if (request.IsAuthorOnly)
            ratingsQuery = ratingsQuery.Where(r => r.UserId == currentUserId);

        var list = await ratingsQuery
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new MapRatingDto
            {
                Id = r.Id,
                UserId = r.UserId,
                MapId = r.MapId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                IsAuthor = r.UserId == currentUserId
            })
            .ToListAsync(cancellationToken);

        return Result<List<MapRatingDto>>.Success(list);
    }
}


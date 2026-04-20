using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Community.Queries.GetGameRatings;

public class GetGameRatingsQueryHandler : IRequestHandler<GetGameRatingsQuery, Result<List<GameRatingDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetGameRatingsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<GameRatingDto>>> Handle(GetGameRatingsQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<List<GameRatingDto>>.Failure("Authentication required. Please log in to view game ratings.", ErrorCodeEnum.Unauthorized);
        var currentUserId = userIdNullable.Value;

        var mapExists = await _unitOfWork.Repository<Game>().GetQueryable()
            .AnyAsync(m => m.Id == request.GameId && !m.IsDeleted, cancellationToken);
        if (!mapExists)
            return Result<List<GameRatingDto>>.Failure($"Không tìm thấy bản đồ có Id: {request.GameId}.", ErrorCodeEnum.NotFound);

        var ratingRepo = _unitOfWork.Repository<GameRating>();
        var ratingsQuery = ratingRepo.GetQueryable()
            .Where(r => !r.IsDeleted && r.GameId == request.GameId);

        if (request.IsAuthorOnly)
            ratingsQuery = ratingsQuery.Where(r => r.UserId == currentUserId);

        var rawRatings = await ratingsQuery
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                Id = r.Id,
                UserId = r.UserId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                IsAuthor = r.UserId == currentUserId
            })
            .ToListAsync(cancellationToken);

        var userIds = rawRatings.Select(r => r.UserId).Distinct().ToList();
        var userNameMap = await _unitOfWork.Repository<AppUser>().GetQueryable()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.UserName })
            .ToDictionaryAsync(
                u => u.Id,
                u =>
                {
                    var fullName = $"{u.FirstName} {u.LastName}".Trim();
                    if (!string.IsNullOrWhiteSpace(fullName)) return fullName;
                    if (!string.IsNullOrWhiteSpace(u.UserName)) return u.UserName!;
                    return "Player";
                },
                cancellationToken);

        var list = rawRatings.Select(r => new GameRatingDto
        {
            Id = r.Id,
            ReviewerName = userNameMap.TryGetValue(r.UserId, out var reviewerName) ? reviewerName : "Player",
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            IsAuthor = r.IsAuthor
        }).ToList();

        return Result<List<GameRatingDto>>.Success(list, "Đã lấy danh sách đánh giá bản đồ.");
    }
}



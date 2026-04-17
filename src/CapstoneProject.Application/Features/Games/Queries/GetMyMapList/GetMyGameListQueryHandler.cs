using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Games.Queries.GetMyGameList;

public class GetMyGameListQueryHandler : IRequestHandler<GetMyGameListQuery, Result<PaginationResult<MapListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyGameListQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<MapListItemDto>>> Handle(GetMyGameListQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<PaginationResult<MapListItemDto>>.Failure("Authentication required. Please log in to view your games.", ErrorCodeEnum.Unauthorized);

        var myMapRepo = _unitOfWork.Repository<MyGame>();
        var query = myMapRepo.GetQueryable()
            .Where(mm => !mm.IsDeleted && mm.UserId == userId.Value);

        if (request.IsAuthor.HasValue)
            query = query.Where(mm => mm.IsAuthor == request.IsAuthor.Value);

        query = query.Include(mm => mm.Game)
            .ThenInclude(m => m!.GameTags)
            .ThenInclude(mt => mt.Tag)
            .Include(mm => mm.Game)
            .ThenInclude(m => m!.GameDetails)
            .Include(mm => mm.Game)
            .ThenInclude(m => m!.GameMedias)
            .Include(mm => mm.Game)
            .ThenInclude(m => m!.Creator)
            .AsNoTracking();

        query = query.Where(mm => mm.Game != null && mm.Game.Status == EntityStatusEnum.Active && (!mm.Game.IsDeleted || !mm.IsAuthor));

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var sortBy = (request.SortBy ?? "CreatedAt").ToLowerInvariant();

        query = sortBy switch
        {
            "title" => request.SortAscending ? query.OrderBy(mm => mm.Game!.Title) : query.OrderByDescending(mm => mm.Game!.Title),
            "difficulty" => request.SortAscending ? query.OrderBy(mm => mm.Game!.Difficulty) : query.OrderByDescending(mm => mm.Game!.Difficulty),
            "timelimitms" => request.SortAscending
                ? query.OrderBy(mm => mm.Game!.GameDetails.Where(d => !d.IsDeleted).OrderBy(d => d.LevelOrder).Select(d => d.TimeLimitMs).FirstOrDefault())
                : query.OrderByDescending(mm => mm.Game!.GameDetails.Where(d => !d.IsDeleted).OrderBy(d => d.LevelOrder).Select(d => d.TimeLimitMs).FirstOrDefault()),
            _ => request.SortAscending ? query.OrderBy(mm => mm.Game!.CreatedAt) : query.OrderByDescending(mm => mm.Game!.CreatedAt)
        };

        var page = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var learnedTagIds = page
            .Where(mm => mm.Game != null)
            .SelectMany(mm => mm.Game!.LearnedTags)
            .Distinct()
            .ToList();
        var learnedTagNameMap = await _unitOfWork.Repository<Tag>().GetQueryable()
            .Where(t => learnedTagIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name, cancellationToken);

        var list = page.Where(mm => mm.Game != null).Select(mm =>
        {
            var m = mm.Game!;
            var (tLimit, win, mapType) = MapFirstLevelHelper.FirstLevelMetadata(m.GameDetails);
            return new MapListItemDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                Difficulty = m.Difficulty,
                Type = mapType.ToString(),
                TimeLimitMs = tLimit,
                IsPublished = m.IsPublished,
                GameStatus = m.GameStatus.ToString(),
                Price = m.Price,
                CreatedByUserId = m.CreatedBy ?? Guid.Empty,
                CreatedByUserName = m.Creator != null ? $"{m.Creator.FirstName} {m.Creator.LastName}".Trim() : null,
                IsAuthor = mm.IsAuthor,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                ContentVersion = m.ContentVersion,
                TagNames = m.GameTags.Select(t => t.Tag.Name).ToList(),
                LearnedTags = m.LearnedTags
                    .Select(id => learnedTagNameMap.TryGetValue(id, out var name) ? name : id.ToString())
                    .ToList(),
                WinCondition = win,
                AvatarUrl = m.AvatarUrl,
                Gallery = m.GameMedias
                    .OrderBy(media => media.SortOrder)
                    .Select(media => new GameMediaItemDto
                    {
                        Id = media.Id,
                        Url = media.Url,
                        Kind = media.Kind.ToString(),
                        SortOrder = media.SortOrder
                    })
                    .ToList()
            };
        }).ToList();

        var result = PaginationResult<MapListItemDto>.Success(list, pageNumber, pageSize, total, "Đã truy xuất thành công");
        return Result<PaginationResult<MapListItemDto>>.Success(result, "Đã lấy danh sách bản đồ của bạn.");
    }
}


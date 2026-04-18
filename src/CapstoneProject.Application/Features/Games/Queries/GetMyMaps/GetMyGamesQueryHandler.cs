using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Games.Queries.GetMyGames;

public class GetMyGamesQueryHandler : IRequestHandler<GetMyGamesQuery, Result<PaginationResult<MapListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyGamesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<MapListItemDto>>> Handle(GetMyGamesQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<PaginationResult<MapListItemDto>>.Failure("Authentication required. Please log in to view your games.", ErrorCodeEnum.Unauthorized);

        var mapRepo = _unitOfWork.Repository<Game>();
        var myMapRepo = _unitOfWork.Repository<MyGame>();
        var paymentRepo = _unitOfWork.Repository<PaymentRecord>();

        // Game IDs tá»« báº£ng MyGame (nguá»“n chÃ­nh: táº¡o game, mua game, thÃªm game free)
        var myGameIds = await myMapRepo.GetQueryable()
            .Where(mm => !mm.IsDeleted && mm.UserId == userId.Value)
            .Select(mm => mm.GameId)
            .ToListAsync(cancellationToken);

        // Backward compat: game do user táº¡o hoáº·c Ä‘Ã£ mua (trÆ°á»›c khi cÃ³ báº£ng MyGame)
        var createdGameIds = await mapRepo.GetQueryable()
            .Where(m => !m.IsDeleted && m.Status == EntityStatusEnum.Active && m.CreatedBy == userId.Value)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);
        var purchasedGameIds = await paymentRepo.GetQueryable()
            .Where(p => !p.IsDeleted
                        && p.UserId == userId.Value
                        && p.GameId != null
                        && p.PaymentStatus == PaymentStatusEnum.Completed)
            .Select(p => p.GameId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var allOwnedIds = myGameIds.Union(createdGameIds).Union(purchasedGameIds).Distinct().ToList();
        if (allOwnedIds.Count == 0)
        {
            var empty = PaginationResult<MapListItemDto>.Success(new List<MapListItemDto>(), 1, request.PageSize, 0, "Đã truy xuất thành công");
            return Result<PaginationResult<MapListItemDto>>.Success(empty, "Đã lấy danh sách bản đồ của bạn.");
        }

        var ownedRootGameIds = await mapRepo.GetQueryable()
            .Where(m => m.Status == EntityStatusEnum.Active && allOwnedIds.Contains(m.Id))
            .Select(m => m.RootGameId ?? m.Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        var query = mapRepo.GetQueryable()
            .Where(m => m.Status == EntityStatusEnum.Active && !m.IsDeleted)
            .Include(m => m.GameDetails)
            .Include(m => m.GameTags).ThenInclude(mt => mt.Tag)
            .Include(m => m.GameMedias)
            .Include(m => m.Creator)
            .AsNoTracking();

        if (request.IsAuthorOnly)
        {
            var latestAuthorGameIds = await mapRepo.GetQueryable()
                .Where(m => m.Status == EntityStatusEnum.Active && !m.IsDeleted && m.CreatedBy == userId.Value)
                .GroupBy(m => m.RootGameId ?? m.Id)
                .Select(g => g
                    .OrderByDescending(m => m.ContentVersion)
                    .ThenByDescending(m => m.CreatedAt)
                    .Select(m => m.Id)
                    .First())
                .ToListAsync(cancellationToken);

            query = query.Where(m => latestAuthorGameIds.Contains(m.Id));
        }
        else
            query = query.Where(m => m.IsActiveVersion && ownedRootGameIds.Contains(m.RootGameId ?? m.Id));

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var sortBy = (request.SortBy ?? "CreatedAt").ToLowerInvariant();
        query = sortBy switch
        {
            "title" => request.SortAscending ? query.OrderBy(m => m.Title) : query.OrderByDescending(m => m.Title),
            "difficulty" => request.SortAscending ? query.OrderBy(m => m.Difficulty) : query.OrderByDescending(m => m.Difficulty),
            "timelimitms" => request.SortAscending
                ? query.OrderBy(m => m.GameDetails.Where(d => !d.IsDeleted).OrderBy(d => d.LevelOrder).Select(d => d.TimeLimitMs).FirstOrDefault())
                : query.OrderByDescending(m => m.GameDetails.Where(d => !d.IsDeleted).OrderBy(d => d.LevelOrder).Select(d => d.TimeLimitMs).FirstOrDefault()),
            _ => request.SortAscending ? query.OrderBy(m => m.CreatedAt) : query.OrderByDescending(m => m.CreatedAt)
        };

        var page = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var learnedTagIds = page.SelectMany(m => m.LearnedTags).Distinct().ToList();
        var learnedTagNameMap = await _unitOfWork.Repository<Tag>().GetQueryable()
            .Where(t => learnedTagIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name, cancellationToken);

        var list = page.Select(m =>
        {
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
            ReviewNote = m.ReviewNote,
            CreatedByUserId = m.CreatedBy ?? Guid.Empty,
            CreatedByUserName = m.Creator != null ? $"{m.Creator.FirstName} {m.Creator.LastName}".Trim() : null,
            // IsAuthor = user Ä‘ang gá»­i request cÃ³ pháº£i lÃ  ngÆ°á»i táº¡o game (CreatedBy), khÃ´ng pháº£i kiá»ƒm tra sá»Ÿ há»¯u
            IsAuthor = m.CreatedBy == userId.Value,
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


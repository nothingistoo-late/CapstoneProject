using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Games.Queries.GetMapById;

public class GetMapByIdQueryHandler : IRequestHandler<GetMapByIdQuery, Result<GameDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMapByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GameDetailDto>> Handle(GetMapByIdQuery request, CancellationToken cancellationToken)
    {
        var mapRepo = _unitOfWork.Repository<Game>();
        var requestedMap = await mapRepo.GetQueryable()
            .Where(m => m.Id == request.GameId
                        && (request.IncludeInactive || m.Status == EntityStatusEnum.Active))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
        if (requestedMap == null)
            return Result<GameDetailDto>.Failure($"Không tìm thấy bản đồ có Id: {request.GameId}.", ErrorCodeEnum.NotFound);

        var rootGameId = requestedMap.RootGameId ?? requestedMap.Id;
        var resolvedGameId = requestedMap.Id;
        if (requestedMap.IsDeleted || !requestedMap.IsActiveVersion)
        {
            var activeGameId = await mapRepo.GetQueryable()
                .Where(m => !m.IsDeleted
                            && m.Status == EntityStatusEnum.Active
                            && (m.RootGameId ?? m.Id) == rootGameId
                            && m.IsActiveVersion)
                .OrderByDescending(m => m.ContentVersion)
                .Select(m => (Guid?)m.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (activeGameId.HasValue)
                resolvedGameId = activeGameId.Value;
        }

        var game = await mapRepo.GetQueryable()
            .Where(m => m.Id == resolvedGameId
                        && (request.IncludeInactive || m.Status == EntityStatusEnum.Active))
            .Include(m => m.GameDetails).ThenInclude(d => d.Hints)
            .Include(m => m.GameMedias)
            .Include(m => m.GameTags).ThenInclude(mt => mt.Tag)
            .Include(m => m.Creator)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
        if (game == null)
            return Result<GameDetailDto>.Failure($"Không tìm thấy bản đồ có Id: {request.GameId}.", ErrorCodeEnum.NotFound);

        if (game.IsDeleted)
        {
            var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
            if (!isValid || !userIdNullable.HasValue)
                return Result<GameDetailDto>.Failure($"Không tìm thấy bản đồ có Id: {request.GameId}.", ErrorCodeEnum.NotFound);

            var userId = userIdNullable.Value;
            var isAuthor = game.CreatedBy.HasValue && game.CreatedBy.Value == userId;
            var isOwned = isAuthor;
            if (!isOwned)
            {
                var purchased = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                    .AnyAsync(p => !p.IsDeleted && p.UserId == userId && p.GameId == game.Id && p.PaymentStatus == PaymentStatusEnum.Completed, cancellationToken);
                if (purchased)
                    isOwned = true;
                else
                    isOwned = await _unitOfWork.Repository<MyGame>().GetQueryable()
                        .AnyAsync(mm => !mm.IsDeleted && mm.UserId == userId && mm.GameId == game.Id, cancellationToken);
            }

            if (!isOwned)
                return Result<GameDetailDto>.Failure($"Không tìm thấy bản đồ có Id: {request.GameId}.", ErrorCodeEnum.NotFound);
        }

        var learnedTagNameMap = await _unitOfWork.Repository<Tag>().GetQueryable()
            .Where(t => game.LearnedTags.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name, cancellationToken);

        bool showEditorial = false;
        if (request.IncludeEditorialForUser)
        {
            var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
            if (isValid && userIdNullable.HasValue)
            {
                showEditorial = await MeetsEditorialStarsAsync(
                    userIdNullable.Value, game, cancellationToken);
            }
        }

        var levelsOrdered = game.GameDetails.OrderBy(d => d.LevelOrder).ToList();
        var levelDtos = levelsOrdered.Select(d => new MapLevelItemDto
        {
            Id = d.Id,
            LevelOrder = d.LevelOrder,
            Title = d.Title,
            DetailJson = ParseGameDetailJson(d.JsonContent),
            Hints = d.Hints.OrderBy(h => h.OrderNo).Select(h => new HintItemDto { OrderNo = h.OrderNo, Content = h.Content }).ToList(),
            TimeLimitMs = d.TimeLimitMs,
            WinCondition = d.WinCondition,
            Type = d.Type.ToString()
        }).ToList();
        var firstJson = levelDtos.FirstOrDefault()?.DetailJson;

        var flatOrder = 0;
        var flatHints = new List<HintItemDto>();
        foreach (var d in levelsOrdered)
        {
            foreach (var h in d.Hints.OrderBy(x => x.OrderNo))
                flatHints.Add(new HintItemDto { OrderNo = flatOrder++, Content = h.Content });
        }

        var dto = new GameDetailDto
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            Difficulty = game.Difficulty,
            IsPublished = game.IsPublished,
            GameStatus = game.GameStatus.ToString(),
            Price = game.Price,
            FreeTrialAttemptLimit = game.FreeTrialAttemptLimit,
            ReviewNote = game.ReviewNote,
            CreatedByUserId = game.CreatedBy ?? Guid.Empty,
            CreatedByUserName = game.Creator != null ? $"{game.Creator.FirstName} {game.Creator.LastName}".Trim() : null,
            EditorialContent = showEditorial ? game.EditorialContent : null,
            UnlockEditorialAfterStars = game.UnlockEditorialAfterStars,
            CreatedAt = game.CreatedAt,
            UpdatedAt = game.UpdatedAt,
            ContentVersion = game.ContentVersion,
            Levels = levelDtos,
            GameDetailJson = firstJson,
            Hints = flatHints,
            TagNames = game.GameTags.Select(t => t.Tag.Name).ToList(),
            LearnedTags = game.LearnedTags
                .Select(id => learnedTagNameMap.TryGetValue(id, out var name) ? name : id.ToString())
                .ToList(),
            AvatarUrl = game.AvatarUrl,
            Gallery = game.GameMedias.OrderBy(x => x.SortOrder).Select(x => new GameMediaItemDto
            {
                Id = x.Id,
                Url = x.Url,
                Kind = x.Kind.ToString(),
                SortOrder = x.SortOrder
            }).ToList()
        };
        return Result<GameDetailDto>.Success(dto, "Đã lấy chi tiết bản đồ.");
    }

    private async Task<bool> MeetsEditorialStarsAsync(Guid userId, Game game, CancellationToken cancellationToken)
    {
        var threshold = game.UnlockEditorialAfterStars;
        var levels = game.GameDetails.OrderBy(d => d.LevelOrder).ToList();
        if (levels.Count == 0) return false;

        var umrs = await _unitOfWork.Repository<UserGameResult>().GetQueryable()
            .Where(u => u.UserId == userId && u.GameId == game.Id && !u.IsDeleted)
            .ToListAsync(cancellationToken);

        if (umrs.Any(u => u.GameDetailId == null))
        {
            var legacy = umrs.FirstOrDefault(u => u.GameDetailId == null);
            return legacy != null && legacy.BestStars >= threshold && levels.Count <= 1;
        }

        return levels.All(d =>
            umrs.Any(u => u.GameDetailId == d.Id && u.BestStars >= threshold));
    }

    private static JsonElement? ParseGameDetailJson(string? jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent)) return null;
        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            return doc.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }
}

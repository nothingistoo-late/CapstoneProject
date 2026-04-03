using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Queries.GetMapById;

public class GetMapByIdQueryHandler : IRequestHandler<GetMapByIdQuery, Result<MapDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMapByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MapDetailDto>> Handle(GetMapByIdQuery request, CancellationToken cancellationToken)
    {
        var map = await _unitOfWork.Repository<Map>().GetQueryable()
            .Where(m => m.Id == request.MapId && m.Status == EntityStatusEnum.Active)
            .Include(m => m.MapDetails).ThenInclude(d => d.Hints)
            .Include(m => m.MapMedias)
            .Include(m => m.MapTags).ThenInclude(mt => mt.Tag)
            .Include(m => m.Creator)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
        if (map == null)
            return Result<MapDetailDto>.Failure($"Map not found with Id: {request.MapId}.", ErrorCodeEnum.NotFound);

        if (map.IsDeleted)
        {
            var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
            if (!isValid || !userIdNullable.HasValue)
                return Result<MapDetailDto>.Failure($"Map not found with Id: {request.MapId}.", ErrorCodeEnum.NotFound);

            var userId = userIdNullable.Value;
            var isAuthor = map.CreatedBy.HasValue && map.CreatedBy.Value == userId;
            var isOwned = isAuthor;
            if (!isOwned)
            {
                var purchased = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                    .AnyAsync(p => !p.IsDeleted && p.UserId == userId && p.MapId == map.Id && p.PaymentStatus == PaymentStatusEnum.Completed, cancellationToken);
                if (purchased)
                    isOwned = true;
                else
                    isOwned = await _unitOfWork.Repository<MyMap>().GetQueryable()
                        .AnyAsync(mm => !mm.IsDeleted && mm.UserId == userId && mm.MapId == map.Id, cancellationToken);
            }

            if (!isOwned)
                return Result<MapDetailDto>.Failure($"Map not found with Id: {request.MapId}.", ErrorCodeEnum.NotFound);
        }

        var learnedTagNameMap = await _unitOfWork.Repository<Tag>().GetQueryable()
            .Where(t => map.LearnedTags.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name, cancellationToken);

        bool showEditorial = false;
        if (request.IncludeEditorialForUser)
        {
            var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
            if (isValid && userIdNullable.HasValue)
            {
                showEditorial = await MeetsEditorialStarsAsync(
                    userIdNullable.Value, map, cancellationToken);
            }
        }

        var levelsOrdered = map.MapDetails.OrderBy(d => d.LevelOrder).ToList();
        var levelDtos = levelsOrdered.Select(d => new MapLevelItemDto
        {
            Id = d.Id,
            LevelOrder = d.LevelOrder,
            Title = d.Title,
            DetailJson = ParseMapDetailJson(d.JsonContent),
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

        var dto = new MapDetailDto
        {
            Id = map.Id,
            Title = map.Title,
            Description = map.Description,
            Difficulty = map.Difficulty,
            IsPublished = map.IsPublished,
            MapStatus = map.MapStatus.ToString(),
            Price = map.Price,
            CreatedByUserId = map.CreatedBy ?? Guid.Empty,
            CreatedByUserName = map.Creator != null ? $"{map.Creator.FirstName} {map.Creator.LastName}".Trim() : null,
            EditorialContent = showEditorial ? map.EditorialContent : null,
            UnlockEditorialAfterStars = map.UnlockEditorialAfterStars,
            CreatedAt = map.CreatedAt,
            UpdatedAt = map.UpdatedAt,
            ContentVersion = map.ContentVersion,
            Levels = levelDtos,
            MapDetailJson = firstJson,
            Hints = flatHints,
            TagNames = map.MapTags.Select(t => t.Tag.Name).ToList(),
            LearnedTags = map.LearnedTags
                .Select(id => learnedTagNameMap.TryGetValue(id, out var name) ? name : id.ToString())
                .ToList(),
            AvatarUrl = map.AvatarUrl,
            Gallery = map.MapMedias.OrderBy(x => x.SortOrder).Select(x => new MapMediaItemDto
            {
                Id = x.Id,
                Url = x.Url,
                Kind = x.Kind.ToString(),
                SortOrder = x.SortOrder
            }).ToList()
        };
        return Result<MapDetailDto>.Success(dto);
    }

    private async Task<bool> MeetsEditorialStarsAsync(Guid userId, Map map, CancellationToken cancellationToken)
    {
        var threshold = map.UnlockEditorialAfterStars;
        var levels = map.MapDetails.OrderBy(d => d.LevelOrder).ToList();
        if (levels.Count == 0) return false;

        var umrs = await _unitOfWork.Repository<UserMapResult>().GetQueryable()
            .Where(u => u.UserId == userId && u.MapId == map.Id && !u.IsDeleted)
            .ToListAsync(cancellationToken);

        if (umrs.Any(u => u.MapDetailId == null))
        {
            var legacy = umrs.FirstOrDefault(u => u.MapDetailId == null);
            return legacy != null && legacy.BestStars >= threshold && levels.Count <= 1;
        }

        return levels.All(d =>
            umrs.Any(u => u.MapDetailId == d.Id && u.BestStars >= threshold));
    }

    private static JsonElement? ParseMapDetailJson(string? jsonContent)
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

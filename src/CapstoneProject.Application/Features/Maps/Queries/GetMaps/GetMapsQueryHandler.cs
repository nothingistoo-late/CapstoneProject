using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Queries.GetMaps;

public class GetMapsQueryHandler : IRequestHandler<GetMapsQuery, Result<PaginationResult<MapListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetMapsQueryHandler(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

    public async Task<Result<PaginationResult<MapListItemDto>>> Handle(GetMapsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Map>().GetQueryable()
            .Where(m => !m.IsDeleted && m.Status == EntityStatusEnum.Active)
            .Include(m => m.MapDetails)
            .Include(m => m.MapTags).ThenInclude(mt => mt.Tag)
            .Include(m => m.MapMedias)
            .Include(m => m.Creator)
            .AsNoTracking();

        // Status filter: if MapStatus provided use it; else if publishedOnly use Published only
        if (request.MapStatus.HasValue)
            query = query.Where(m => m.MapStatus == request.MapStatus.Value);
        else if (request.PublishedOnly == true)
            query = query.Where(m => m.IsPublished && m.MapStatus == MapStatusEnum.Published);
        if (request.Difficulty.HasValue)
        {
            var d = request.Difficulty.Value;
            query = query.Where(m => m.Difficulty == d);
        }
        if (request.Type.HasValue)
        {
            var mapType = request.Type.Value;
            query = query.Where(m => m.MapDetails.Any(d => !d.IsDeleted && d.Type == mapType));
        }
        if (request.TagId.HasValue) query = query.Where(m => m.MapTags.Any(t => t.TagId == request.TagId.Value));
        if (request.CreatedByUserId.HasValue) query = query.Where(m => m.CreatedBy == request.CreatedByUserId.Value);
        if (request.MinPrice.HasValue) query = query.Where(m => (m.Price ?? 0) >= request.MinPrice.Value);
        if (request.MaxPrice.HasValue) query = query.Where(m => (m.Price ?? 0) <= request.MaxPrice.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(m => (m.Title != null && m.Title.ToLower().Contains(term)) || (m.Description != null && m.Description.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var sortBy = (request.SortBy ?? "CreatedAt").ToLowerInvariant();
        query = sortBy switch
        {
            "title" => request.SortAscending ? query.OrderBy(m => m.Title) : query.OrderByDescending(m => m.Title),
            "difficulty" => request.SortAscending ? query.OrderBy(m => m.Difficulty) : query.OrderByDescending(m => m.Difficulty),
            "timelimitms" => request.SortAscending
                ? query.OrderBy(m => m.MapDetails.Where(d => !d.IsDeleted).OrderBy(d => d.LevelOrder).Select(d => d.TimeLimitMs).FirstOrDefault())
                : query.OrderByDescending(m => m.MapDetails.Where(d => !d.IsDeleted).OrderBy(d => d.LevelOrder).Select(d => d.TimeLimitMs).FirstOrDefault()),
            "price" => request.SortAscending ? query.OrderBy(m => m.Price ?? 0) : query.OrderByDescending(m => m.Price ?? 0),
            _ => request.SortAscending ? query.OrderBy(m => m.CreatedAt) : query.OrderByDescending(m => m.CreatedAt)
        };

        var page = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var learnedTagIds = page.SelectMany(m => m.LearnedTags).Distinct().ToList();
        var learnedTagNameMap = await _unitOfWork.Repository<Tag>().GetQueryable()
            .Where(t => learnedTagIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name, cancellationToken);

        var list = page.Select(m =>
        {
            var (tLimit, win, mapType) = MapFirstLevelHelper.FirstLevelMetadata(m.MapDetails);
            return new MapListItemDto
        {
            Id = m.Id,
            Title = m.Title,
            Description = m.Description,
            Difficulty = m.Difficulty,
            Type = mapType.ToString(),
            TimeLimitMs = tLimit,
            IsPublished = m.IsPublished,
            MapStatus = m.MapStatus.ToString(),
            Price = m.Price,
            FreeTrialAttemptLimit = m.FreeTrialAttemptLimit,
            CreatedByUserId = m.CreatedBy ?? Guid.Empty,
            CreatedByUserName = m.Creator != null ? $"{m.Creator.FirstName} {m.Creator.LastName}".Trim() : null,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt,
            ContentVersion = m.ContentVersion,
            TagNames = m.MapTags.Select(t => t.Tag.Name).ToList(),
            LearnedTags = m.LearnedTags
                .Select(id => learnedTagNameMap.TryGetValue(id, out var name) ? name : id.ToString())
                .ToList(),
            WinCondition = win,
            AvatarUrl = m.AvatarUrl,
            Gallery = m.MapMedias
                .OrderBy(media => media.SortOrder)
                .Select(media => new MapMediaItemDto
                {
                    Id = media.Id,
                    Url = media.Url,
                    Kind = media.Kind.ToString(),
                    SortOrder = media.SortOrder
                })
                .ToList()
        };
        }).ToList();

        var result = PaginationResult<MapListItemDto>.Success(list, pageNumber, pageSize, total, "Retrieved successfully");
        return Result<PaginationResult<MapListItemDto>>.Success(result);
    }
}

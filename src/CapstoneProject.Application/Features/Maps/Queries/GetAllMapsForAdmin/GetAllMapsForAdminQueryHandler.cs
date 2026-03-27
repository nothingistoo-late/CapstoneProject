using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Queries.GetAllMapsForAdmin;

public class GetAllMapsForAdminQueryHandler : IRequestHandler<GetAllMapsForAdminQuery, Result<PaginationResult<MapListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllMapsForAdminQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PaginationResult<MapListItemDto>>> Handle(GetAllMapsForAdminQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Map>().GetQueryable()
            .Where(m => !m.IsDeleted && m.Status == EntityStatusEnum.Active)
            .Include(m => m.MapTags).ThenInclude(mt => mt.Tag)
            .Include(m => m.Creator)
            .AsNoTracking();

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var sortBy = (request.SortBy ?? "CreatedAt").ToLowerInvariant();
        query = sortBy switch
        {
            "title" => request.SortAscending ? query.OrderBy(m => m.Title) : query.OrderByDescending(m => m.Title),
            "difficulty" => request.SortAscending ? query.OrderBy(m => m.Difficulty) : query.OrderByDescending(m => m.Difficulty),
            "timelimitms" => request.SortAscending ? query.OrderBy(m => m.TimeLimitMs) : query.OrderByDescending(m => m.TimeLimitMs),
            "price" => request.SortAscending ? query.OrderBy(m => m.Price ?? 0) : query.OrderByDescending(m => m.Price ?? 0),
            "mapstatus" => request.SortAscending ? query.OrderBy(m => m.MapStatus) : query.OrderByDescending(m => m.MapStatus),
            _ => request.SortAscending ? query.OrderBy(m => m.CreatedAt) : query.OrderByDescending(m => m.CreatedAt)
        };

        var page = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var learnedTagIds = page.SelectMany(m => m.LearnedTags).Distinct().ToList();
        var learnedTagNameMap = await _unitOfWork.Repository<Tag>().GetQueryable()
            .Where(t => learnedTagIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name, cancellationToken);

        var list = page.Select(m => new MapListItemDto
        {
            Id = m.Id,
            Title = m.Title,
            Description = m.Description,
            Difficulty = m.Difficulty,
            Type = m.Type.ToString(),
            TimeLimitMs = m.TimeLimitMs,
            IsPublished = m.IsPublished,
            MapStatus = m.MapStatus.ToString(),
            Price = m.Price,
            CreatedByUserId = m.CreatedBy ?? Guid.Empty,
            CreatedByUserName = m.Creator != null ? $"{m.Creator.FirstName} {m.Creator.LastName}".Trim() : null,
            CreatedAt = m.CreatedAt,
            TagNames = m.MapTags.Select(t => t.Tag.Name).ToList(),
            LearnedTags = m.LearnedTags
                .Select(id => learnedTagNameMap.TryGetValue(id, out var name) ? name : id.ToString())
                .ToList(),
            WinCondition = m.WinCondition,
            AvatarUrl = m.AvatarUrl
        }).ToList();

        var result = PaginationResult<MapListItemDto>.Success(list, pageNumber, pageSize, total, "Retrieved successfully");
        return Result<PaginationResult<MapListItemDto>>.Success(result);
    }
}

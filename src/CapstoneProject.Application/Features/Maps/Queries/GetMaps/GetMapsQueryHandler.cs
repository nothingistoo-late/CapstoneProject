using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
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
            .Include(m => m.MapTags).ThenInclude(mt => mt.Tag)
            .AsNoTracking();

        if (request.PublishedOnly == true) query = query.Where(m => m.IsPublished && m.MapStatus == MapStatusEnum.Published);
        if (request.MapStatus.HasValue) query = query.Where(m => m.MapStatus == request.MapStatus.Value);
        if (request.Difficulty.HasValue) query = query.Where(m => m.Difficulty == request.Difficulty.Value);
        if (request.TagId.HasValue) query = query.Where(m => m.MapTags.Any(t => t.TagId == request.TagId.Value));
        if (request.CreatedByUserId.HasValue) query = query.Where(m => m.CreatedBy == request.CreatedByUserId.Value);
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
            "timelimitms" => request.SortAscending ? query.OrderBy(m => m.TimeLimitMs) : query.OrderByDescending(m => m.TimeLimitMs),
            _ => request.SortAscending ? query.OrderBy(m => m.CreatedAt) : query.OrderByDescending(m => m.CreatedAt)
        };

        var list = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(m => new MapListItemDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                Difficulty = m.Difficulty,
                TimeLimitMs = m.TimeLimitMs,
                IsPublished = m.IsPublished,
                MapStatus = m.MapStatus,
                Price = m.Price,
                CreatedByUserId = m.CreatedBy ?? Guid.Empty,
                CreatedAt = m.CreatedAt,
                TagNames = m.MapTags.Select(t => t.Tag.Name).ToList(),
                WinCondition = m.WinCondition
            }).ToListAsync(cancellationToken);

        var result = PaginationResult<MapListItemDto>.Success(list, pageNumber, pageSize, total, "Retrieved successfully");
        return Result<PaginationResult<MapListItemDto>>.Success(result);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Queries.GetMyMapList;

public class GetMyMapListQueryHandler : IRequestHandler<GetMyMapListQuery, Result<PaginationResult<MapListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyMapListQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<MapListItemDto>>> Handle(GetMyMapListQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<PaginationResult<MapListItemDto>>.Failure("Authentication required. Please log in to view your maps.", ErrorCodeEnum.Unauthorized);

        var myMapRepo = _unitOfWork.Repository<MyMap>();
        var query = myMapRepo.GetQueryable()
            .Where(mm => !mm.IsDeleted && mm.UserId == userId.Value);

        if (request.IsAuthor.HasValue)
            query = query.Where(mm => mm.IsAuthor == request.IsAuthor.Value);

        query = query.Include(mm => mm.Map)
            .ThenInclude(m => m!.MapTags)
            .ThenInclude(mt => mt.Tag)
            .Include(mm => mm.Map)
            .ThenInclude(m => m!.Creator)
            .AsNoTracking();

        query = query.Where(mm => mm.Map != null && !mm.Map.IsDeleted && mm.Map.Status == EntityStatusEnum.Active);

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var sortBy = (request.SortBy ?? "CreatedAt").ToLowerInvariant();

        query = sortBy switch
        {
            "title" => request.SortAscending ? query.OrderBy(mm => mm.Map!.Title) : query.OrderByDescending(mm => mm.Map!.Title),
            "difficulty" => request.SortAscending ? query.OrderBy(mm => mm.Map!.Difficulty) : query.OrderByDescending(mm => mm.Map!.Difficulty),
            "timelimitms" => request.SortAscending ? query.OrderBy(mm => mm.Map!.TimeLimitMs) : query.OrderByDescending(mm => mm.Map!.TimeLimitMs),
            _ => request.SortAscending ? query.OrderBy(mm => mm.Map!.CreatedAt) : query.OrderByDescending(mm => mm.Map!.CreatedAt)
        };

        var page = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var list = page.Where(mm => mm.Map != null).Select(mm =>
        {
            var m = mm.Map!;
            return new MapListItemDto
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
                IsAuthor = mm.IsAuthor,
                CreatedAt = m.CreatedAt,
                TagNames = m.MapTags.Select(t => t.Tag.Name).ToList(),
                WinCondition = m.WinCondition,
                AvatarUrl = m.AvatarUrl
            };
        }).ToList();

        var result = PaginationResult<MapListItemDto>.Success(list, pageNumber, pageSize, total, "Retrieved successfully");
        return Result<PaginationResult<MapListItemDto>>.Success(result);
    }
}

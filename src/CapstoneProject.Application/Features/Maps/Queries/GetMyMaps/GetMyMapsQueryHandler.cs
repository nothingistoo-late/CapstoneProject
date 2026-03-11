using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Queries.GetMyMaps;

public class GetMyMapsQueryHandler : IRequestHandler<GetMyMapsQuery, Result<PaginationResult<MapListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyMapsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<MapListItemDto>>> Handle(GetMyMapsQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<PaginationResult<MapListItemDto>>.Failure("Authentication required. Please log in to view your maps.", ErrorCodeEnum.Unauthorized);

        var mapRepo = _unitOfWork.Repository<Map>();
        var paymentRepo = _unitOfWork.Repository<PaymentRecord>();

        // Map IDs: created by user
        var createdMapIds = await mapRepo.GetQueryable()
            .Where(m => !m.IsDeleted && m.Status == EntityStatusEnum.Active && m.CreatedBy == userId.Value)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        // Map IDs: purchased by user (PaymentRecord with MapId)
        var purchasedMapIds = await paymentRepo.GetQueryable()
            .Where(p => p.UserId == userId.Value && p.MapId != null)
            .Select(p => p.MapId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var ownedMapIds = createdMapIds.Union(purchasedMapIds).Distinct().ToList();
        if (ownedMapIds.Count == 0)
        {
            var empty = PaginationResult<MapListItemDto>.Success(new List<MapListItemDto>(), 1, request.PageSize, 0, "Retrieved successfully");
            return Result<PaginationResult<MapListItemDto>>.Success(empty);
        }

        var query = mapRepo.GetQueryable()
            .Where(m => !m.IsDeleted && m.Status == EntityStatusEnum.Active && ownedMapIds.Contains(m.Id))
            .Include(m => m.MapTags).ThenInclude(mt => mt.Tag)
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
            _ => request.SortAscending ? query.OrderBy(m => m.CreatedAt) : query.OrderByDescending(m => m.CreatedAt)
        };

        var list = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(m => new MapListItemDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                Difficulty = m.Difficulty,
                Type = m.Type.ToString(),
                TimeLimitMs = m.TimeLimitMs,
                IsPublished = m.IsPublished,
                MapStatus = m.MapStatus,
                Price = m.Price,
                CreatedByUserId = m.CreatedBy ?? Guid.Empty,
                CreatedAt = m.CreatedAt,
                TagNames = m.MapTags.Select(t => t.Tag.Name).ToList(),
                WinCondition = m.WinCondition,
                AvatarUrl = m.AvatarUrl
            }).ToListAsync(cancellationToken);

        var result = PaginationResult<MapListItemDto>.Success(list, pageNumber, pageSize, total, "Retrieved successfully");
        return Result<PaginationResult<MapListItemDto>>.Success(result);
    }
}

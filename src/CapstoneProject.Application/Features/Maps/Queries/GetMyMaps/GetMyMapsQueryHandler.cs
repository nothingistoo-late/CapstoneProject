using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Helpers;
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
        var myMapRepo = _unitOfWork.Repository<MyMap>();
        var paymentRepo = _unitOfWork.Repository<PaymentRecord>();

        // Map IDs từ bảng MyMap (nguồn chính: tạo map, mua map, thêm map free)
        var myMapIds = await myMapRepo.GetQueryable()
            .Where(mm => !mm.IsDeleted && mm.UserId == userId.Value)
            .Select(mm => mm.MapId)
            .ToListAsync(cancellationToken);

        // Backward compat: map do user tạo hoặc đã mua (trước khi có bảng MyMap)
        var createdMapIds = await mapRepo.GetQueryable()
            .Where(m => !m.IsDeleted && m.Status == EntityStatusEnum.Active && m.CreatedBy == userId.Value)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);
        var purchasedMapIds = await paymentRepo.GetQueryable()
            .Where(p => !p.IsDeleted && p.UserId == userId.Value && p.MapId != null)
            .Select(p => p.MapId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var allOwnedIds = myMapIds.Union(createdMapIds).Union(purchasedMapIds).Distinct().ToList();
        var ownedMapIds = request.IsAuthorOnly
            ? allOwnedIds.Where(id => createdMapIds.Contains(id)).ToList()
            : allOwnedIds;
        if (ownedMapIds.Count == 0)
        {
            var empty = PaginationResult<MapListItemDto>.Success(new List<MapListItemDto>(), 1, request.PageSize, 0, "Retrieved successfully");
            return Result<PaginationResult<MapListItemDto>>.Success(empty);
        }

        var query = mapRepo.GetQueryable()
            .Where(m => m.Status == EntityStatusEnum.Active && ownedMapIds.Contains(m.Id))
            .Include(m => m.MapDetails)
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
            "timelimitms" => request.SortAscending
                ? query.OrderBy(m => m.MapDetails.Where(d => !d.IsDeleted).OrderBy(d => d.LevelOrder).Select(d => d.TimeLimitMs).FirstOrDefault())
                : query.OrderByDescending(m => m.MapDetails.Where(d => !d.IsDeleted).OrderBy(d => d.LevelOrder).Select(d => d.TimeLimitMs).FirstOrDefault()),
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
            CreatedByUserId = m.CreatedBy ?? Guid.Empty,
            CreatedByUserName = m.Creator != null ? $"{m.Creator.FirstName} {m.Creator.LastName}".Trim() : null,
            // IsAuthor = user đang gửi request có phải là người tạo map (CreatedBy), không phải kiểm tra sở hữu
            IsAuthor = m.CreatedBy == userId.Value,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt,
            ContentVersion = m.ContentVersion,
            TagNames = m.MapTags.Select(t => t.Tag.Name).ToList(),
            LearnedTags = m.LearnedTags
                .Select(id => learnedTagNameMap.TryGetValue(id, out var name) ? name : id.ToString())
                .ToList(),
            WinCondition = win,
            AvatarUrl = m.AvatarUrl
        };
        }).ToList();

        var result = PaginationResult<MapListItemDto>.Success(list, pageNumber, pageSize, total, "Retrieved successfully");
        return Result<PaginationResult<MapListItemDto>>.Success(result);
    }
}

using CapstoneProject.Application.Commons.DTOs.Gameplay;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Gameplay.Queries.GetMyGamePlayHistory;

public class GetMyGamePlayHistoryQueryHandler : IRequestHandler<GetMyGamePlayHistoryQuery, Result<PaginationResult<MapPlayHistoryItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyGamePlayHistoryQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<MapPlayHistoryItemDto>>> Handle(GetMyGamePlayHistoryQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<PaginationResult<MapPlayHistoryItemDto>>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var historyRepo = _unitOfWork.Repository<UserGamePlayHistory>();
        var query = historyRepo.GetQueryable()
            .AsNoTracking()
            .Where(h => !h.IsDeleted && h.UserId == userId.Value);

        if (request.GameId.HasValue)
            query = query.Where(h => h.GameId == request.GameId.Value);

        if (request.PlayMode.HasValue)
            query = query.Where(h => h.PlayMode == request.PlayMode.Value);

        query = query.OrderByDescending(h => h.StartTime);

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var rows = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var gameIds = rows.Select(r => r.GameId).Distinct().ToList();
        var mapTitles = await _unitOfWork.Repository<Game>()
            .GetQueryable()
            .AsNoTracking()
            .Where(m => gameIds.Contains(m.Id) && !m.IsDeleted)
            .ToDictionaryAsync(m => m.Id, m => m.Title, cancellationToken);

        var list = rows.Select(h => new MapPlayHistoryItemDto
        {
            Id = h.Id,
            GameId = h.GameId,
            MapTitle = mapTitles.TryGetValue(h.GameId, out var t) ? t : null,
            PlayMode = h.PlayMode,
            StartTime = h.StartTime,
            EndTime = h.EndTime,
            IsCompleted = h.IsCompleted,
            Score = h.Score,
            Stars = h.Stars,
            SubmissionId = h.SubmissionId,
            ExecutionsResultId = h.ExecutionsResultId,
            RoomId = h.RoomId,
            MatchId = h.MatchId,
            Language = h.Language
        }).ToList();

        var page = PaginationResult<MapPlayHistoryItemDto>.Success(list, pageNumber, pageSize, total, "Đã truy xuất thành công");
        return Result<PaginationResult<MapPlayHistoryItemDto>>.Success(page, "Đã truy xuất thành công.");
    }
}

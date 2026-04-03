using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Gameplay;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Gameplay.Queries.GetProgressDashboard;

public class GetProgressDashboardQueryHandler : IRequestHandler<GetProgressDashboardQuery, Result<ProgressDashboardDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetProgressDashboardQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ProgressDashboardDto>> Handle(GetProgressDashboardQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<ProgressDashboardDto>.Failure("Authentication required. Please log in to view your progress dashboard.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var user = await _unitOfWork.Repository<AppUser>().GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user == null)
            return Result<ProgressDashboardDto>.Failure("User not found.", ErrorCodeEnum.NotFound);
        var totalXp = user.CurrentXp;

        var umrRepo = _unitOfWork.Repository<UserMapResult>();
        var mapRepo = _unitOfWork.Repository<Map>();
        var mapIdsTouched = await umrRepo.GetQueryable()
            .Where(u => u.UserId == userId && !u.IsDeleted)
            .Select(u => u.MapId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var completed = 0;
        foreach (var mid in mapIdsTouched)
        {
            if (await MapProgressHelper.MapHasAllLevelsCompletedAsync(_unitOfWork, userId, mid, minStars: 1, cancellationToken))
                completed++;
        }
        var totalStars = await umrRepo.GetQueryable()
            .Where(u => u.UserId == userId && !u.IsDeleted)
            .SumAsync(u => (int?)u.BestStars, cancellationToken) ?? 0;

        var badges = await _unitOfWork.Repository<UserAchievement>().GetQueryable()
            .Where(ua => ua.UserId == userId && !ua.IsDeleted)
            .Include(ua => ua.Achievement)
            .OrderByDescending(ua => ua.UnlockedAt)
            .Take(20)
            .Select(ua => new BadgeDto
            {
                Code = ua.Achievement.Code,
                Name = ua.Achievement.Name,
                UnlockedAt = ua.UnlockedAt
            })
            .ToListAsync(cancellationToken);

        var conceptsPracticed = new List<string>();

        var recent = await umrRepo.GetQueryable()
            .Where(u => u.UserId == userId && u.LastPlayedAt != null)
            .OrderByDescending(u => u.LastPlayedAt)
            .Take(10)
            .Join(mapRepo.GetQueryable(), u => u.MapId, m => m.Id, (u, m) => new { u, m })
            .Select(x => new RecentActivityDto
            {
                MapId = x.u.MapId,
                MapTitle = x.m.Title,
                Stars = x.u.BestStars,
                At = x.u.LastPlayedAt ?? DateTime.MinValue
            })
            .ToListAsync(cancellationToken);

        var dto = new ProgressDashboardDto
        {
            TotalXp = totalXp,
            MapsCompleted = completed,
            TotalStars = totalStars,
            Badges = badges,
            ConceptsPracticed = conceptsPracticed,
            RecentActivities = recent
        };
        return Result<ProgressDashboardDto>.Success(dto);
    }
}

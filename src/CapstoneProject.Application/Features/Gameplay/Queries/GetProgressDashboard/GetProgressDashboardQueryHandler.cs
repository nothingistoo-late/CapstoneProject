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
            return Result<ProgressDashboardDto>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để xem bảng điều khiển tiến trình của bạn.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var user = await _unitOfWork.Repository<AppUser>().GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user == null)
            return Result<ProgressDashboardDto>.Failure("Không tìm thấy người dùng.", ErrorCodeEnum.NotFound);
        var totalXp = user.CurrentXp;

        var umrRepo = _unitOfWork.Repository<UserGameResult>();
        var mapRepo = _unitOfWork.Repository<Game>();
        var gameIdsTouched = await umrRepo.GetQueryable()
            .Where(u => u.UserId == userId && !u.IsDeleted)
            .Select(u => u.GameId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var completed = 0;
        foreach (var mid in gameIdsTouched)
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
            .Join(mapRepo.GetQueryable(), u => u.GameId, m => m.Id, (u, m) => new { u, m })
            .Select(x => new RecentActivityDto
            {
                GameId = x.u.GameId,
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
        return Result<ProgressDashboardDto>.Success(dto, "Đã lấy bảng điều khiển tiến trình.");
    }
}

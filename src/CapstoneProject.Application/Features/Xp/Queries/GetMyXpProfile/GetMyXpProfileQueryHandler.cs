using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Xp.Queries.GetMyXpProfile;

public class GetMyXpProfileQueryHandler : IRequestHandler<GetMyXpProfileQuery, Result<MyXpProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyXpProfileQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MyXpProfileDto>> Handle(GetMyXpProfileQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<MyXpProfileDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var user = await _unitOfWork.Repository<AppUser>().GetQueryable().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user == null)
            return Result<MyXpProfileDto>.Failure("Không tìm thấy người dùng.", ErrorCodeEnum.NotFound);

        var thresholds = await _unitOfWork.Repository<LevelThreshold>().GetQueryable()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Level)
            .ToListAsync(cancellationToken);
        var currentThreshold = thresholds.LastOrDefault(x => x.RequiredTotalXp <= user.CurrentXp);
        var nextThreshold = thresholds.FirstOrDefault(x => x.RequiredTotalXp > user.CurrentXp);

        var baseXp = currentThreshold?.RequiredTotalXp ?? 0;
        var nextXp = nextThreshold?.RequiredTotalXp ?? user.CurrentXp;
        var denom = Math.Max(1, nextXp - baseXp);
        var progress = nextThreshold == null ? 100.0 : ((double)(user.CurrentXp - baseXp) / denom) * 100.0;

        return Result<MyXpProfileDto>.Success(new MyXpProfileDto
        {
            UserId = user.Id,
            CurrentXp = user.CurrentXp,
            CurrentLevel = user.CurrentLevel,
            NextLevel = nextThreshold?.Level ?? user.CurrentLevel,
            XpToNextLevel = Math.Max(0, nextXp - user.CurrentXp),
            ProgressPercent = Math.Clamp(progress, 0, 100)
        });
    }
}


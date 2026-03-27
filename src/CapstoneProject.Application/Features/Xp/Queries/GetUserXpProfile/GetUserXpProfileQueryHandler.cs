using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Xp.Queries.GetMyXpProfile;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Xp.Queries.GetUserXpProfile;

public class GetUserXpProfileQueryHandler : IRequestHandler<GetUserXpProfileQuery, Result<MyXpProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetUserXpProfileQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MyXpProfileDto>> Handle(GetUserXpProfileQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<MyXpProfileDto>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<MyXpProfileDto>.Failure("Only Admin/Moderator can view user XP profile.", ErrorCodeEnum.Forbidden);

        var user = await _unitOfWork.Repository<AppUser>().GetQueryable().FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        if (user == null)
            return Result<MyXpProfileDto>.Failure("User not found.", ErrorCodeEnum.NotFound);

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


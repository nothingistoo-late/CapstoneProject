using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Xp.Queries.GetXpLeaderboard;

public class GetXpLeaderboardQueryHandler : IRequestHandler<GetXpLeaderboardQuery, Result<PaginationResult<XpLeaderboardItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetXpLeaderboardQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<XpLeaderboardItemDto>>> Handle(GetXpLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<PaginationResult<XpLeaderboardItemDto>>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var query = _unitOfWork.Repository<AppUser>().GetQueryable()
            .Where(u => u.Status == Domain.Enums.EntityStatusEnum.Active)
            .OrderByDescending(u => u.CurrentXp)
            .ThenByDescending(u => u.CurrentLevel)
            .ThenBy(u => u.JoiningAt);

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (pageNumber - 1) * pageSize;

        var users = await query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
        var items = users.Select((u, idx) => new XpLeaderboardItemDto
        {
            Rank = skip + idx + 1,
            UserId = u.Id,
            DisplayName = $"{u.FirstName} {u.LastName}".Trim(),
            CurrentXp = u.CurrentXp,
            CurrentLevel = u.CurrentLevel
        }).ToList();

        var result = PaginationResult<XpLeaderboardItemDto>.Success(items, pageNumber, pageSize, total);
        return Result<PaginationResult<XpLeaderboardItemDto>>.Success(result);
    }
}


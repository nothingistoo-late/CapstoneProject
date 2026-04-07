using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Xp.Queries.GetXpSourceConfigs;

public class GetXpSourceConfigsQueryHandler : IRequestHandler<GetXpSourceConfigsQuery, Result<List<XpSourceConfigDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetXpSourceConfigsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<XpSourceConfigDto>>> Handle(GetXpSourceConfigsQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<List<XpSourceConfigDto>>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<List<XpSourceConfigDto>>.Failure("Only Admin/Moderator can view XP source configs.", ErrorCodeEnum.Forbidden);

        var list = await _unitOfWork.Repository<XpSourceConfig>().GetQueryable()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.SourceType)
            .Select(x => new XpSourceConfigDto
            {
                SourceType = x.SourceType.ToString(),
                IsEnabled = x.IsEnabled,
                BaseXp = x.BaseXp,
                DailyCap = x.DailyCap,
                BonusMultiplier = x.BonusMultiplier,
                ConfigJson = x.ConfigJson
            })
            .ToListAsync(cancellationToken);

        return Result<List<XpSourceConfigDto>>.Success(list);
    }
}


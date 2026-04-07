using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Xp.Queries.GetXpPolicyConfigs;

public class GetXpPolicyConfigsQueryHandler : IRequestHandler<GetXpPolicyConfigsQuery, Result<List<XpPolicyConfigDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetXpPolicyConfigsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<XpPolicyConfigDto>>> Handle(GetXpPolicyConfigsQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<List<XpPolicyConfigDto>>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<List<XpPolicyConfigDto>>.Failure("Only Admin/Moderator can view XP policy configs.", ErrorCodeEnum.Forbidden);

        var list = await _unitOfWork.Repository<XpPolicyConfig>().GetQueryable()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Priority)
            .Select(x => new XpPolicyConfigDto
            {
                PolicyKey = x.PolicyKey,
                IsEnabled = x.IsEnabled,
                Priority = x.Priority,
                ConfigJson = x.ConfigJson,
                ActiveFrom = x.ActiveFrom,
                ActiveTo = x.ActiveTo
            })
            .ToListAsync(cancellationToken);

        return Result<List<XpPolicyConfigDto>>.Success(list);
    }
}


using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetComplaintPolicyRuleConfigs;

public class GetComplaintPolicyRuleConfigsQueryHandler : IRequestHandler<GetComplaintPolicyRuleConfigsQuery, Result<List<ComplaintPolicyRuleConfigDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetComplaintPolicyRuleConfigsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<ComplaintPolicyRuleConfigDto>>> Handle(GetComplaintPolicyRuleConfigsQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<List<ComplaintPolicyRuleConfigDto>>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<List<ComplaintPolicyRuleConfigDto>>.Failure("Only Admin/Moderator can view complaint policy rule configs.", ErrorCodeEnum.Forbidden);

        var query = _unitOfWork.Repository<ComplaintPolicyRuleConfig>().GetQueryable()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.CategoryKey))
        {
            var categoryKey = request.CategoryKey.Trim();
            query = query.Where(x => x.CategoryKey == categoryKey);
        }

        var list = await query
            .OrderBy(x => x.CategoryKey)
            .ThenBy(x => x.Priority)
            .ThenBy(x => x.RuleKey)
            .Select(x => new ComplaintPolicyRuleConfigDto
            {
                CategoryKey = x.CategoryKey,
                RuleKey = x.RuleKey,
                IsEnabled = x.IsEnabled,
                Priority = x.Priority,
                ConfigJson = x.ConfigJson,
                ActiveFrom = x.ActiveFrom,
                ActiveTo = x.ActiveTo
            })
            .ToListAsync(cancellationToken);

        return Result<List<ComplaintPolicyRuleConfigDto>>.Success(list);
    }
}

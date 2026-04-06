using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Commands.UpsertComplaintPolicyRuleConfig;

public class UpsertComplaintPolicyRuleConfigCommandHandler : IRequestHandler<UpsertComplaintPolicyRuleConfigCommand, Result<UpsertComplaintPolicyRuleConfigDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpsertComplaintPolicyRuleConfigCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UpsertComplaintPolicyRuleConfigDto>> Handle(UpsertComplaintPolicyRuleConfigCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<UpsertComplaintPolicyRuleConfigDto>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<UpsertComplaintPolicyRuleConfigDto>.Failure("Only Admin/Moderator can update complaint policy rule configs.", ErrorCodeEnum.Forbidden);

        if (string.IsNullOrWhiteSpace(request.CategoryKey))
            return Result<UpsertComplaintPolicyRuleConfigDto>.Failure("CategoryKey is required.", ErrorCodeEnum.ValidationFailed);
        if (string.IsNullOrWhiteSpace(request.RuleKey))
            return Result<UpsertComplaintPolicyRuleConfigDto>.Failure("RuleKey is required.", ErrorCodeEnum.ValidationFailed);

        var categoryKey = request.CategoryKey.Trim();
        var ruleKey = request.RuleKey.Trim();
        var userId = userIdNullable.Value;

        var categoryExists = await _unitOfWork.Repository<ComplaintCategoryCatalog>().GetQueryable()
            .AnyAsync(x => !x.IsDeleted && x.CategoryKey == categoryKey, cancellationToken);
        if (!categoryExists)
            return Result<UpsertComplaintPolicyRuleConfigDto>.Failure("Complaint category config not found for this rule.", ErrorCodeEnum.NotFound);

        var repo = _unitOfWork.Repository<ComplaintPolicyRuleConfig>();
        var row = await repo.GetQueryable().FirstOrDefaultAsync(x => x.CategoryKey == categoryKey && x.RuleKey == ruleKey, cancellationToken);

        if (row == null)
        {
            row = new ComplaintPolicyRuleConfig
            {
                CategoryKey = categoryKey,
                RuleKey = ruleKey,
                IsEnabled = request.IsEnabled,
                Priority = request.Priority,
                ConfigJson = request.ConfigJson,
                ActiveFrom = request.ActiveFrom,
                ActiveTo = request.ActiveTo,
                Status = EntityStatusEnum.Active
            };
            row.InitializeEntity(userId);
            await repo.AddAsync(row);
        }
        else
        {
            row.IsEnabled = request.IsEnabled;
            row.Priority = request.Priority;
            row.ConfigJson = request.ConfigJson;
            row.ActiveFrom = request.ActiveFrom;
            row.ActiveTo = request.ActiveTo;
            if (row.IsDeleted)
                row.RestoreEntity(userId);
            row.UpdateEntity(userId);
            repo.Update(row);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var result = new UpsertComplaintPolicyRuleConfigDto
        {
            CategoryKey = row.CategoryKey,
            RuleKey = row.RuleKey,
            IsEnabled = row.IsEnabled,
            Priority = row.Priority,
            ConfigJson = row.ConfigJson,
            ActiveFrom = row.ActiveFrom,
            ActiveTo = row.ActiveTo
        };

        return Result<UpsertComplaintPolicyRuleConfigDto>.Success(result, "Complaint policy rule config saved.");
    }
}

using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Complaints.Queries.GetComplaintCategoryConfigs;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetAvailableComplaintCategories;

public class GetAvailableComplaintCategoriesQueryHandler : IRequestHandler<GetAvailableComplaintCategoriesQuery, Result<List<ComplaintCategoryConfigDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetAvailableComplaintCategoriesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<ComplaintCategoryConfigDto>>> Handle(GetAvailableComplaintCategoriesQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<List<ComplaintCategoryConfigDto>>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Learner) && !roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<List<ComplaintCategoryConfigDto>>.Failure("You do not have permission to view complaint categories.", ErrorCodeEnum.Forbidden);

        var categories = await _unitOfWork.Repository<ComplaintCategoryCatalog>().GetQueryable()
            .Where(x => !x.IsDeleted && x.IsEnabled)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CategoryKey)
            .ToListAsync(cancellationToken);

        var categoryKeys = categories.Select(x => x.CategoryKey).Distinct().ToList();
        var requiredContextRules = await _unitOfWork.Repository<ComplaintPolicyRuleConfig>().GetQueryable()
            .Where(x => !x.IsDeleted
                        && x.IsEnabled
                        && x.RuleKey == "required_context"
                        && categoryKeys.Contains(x.CategoryKey))
            .Select(x => new { x.CategoryKey, x.ConfigJson })
            .ToListAsync(cancellationToken);

        var requiredFieldsByCategory = requiredContextRules
            .GroupBy(x => x.CategoryKey)
            .ToDictionary(
                g => g.Key,
                g => g.SelectMany(x => ReadAnyOfFields(x.ConfigJson)).Distinct().ToList());

        var list = categories
            .Select(x => new ComplaintCategoryConfigDto
            {
                CategoryKey = x.CategoryKey,
                DisplayName = x.DisplayName,
                Description = x.Description,
                IsEnabled = x.IsEnabled,
                SortOrder = x.SortOrder,
                RequiredAnyContextFields = requiredFieldsByCategory.GetValueOrDefault(x.CategoryKey, new List<string>()),
                // Learner should open complaints from contextual flows instead of manually typing GUIDs.
                AllowManualContextInput = false,
            })
            .ToList();

        return Result<List<ComplaintCategoryConfigDto>>.Success(list);
    }

    private static IEnumerable<string> ReadAnyOfFields(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
            return Array.Empty<string>();

        try
        {
            using var doc = JsonDocument.Parse(configJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return Array.Empty<string>();
            if (!doc.RootElement.TryGetProperty("anyOf", out var anyOf) || anyOf.ValueKind != JsonValueKind.Array)
                return Array.Empty<string>();

            return anyOf
                .EnumerateArray()
                .Where(x => x.ValueKind == JsonValueKind.String)
                .Select(x => x.GetString())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim())
                .ToList();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}

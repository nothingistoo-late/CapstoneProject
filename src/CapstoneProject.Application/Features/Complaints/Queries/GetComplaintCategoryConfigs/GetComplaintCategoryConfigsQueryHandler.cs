using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetComplaintCategoryConfigs;

public class GetComplaintCategoryConfigsQueryHandler : IRequestHandler<GetComplaintCategoryConfigsQuery, Result<List<ComplaintCategoryConfigDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetComplaintCategoryConfigsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<ComplaintCategoryConfigDto>>> Handle(GetComplaintCategoryConfigsQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<List<ComplaintCategoryConfigDto>>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<List<ComplaintCategoryConfigDto>>.Failure("Only Admin/Moderator can view complaint category configs.", ErrorCodeEnum.Forbidden);

        var list = await _unitOfWork.Repository<ComplaintCategoryCatalog>().GetQueryable()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CategoryKey)
            .Select(x => new ComplaintCategoryConfigDto
            {
                CategoryKey = x.CategoryKey,
                DisplayName = x.DisplayName,
                Description = x.Description,
                IsEnabled = x.IsEnabled,
                SortOrder = x.SortOrder
            })
            .ToListAsync(cancellationToken);

        return Result<List<ComplaintCategoryConfigDto>>.Success(list);
    }
}

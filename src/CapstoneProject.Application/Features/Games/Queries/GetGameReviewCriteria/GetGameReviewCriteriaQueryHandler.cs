using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Games.Queries.GetGameReviewCriteria;

public class GetGameReviewCriteriaQueryHandler : IRequestHandler<GetGameReviewCriteriaQuery, Result<List<GameReviewCriterionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetGameReviewCriteriaQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<GameReviewCriterionDto>>> Handle(GetGameReviewCriteriaQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<List<GameReviewCriterionDto>>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<List<GameReviewCriterionDto>>.Failure("Chỉ Admin hoặc Moderator có thể xem tiêu chí duyệt game.", ErrorCodeEnum.Forbidden);

        var list = await _unitOfWork.Repository<GameReviewCriterionCatalog>().GetQueryable()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CriterionKey)
            .Select(x => new GameReviewCriterionDto
            {
                Id = x.Id,
                CriterionKey = x.CriterionKey,
                SectionKey = x.SectionKey,
                SectionTitle = x.SectionTitle,
                Label = x.Label,
                SortOrder = x.SortOrder,
                IsEnabled = x.IsEnabled,
            })
            .ToListAsync(cancellationToken);

        return Result<List<GameReviewCriterionDto>>.Success(list, "OK");
    }
}

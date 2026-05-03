using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Games.Commands.UpdateGameReviewCriterion;

public class UpdateGameReviewCriterionCommandHandler : IRequestHandler<UpdateGameReviewCriterionCommand, Result<GameReviewCriterionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateGameReviewCriterionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GameReviewCriterionDto>> Handle(UpdateGameReviewCriterionCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<GameReviewCriterionDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin))
            return Result<GameReviewCriterionDto>.Failure("Chỉ Admin có thể cập nhật tiêu chí duyệt game.", ErrorCodeEnum.Forbidden);

        var entity = await _unitOfWork.Repository<GameReviewCriterionCatalog>().GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (entity == null)
            return Result<GameReviewCriterionDto>.Failure("Không tìm thấy tiêu chí.", ErrorCodeEnum.NotFound);

        var r = command.Request;
        var sectionKey = r.SectionKey?.Trim() ?? "";
        if (string.IsNullOrEmpty(sectionKey) || sectionKey.Length > 80)
            return Result<GameReviewCriterionDto>.Failure("SectionKey là bắt buộc (tối đa 80 ký tự).", ErrorCodeEnum.ValidationFailed);

        var sectionTitle = r.SectionTitle?.Trim() ?? "";
        if (string.IsNullOrEmpty(sectionTitle) || sectionTitle.Length > 200)
            return Result<GameReviewCriterionDto>.Failure("SectionTitle là bắt buộc (tối đa 200 ký tự).", ErrorCodeEnum.ValidationFailed);

        var label = r.Label?.Trim() ?? "";
        if (string.IsNullOrEmpty(label) || label.Length > 500)
            return Result<GameReviewCriterionDto>.Failure("Label là bắt buộc (tối đa 500 ký tự).", ErrorCodeEnum.ValidationFailed);

        entity.SectionKey = sectionKey;
        entity.SectionTitle = sectionTitle;
        entity.Label = label;
        entity.SortOrder = r.SortOrder;
        entity.IsEnabled = r.IsEnabled;
        entity.UpdateEntity(userIdNullable.Value);
        _unitOfWork.Repository<GameReviewCriterionCatalog>().Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new GameReviewCriterionDto
        {
            Id = entity.Id,
            CriterionKey = entity.CriterionKey,
            SectionKey = entity.SectionKey,
            SectionTitle = entity.SectionTitle,
            Label = entity.Label,
            SortOrder = entity.SortOrder,
            IsEnabled = entity.IsEnabled,
        };
        return Result<GameReviewCriterionDto>.Success(dto, "Đã cập nhật tiêu chí.");
    }
}

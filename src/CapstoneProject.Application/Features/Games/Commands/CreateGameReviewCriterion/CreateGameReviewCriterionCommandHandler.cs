using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using System.Text.RegularExpressions;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Games.Commands.CreateGameReviewCriterion;

public class CreateGameReviewCriterionCommandHandler : IRequestHandler<CreateGameReviewCriterionCommand, Result<GameReviewCriterionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateGameReviewCriterionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GameReviewCriterionDto>> Handle(CreateGameReviewCriterionCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<GameReviewCriterionDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin))
            return Result<GameReviewCriterionDto>.Failure("Chỉ Admin có thể thêm tiêu chí duyệt game.", ErrorCodeEnum.Forbidden);

        var r = command.Request;
        var key = NormalizeCriterionKey(r.CriterionKey);
        if (string.IsNullOrEmpty(key) || key.Length > 120 || !Regex.IsMatch(key, "^[a-z0-9_-]+$"))
            return Result<GameReviewCriterionDto>.Failure("CriterionKey không hợp lệ (chữ thường, số, gạch ngang/gạch dưới, tối đa 120).", ErrorCodeEnum.ValidationFailed);

        var sectionKey = r.SectionKey?.Trim() ?? "";
        if (string.IsNullOrEmpty(sectionKey) || sectionKey.Length > 80)
            return Result<GameReviewCriterionDto>.Failure("SectionKey là bắt buộc (tối đa 80 ký tự).", ErrorCodeEnum.ValidationFailed);

        var sectionTitle = r.SectionTitle?.Trim() ?? "";
        if (string.IsNullOrEmpty(sectionTitle) || sectionTitle.Length > 200)
            return Result<GameReviewCriterionDto>.Failure("SectionTitle là bắt buộc (tối đa 200 ký tự).", ErrorCodeEnum.ValidationFailed);

        var label = r.Label?.Trim() ?? "";
        if (string.IsNullOrEmpty(label) || label.Length > 500)
            return Result<GameReviewCriterionDto>.Failure("Label là bắt buộc (tối đa 500 ký tự).", ErrorCodeEnum.ValidationFailed);

        var exists = await _unitOfWork.Repository<GameReviewCriterionCatalog>().GetQueryable()
            .AnyAsync(x => x.CriterionKey == key, cancellationToken);
        if (exists)
            return Result<GameReviewCriterionDto>.Failure($"Đã tồn tại tiêu chí với key: {key}.", ErrorCodeEnum.ValidationFailed);

        var entity = new GameReviewCriterionCatalog
        {
            CriterionKey = key,
            SectionKey = sectionKey,
            SectionTitle = sectionTitle,
            Label = label,
            SortOrder = r.SortOrder,
            IsEnabled = r.IsEnabled,
            Status = EntityStatusEnum.Active,
        };
        entity.InitializeEntity(userIdNullable.Value);
        await _unitOfWork.Repository<GameReviewCriterionCatalog>().AddAsync(entity);
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
        return Result<GameReviewCriterionDto>.Success(dto, "Đã tạo tiêu chí.");
    }

    private static string NormalizeCriterionKey(string raw)
    {
        var s = raw.Trim().ToLowerInvariant().Replace(' ', '-');
        return Regex.Replace(s, "[^a-z0-9_-]", "");
    }
}

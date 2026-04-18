using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Games.Commands.BatchApproveMaps;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Games.Commands.BatchPublishMaps;

public class BatchPublishMapsCommandHandler : IRequestHandler<BatchPublishMapsCommand, Result<BatchMapResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public BatchPublishMapsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<BatchMapResultDto>> Handle(BatchPublishMapsCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<BatchMapResultDto>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để thực hiện xuất bản hàng loạt.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<BatchMapResultDto>.Failure("Bạn không có quyền xuất bản bản đồ. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện xuất bản hàng loạt.", ErrorCodeEnum.Forbidden);

        var repo = _unitOfWork.Repository<Game>();
        var games = await repo.GetQueryable()
            .Where(m => command.GameIds.Contains(m.Id) && !m.IsDeleted)
            .ToListAsync(cancellationToken);
        var foundIds = games.Select(m => m.Id).ToHashSet();
        var notFoundIds = command.GameIds.Where(id => !foundIds.Contains(id)).ToList();
        var toPublish = games.Where(m => m.GameStatus == GameStatusEnum.Approved).ToList();
        var invalidStatusIds = games.Where(m => m.GameStatus != GameStatusEnum.Approved).Select(m => m.Id).ToList();

        foreach (var game in toPublish)
        {
            var rootGameId = game.RootGameId ?? game.Id;
            if (!game.RootGameId.HasValue)
                game.RootGameId = rootGameId;

            var lineMaps = await repo.GetQueryable()
                .Where(m => !m.IsDeleted && (m.RootGameId ?? m.Id) == rootGameId)
                .ToListAsync(cancellationToken);

            foreach (var sibling in lineMaps.Where(m => m.Id != game.Id && m.IsActiveVersion))
            {
                sibling.IsActiveVersion = false;
                sibling.IsPublished = false;
                sibling.UpdateEntity(userIdNullable.Value);
                repo.Update(sibling);
            }

            game.GameStatus = GameStatusEnum.Published;
            game.IsPublished = true;
            game.IsActiveVersion = true;
            game.UpdateEntity(userIdNullable!.Value);
            repo.Update(game);

            var publishedInactive = lineMaps
                .Where(m => m.Id != game.Id && !m.IsDeleted && m.GameStatus == GameStatusEnum.Published)
                .OrderByDescending(m => m.ContentVersion)
                .ThenByDescending(m => m.CreatedAt)
                .ToList();
            var keepSet = publishedInactive.Take(2).Select(m => m.Id).ToHashSet();
            foreach (var old in publishedInactive.Where(m => !keepSet.Contains(m.Id)))
            {
                old.IsActiveVersion = false;
                old.IsPublished = false;
                old.IsDeleted = true;
                old.DeletedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
                old.DeletedBy = userIdNullable.Value;
                old.UpdateEntity(userIdNullable.Value);
                repo.Update(old);
            }
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new BatchMapResultDto
        {
            SuccessCount = toPublish.Count,
            FailedCount = notFoundIds.Count + invalidStatusIds.Count,
            NotFoundIds = notFoundIds,
            InvalidStatusIds = invalidStatusIds
        };
        return Result<BatchMapResultDto>.Success(dto, $"Đã xuất bản {dto.SuccessCount} bản đồ.");
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Maps.Commands.BatchApproveMaps;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Commands.BatchPublishMaps;

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

        var repo = _unitOfWork.Repository<Map>();
        var maps = await repo.GetQueryable()
            .Where(m => command.MapIds.Contains(m.Id) && !m.IsDeleted)
            .ToListAsync(cancellationToken);
        var foundIds = maps.Select(m => m.Id).ToHashSet();
        var notFoundIds = command.MapIds.Where(id => !foundIds.Contains(id)).ToList();
        var toPublish = maps.Where(m => m.MapStatus == MapStatusEnum.Approved).ToList();
        var invalidStatusIds = maps.Where(m => m.MapStatus != MapStatusEnum.Approved).Select(m => m.Id).ToList();

        foreach (var map in toPublish)
        {
            var rootMapId = map.RootMapId ?? map.Id;
            if (!map.RootMapId.HasValue)
                map.RootMapId = rootMapId;

            var lineMaps = await repo.GetQueryable()
                .Where(m => !m.IsDeleted && (m.RootMapId ?? m.Id) == rootMapId)
                .ToListAsync(cancellationToken);

            foreach (var sibling in lineMaps.Where(m => m.Id != map.Id && m.IsActiveVersion))
            {
                sibling.IsActiveVersion = false;
                sibling.IsPublished = false;
                sibling.UpdateEntity(userIdNullable.Value);
                repo.Update(sibling);
            }

            map.MapStatus = MapStatusEnum.Published;
            map.IsPublished = true;
            map.IsActiveVersion = true;
            map.UpdateEntity(userIdNullable!.Value);
            repo.Update(map);

            var publishedInactive = lineMaps
                .Where(m => m.Id != map.Id && !m.IsDeleted && m.MapStatus == MapStatusEnum.Published)
                .OrderByDescending(m => m.ContentVersion)
                .ThenByDescending(m => m.CreatedAt)
                .ToList();
            var keepSet = publishedInactive.Take(2).Select(m => m.Id).ToHashSet();
            foreach (var old in publishedInactive.Where(m => !keepSet.Contains(m.Id)))
            {
                old.IsActiveVersion = false;
                old.IsPublished = false;
                old.IsDeleted = true;
                old.DeletedAt = DateTime.UtcNow;
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

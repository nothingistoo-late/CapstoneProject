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
            return Result<BatchMapResultDto>.Failure("Authentication required. Please log in to perform batch publish.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<BatchMapResultDto>.Failure("You do not have permission to publish maps. Only Admin or Moderator can perform batch publish.", ErrorCodeEnum.Forbidden);

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
            map.MapStatus = MapStatusEnum.Published;
            map.IsPublished = true;
            map.UpdateEntity(userIdNullable!.Value);
            repo.Update(map);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new BatchMapResultDto
        {
            SuccessCount = toPublish.Count,
            FailedCount = notFoundIds.Count + invalidStatusIds.Count,
            NotFoundIds = notFoundIds,
            InvalidStatusIds = invalidStatusIds
        };
        return Result<BatchMapResultDto>.Success(dto, $"Published {dto.SuccessCount} map(s).");
    }
}

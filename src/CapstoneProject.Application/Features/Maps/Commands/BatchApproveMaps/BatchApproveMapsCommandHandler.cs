using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Commands.BatchApproveMaps;

public class BatchApproveMapsCommandHandler : IRequestHandler<BatchApproveMapsCommand, Result<BatchMapResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public BatchApproveMapsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<BatchMapResultDto>> Handle(BatchApproveMapsCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<BatchMapResultDto>.Failure("Authentication required. Please log in to perform batch approve.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<BatchMapResultDto>.Failure("You do not have permission to approve maps. Only Admin or Moderator can perform batch approve.", ErrorCodeEnum.Forbidden);

        var repo = _unitOfWork.Repository<Map>();
        var maps = await repo.GetQueryable()
            .Where(m => command.MapIds.Contains(m.Id) && !m.IsDeleted)
            .ToListAsync(cancellationToken);
        var foundIds = maps.Select(m => m.Id).ToHashSet();
        var notFoundIds = command.MapIds.Where(id => !foundIds.Contains(id)).ToList();
        var toApprove = maps.Where(m => m.MapStatus == MapStatusEnum.PendingReview).ToList();
        var invalidStatusIds = maps.Where(m => m.MapStatus != MapStatusEnum.PendingReview).Select(m => m.Id).ToList();

        foreach (var map in toApprove)
        {
            map.MapStatus = MapStatusEnum.Approved;
            map.UpdateEntity(userIdNullable!.Value);
            repo.Update(map);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new BatchMapResultDto
        {
            SuccessCount = toApprove.Count,
            FailedCount = notFoundIds.Count + invalidStatusIds.Count,
            NotFoundIds = notFoundIds,
            InvalidStatusIds = invalidStatusIds
        };
        return Result<BatchMapResultDto>.Success(dto, $"Approved {dto.SuccessCount} map(s).");
    }
}

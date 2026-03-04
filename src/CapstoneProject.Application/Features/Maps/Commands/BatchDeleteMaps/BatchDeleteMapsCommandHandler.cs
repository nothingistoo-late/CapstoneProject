using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Maps.Commands.BatchDeleteMaps;

public class BatchDeleteMapsCommandHandler : IRequestHandler<BatchDeleteMapsCommand, Result<BatchDeleteMapsResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public BatchDeleteMapsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<BatchDeleteMapsResultDto>> Handle(BatchDeleteMapsCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<BatchDeleteMapsResultDto>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var repo = _unitOfWork.Repository<LevelCatalog>();
        var ids = command.Ids?.Distinct().ToList() ?? new List<Guid>();
        var entities = await repo.GetQueryable()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
        var foundIds = entities.Select(x => x.Id).ToHashSet();
        var notFoundIds = ids.Where(id => !foundIds.Contains(id)).ToList();

        foreach (var entity in entities)
        {
            entity.SoftDeleteEntity(userId);
            repo.Update(entity);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new BatchDeleteMapsResultDto
        {
            SuccessCount = entities.Count,
            NotFoundCount = notFoundIds.Count,
            NotFoundIds = notFoundIds
        };
        return Result<BatchDeleteMapsResultDto>.Success(dto, $"Deleted {dto.SuccessCount} map(s).");
    }
}

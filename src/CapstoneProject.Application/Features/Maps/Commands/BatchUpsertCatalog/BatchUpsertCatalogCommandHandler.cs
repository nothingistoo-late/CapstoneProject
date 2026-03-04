using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Maps.Commands.BatchUpsertCatalog;

public class BatchUpsertCatalogCommandHandler : IRequestHandler<BatchUpsertCatalogCommand, Result<BatchUpsertCatalogResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public BatchUpsertCatalogCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<BatchUpsertCatalogResultDto>> Handle(BatchUpsertCatalogCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<BatchUpsertCatalogResultDto>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var levels = command.Request.Levels?
            .Where(x => !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.Name))
            .GroupBy(x => x.Id.Trim())
            .Select(g => g.First())
            .ToList() ?? new List<LevelCatalogItemDto>();
        if (levels.Count == 0)
            return Result<BatchUpsertCatalogResultDto>.Success(
                new BatchUpsertCatalogResultDto { CreatedCount = 0, UpdatedCount = 0, ExternalIds = new List<string>() },
                "No valid levels to upsert.");

        var names = levels.Select(x => x.Name.Trim()).Distinct().ToList();
        var repo = _unitOfWork.Repository<LevelCatalog>();
        var existing = await repo.GetQueryable()
            .Where(x => names.Contains(x.Name))
            .ToListAsync(cancellationToken);
        var byName = existing.ToDictionary(x => x.Name, x => x);

        int created = 0, updated = 0;
        var processedIds = new List<string>();

        foreach (var item in levels)
        {
            var nameKey = item.Name.Trim();
            if (byName.TryGetValue(nameKey, out var entity))
            {
                entity.Name = item.Name;
                entity.Type = item.Type ?? entity.Type;
                entity.Difficulty = item.Difficulty ?? entity.Difficulty;
                entity.UpdateEntity(userId);
                repo.Update(entity);
                updated++;
            }
            else
            {
                var newEntity = new LevelCatalog
                {
                    Name = item.Name,
                    Type = item.Type,
                    Difficulty = item.Difficulty
                };
                newEntity.InitializeEntity(userId);
                await repo.AddAsync(newEntity);
                created++;
            }
            processedIds.Add(nameKey);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new BatchUpsertCatalogResultDto
        {
            CreatedCount = created,
            UpdatedCount = updated,
            ExternalIds = processedIds.Distinct().ToList()
        };
        return Result<BatchUpsertCatalogResultDto>.Success(dto, $"Catalog upserted: {created} created, {updated} updated.");
    }
}

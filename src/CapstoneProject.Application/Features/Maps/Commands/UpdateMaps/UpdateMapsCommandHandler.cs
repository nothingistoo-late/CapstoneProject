using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Maps.Commands.UpdateMaps;

public class UpdateMapsCommandHandler : IRequestHandler<UpdateMapsCommand, Result<MapsResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMapsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MapsResponseDto>> Handle(UpdateMapsCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<MapsResponseDto>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var catalogRepo = _unitOfWork.Repository<LevelCatalog>();
        var catalog = await catalogRepo.GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (catalog == null)
            return Result<MapsResponseDto>.Failure("Level not found.", ErrorCodeEnum.NotFound);

        catalog.Name = command.Request.Name ?? catalog.Name;
        if (command.Request.Type != null) catalog.Type = command.Request.Type;
        if (command.Request.Difficulty != null) catalog.Difficulty = command.Request.Difficulty;
        if (!string.IsNullOrWhiteSpace(command.Request.JsonContent))
        {
            try
            {
                using var doc = JsonDocument.Parse(command.Request.JsonContent);
                var root = doc.RootElement;
                if (root.TryGetProperty("name", out var nameEl)) catalog.Name = nameEl.GetString() ?? catalog.Name;
                if (root.TryGetProperty("type", out var typeEl)) catalog.Type = typeEl.GetString();
                if (root.TryGetProperty("metadata", out var meta) && meta.TryGetProperty("difficulty", out var diffEl)) catalog.Difficulty = diffEl.GetString();
            }
            catch { /* keep existing */ }
        }

        catalog.UpdateEntity(userId);
        catalogRepo.Update(catalog);

        var detailRepo = _unitOfWork.Repository<LevelDetail>();
        var existingDetail = await detailRepo.GetQueryable()
            .FirstOrDefaultAsync(x => x.LevelCatalogId == command.Id, cancellationToken);
        string? jsonContent = existingDetail?.JsonContent;

        if (!string.IsNullOrWhiteSpace(command.Request.JsonContent))
        {
            if (existingDetail != null)
            {
                existingDetail.JsonContent = command.Request.JsonContent;
                existingDetail.UpdateEntity(userId);
                detailRepo.Update(existingDetail);
            }
            else
            {
                var detail = new LevelDetail { LevelCatalogId = command.Id, JsonContent = command.Request.JsonContent };
                detail.InitializeEntity(userId);
                await detailRepo.AddAsync(detail);
            }
            jsonContent = command.Request.JsonContent;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new MapsResponseDto
        {
            Id = catalog.Id,
            Name = catalog.Name,
            Type = catalog.Type,
            Difficulty = catalog.Difficulty,
            JsonContent = jsonContent,
            CreatedAt = catalog.CreatedAt,
            UpdatedAt = catalog.UpdatedAt
        };
        return Result<MapsResponseDto>.Success(dto, "Level updated successfully.");
    }
}

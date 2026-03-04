using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using MapEntity = CapstoneProject.Domain.Entities.Maps;

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

        var repo = _unitOfWork.Repository<MapEntity>();
        var entity = await repo.GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (entity == null)
            return Result<MapsResponseDto>.Failure("Map not found.", ErrorCodeEnum.NotFound);

        entity.Name = command.Request.Name ?? entity.Name;
        if (!string.IsNullOrWhiteSpace(command.Request.JsonContent))
        {
            entity.JsonContent = command.Request.JsonContent;
            try
            {
                using var doc = JsonDocument.Parse(command.Request.JsonContent);
                var root = doc.RootElement;
                if (root.TryGetProperty("id", out var idEl))
                    entity.ExternalId = idEl.GetString();
                if (root.TryGetProperty("name", out var nameEl))
                    entity.Name = nameEl.GetString() ?? entity.Name;
            }
            catch { /* keep existing name/externalId */ }
        }

        entity.UpdateEntity(userId);
        repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new MapsResponseDto
        {
            Id = entity.Id,
            ExternalId = entity.ExternalId,
            Name = entity.Name,
            JsonContent = entity.JsonContent,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
        return Result<MapsResponseDto>.Success(dto, "Map updated successfully.");
    }
}

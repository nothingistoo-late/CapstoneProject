using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Maps.Queries.GetMapsById;

public class GetMapsByIdQueryHandler : IRequestHandler<GetMapsByIdQuery, Result<MapsResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMapsByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MapsResponseDto>> Handle(GetMapsByIdQuery request, CancellationToken cancellationToken)
    {
        var catalogRepo = _unitOfWork.Repository<LevelCatalog>();
        var catalog = await catalogRepo.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (catalog == null)
            return Result<MapsResponseDto>.Failure("Level not found.", ErrorCodeEnum.NotFound);

        string? jsonContent = null;
        var detailRepo = _unitOfWork.Repository<LevelDetail>();
        var detail = await detailRepo.GetQueryable()
            .FirstOrDefaultAsync(x => x.LevelCatalogId == request.Id, cancellationToken);
        if (detail != null)
            jsonContent = detail.JsonContent;

        JsonElement? jsonContentElement = null;
        if (!string.IsNullOrWhiteSpace(jsonContent))
            jsonContentElement = JsonSerializer.Deserialize<JsonElement>(jsonContent);

        var dto = new MapsResponseDto
        {
            Id = catalog.Id,
            Name = catalog.Name,
            Type = catalog.Type,
            Difficulty = catalog.Difficulty,
            JsonContent = jsonContentElement,
            CreatedAt = catalog.CreatedAt,
            UpdatedAt = catalog.UpdatedAt
        };
        return Result<MapsResponseDto>.Success(dto, "Success");
    }
}

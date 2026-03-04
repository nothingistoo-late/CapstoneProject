using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Interfaces;
using MapEntity = CapstoneProject.Domain.Entities.Maps;

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
        var repo = _unitOfWork.Repository<MapEntity>();
        var entity = await repo.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity == null)
            return Result<MapsResponseDto>.Failure("Map not found.", ErrorCodeEnum.NotFound);

        var dto = new MapsResponseDto
        {
            Id = entity.Id,
            ExternalId = entity.ExternalId,
            Name = entity.Name,
            JsonContent = entity.JsonContent,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
        return Result<MapsResponseDto>.Success(dto, "Success");
    }
}

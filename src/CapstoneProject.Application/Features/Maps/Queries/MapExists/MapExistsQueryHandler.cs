using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Maps.Queries.MapExists;

public class MapExistsQueryHandler : IRequestHandler<MapExistsQuery, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public MapExistsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(MapExistsQuery request, CancellationToken cancellationToken)
    {
        if (request.MapId == Guid.Empty)
            return Result<bool>.Failure("Map Id is required.", ErrorCodeEnum.ValidationFailed);

        var exists = await _unitOfWork.Repository<Map>().GetQueryable()
            .AnyAsync(m => m.Id == request.MapId && !m.IsDeleted, cancellationToken);

        if (!exists)
            return Result<bool>.Failure("Map not found or has been deleted.", ErrorCodeEnum.NotFound);

        return Result<bool>.Success(true, "Map exists.");
    }
}

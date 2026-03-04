using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Maps.Queries.GetPagedMaps;

public class GetPagedMapsQueryHandler : IRequestHandler<GetPagedMapsQuery, PaginationResult<MapsListItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPagedMapsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginationResult<MapsListItemDto>> Handle(GetPagedMapsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var repo = _unitOfWork.Repository<LevelCatalog>();
        var query = repo.GetQueryable();
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Name != null && x.Name.ToLower().Contains(term));
        }
        if (filter.Status.HasValue)
            query = query.Where(x => x.Status == filter.Status.Value);

        var total = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var sortBy = (filter.SortBy ?? "CreatedAt").ToLowerInvariant();
        var asc = filter.IsAscending ?? false;

        query = sortBy switch
        {
            "name" => asc ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
            "updatedat" => asc ? query.OrderBy(x => x.UpdatedAt) : query.OrderByDescending(x => x.UpdatedAt),
            _ => asc ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt)
        };

        var list = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MapsListItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type,
                Difficulty = x.Difficulty,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return PaginationResult<MapsListItemDto>.Success(list, page, pageSize, total, "Retrieved successfully");
    }
}

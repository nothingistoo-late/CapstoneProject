using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Marketplace;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Marketplace.Queries.GetPackages;

public class GetPackagesQueryHandler : IRequestHandler<GetPackagesQuery, Result<PaginationResult<PackageDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPackagesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginationResult<PackageDto>>> Handle(GetPackagesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new PackageFilter();
        var query = _unitOfWork.Repository<Package>().GetQueryable()
            .Where(p => !p.IsDeleted)
            .AsNoTracking();

        if (filter.IsActive.HasValue)
            query = query.Where(p => p.Status == (filter.IsActive.Value ? EntityStatusEnum.Active : EntityStatusEnum.Inactive));
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(p => p.Name != null && p.Name.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var list = await query
            .OrderBy(p => p.Price)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PackageDto
            {
                Id = p.Id,
                Name = p.Name,
                DurationDays = p.DurationDays,
                Limit = p.Limit,
                Price = p.Price,
                FeaturesSpec = p.FeaturesSpec,
                Status = p.Status
            })
            .ToListAsync(cancellationToken);

        var result = PaginationResult<PackageDto>.Success(list, pageNumber, pageSize, total, "Đã truy xuất thành công");
        return Result<PaginationResult<PackageDto>>.Success(result, "Đã lấy danh sách gói.");
    }
}


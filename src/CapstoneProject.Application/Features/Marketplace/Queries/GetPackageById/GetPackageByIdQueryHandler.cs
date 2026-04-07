using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Marketplace;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Marketplace.Queries.GetPackageById;

public class GetPackageByIdQueryHandler : IRequestHandler<GetPackageByIdQuery, Result<PackageDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPackageByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PackageDto>> Handle(GetPackageByIdQuery request, CancellationToken cancellationToken)
    {
        var pkg = await _unitOfWork.Repository<Package>().GetQueryable()
            .Where(p => p.Id == request.PackageId && !p.IsDeleted)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (pkg == null)
            return Result<PackageDto>.Failure($"Không tìm thấy gói có Id: {request.PackageId}. Gói có thể đã bị xóa hoặc không tồn tại.", ErrorCodeEnum.NotFound);
        return Result<PackageDto>.Success(pkg, "Đã lấy thông tin gói.");
    }
}

using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Marketplace;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Marketplace.Queries.GetMyPackages;

public class GetMyPackagesQueryHandler : IRequestHandler<GetMyPackagesQuery, Result<PaginationResult<MyPackageDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyPackagesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<MyPackageDto>>> Handle(GetMyPackagesQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<PaginationResult<MyPackageDto>>.Failure(
                "Authentication required. Please log in to view your packages.",
                ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var f = request.Filter ?? new MyPackagesFilter();
        var now = VietnamDateTime.DbNow;
        var query = _unitOfWork.Repository<UserPackage>().GetQueryable()
            .AsNoTracking()
            .Where(up => up.UserId == userId && !up.IsDeleted);

        if (f.ActiveOnly == true)
        {
            query = query.Where(up =>
                ((up.Package != null && up.Package.Limit == null) || up.Remaining > 0) &&
                (up.ExpiresAt == null || up.ExpiresAt > now));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, f.PageNumber);
        var pageSize = Math.Clamp(f.PageSize, 1, 100);

        var list = await query
            .OrderByDescending(up => up.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(up => new MyPackageDto
            {
                UserPackageId = up.Id,
                PackageId = up.PackageId,
                Name = up.Package != null ? up.Package.Name : string.Empty,
                DurationDays = up.Package != null ? up.Package.DurationDays : 0,
                Limit = up.Package != null ? up.Package.Limit : null,
                Price = up.Package != null ? up.Package.Price : 0,
                FeaturesSpec = up.Package != null ? up.Package.FeaturesSpec : null,
                Remaining = up.Package != null && up.Package.Limit == null ? null : up.Remaining,
                ExpiresAt = up.ExpiresAt,
                PurchasedAt = up.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var paginated = PaginationResult<MyPackageDto>.Success(list, pageNumber, pageSize, total);
        return Result<PaginationResult<MyPackageDto>>.Success(paginated, "Đã lấy danh sách gói của bạn.");
    }
}


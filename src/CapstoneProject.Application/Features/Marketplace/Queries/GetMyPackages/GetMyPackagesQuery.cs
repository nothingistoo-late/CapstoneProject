using CapstoneProject.Application.Commons.DTOs.Marketplace;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Marketplace.Queries.GetMyPackages;

/// <param name="Filter">Null dùng mặc định giống query string rỗng.</param>
public record GetMyPackagesQuery(MyPackagesFilter? Filter = null) : IRequest<Result<PaginationResult<MyPackageDto>>>;

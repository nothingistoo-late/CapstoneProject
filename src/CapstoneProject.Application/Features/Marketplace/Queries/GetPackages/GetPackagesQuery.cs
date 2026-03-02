using CapstoneProject.Application.Commons.DTOs.Marketplace;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Marketplace.Queries.GetPackages;

public record GetPackagesQuery(PackageFilter? Filter = null) : IRequest<Result<PaginationResult<PackageDto>>>;

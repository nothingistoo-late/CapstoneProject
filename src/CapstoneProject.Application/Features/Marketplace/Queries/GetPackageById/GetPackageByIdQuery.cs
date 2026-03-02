using CapstoneProject.Application.Commons.DTOs.Marketplace;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Marketplace.Queries.GetPackageById;

public record GetPackageByIdQuery(Guid PackageId) : IRequest<Result<PackageDto>>;

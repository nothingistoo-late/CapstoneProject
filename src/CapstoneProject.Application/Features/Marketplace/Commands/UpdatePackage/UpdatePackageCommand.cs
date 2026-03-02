using CapstoneProject.Application.Commons.DTOs.Marketplace;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Marketplace.Commands.UpdatePackage;

public record UpdatePackageCommand(Guid PackageId, UpdatePackageRequest Request) : IRequest<Result>;

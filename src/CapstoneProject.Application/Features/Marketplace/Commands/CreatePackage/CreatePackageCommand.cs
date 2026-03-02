using CapstoneProject.Application.Commons.DTOs.Marketplace;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Marketplace.Commands.CreatePackage;

public record CreatePackageCommand(CreatePackageRequest Request) : IRequest<Result<Guid>>;

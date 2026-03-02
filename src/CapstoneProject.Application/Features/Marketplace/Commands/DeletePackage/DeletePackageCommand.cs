using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Marketplace.Commands.DeletePackage;

public record DeletePackageCommand(Guid PackageId) : IRequest<Result>;

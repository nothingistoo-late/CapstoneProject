using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Marketplace.Commands.BatchUpdatePackageStatus;

public record BatchUpdatePackageStatusCommand(List<Guid> PackageIds, bool IsActive) : IRequest<Result<BatchUpdatePackageStatusResultDto>>;

public class BatchUpdatePackageStatusResultDto
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<Guid> NotFoundIds { get; set; } = new();
}

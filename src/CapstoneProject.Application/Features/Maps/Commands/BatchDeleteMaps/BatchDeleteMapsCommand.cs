using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.BatchDeleteMaps;

public record BatchDeleteMapsCommand(List<Guid> Ids) : IRequest<Result<BatchDeleteMapsResultDto>>;

public class BatchDeleteMapsResultDto
{
    public int SuccessCount { get; set; }
    public int NotFoundCount { get; set; }
    public List<Guid> NotFoundIds { get; set; } = new();
}

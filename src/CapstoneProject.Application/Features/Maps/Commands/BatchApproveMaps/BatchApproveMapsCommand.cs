using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.BatchApproveMaps;

public record BatchApproveMapsCommand(List<Guid> MapIds, string? ReviewNote = null) : IRequest<Result<BatchMapResultDto>>;

public class BatchMapResultDto
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<Guid> NotFoundIds { get; set; } = new();
    public List<Guid> InvalidStatusIds { get; set; } = new();
}

using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.BatchCreateMaps;

public record BatchCreateMapsCommand(BatchCreateMapsRequest Request) : IRequest<Result<BatchCreateMapsResultDto>>;

public class BatchCreateMapsResultDto
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<Guid> CreatedIds { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

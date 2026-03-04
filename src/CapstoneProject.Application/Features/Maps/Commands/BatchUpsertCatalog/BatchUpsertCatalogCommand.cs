using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.BatchUpsertCatalog;

public record BatchUpsertCatalogCommand(BatchUpsertCatalogRequest Request) : IRequest<Result<BatchUpsertCatalogResultDto>>;

public class BatchUpsertCatalogResultDto
{
    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }
    public List<string> ExternalIds { get; set; } = new();
}

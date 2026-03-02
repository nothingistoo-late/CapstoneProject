using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Challenge.Queries.GetConcepts;

public record GetConceptsQuery(string? Search = null) : IRequest<Result<List<ConceptDto>>>;

public class ConceptDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

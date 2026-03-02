using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Challenge.Queries.GetTags;

public record GetTagsQuery(string? Search = null) : IRequest<Result<List<TagDto>>>;

public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

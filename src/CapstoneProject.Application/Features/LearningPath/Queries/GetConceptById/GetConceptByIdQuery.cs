using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetConceptById;

/// <summary>Lấy chi tiết một khái niệm theo Id.</summary>
public record GetConceptByIdQuery(Guid ConceptId) : IRequest<Result<ConceptDetailDto>>;

public class ConceptDetailDto
{
    public Guid Id { get; set; }
    public Guid LearningGoalId { get; set; }
    public string? LearningGoalName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContentKey { get; set; }
    public int SortOrder { get; set; }
}

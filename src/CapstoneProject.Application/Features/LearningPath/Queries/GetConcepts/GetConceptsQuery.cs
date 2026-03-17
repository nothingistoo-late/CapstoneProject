using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetConcepts;

public class ConceptDto
{
    public Guid Id { get; set; }
    public Guid LearningGoalId { get; set; }
    public string? LearningGoalName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>Key để FE load nội dung (vd. content/variables.md).</summary>
    public string? ContentKey { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>Lấy danh sách khái niệm (concept). Có thể lọc theo LearningGoalId.</summary>
public record GetConceptsQuery(Guid? LearningGoalId = null) : IRequest<Result<List<ConceptDto>>>;

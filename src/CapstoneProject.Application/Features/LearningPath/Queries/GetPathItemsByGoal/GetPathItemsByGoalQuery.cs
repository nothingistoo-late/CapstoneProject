using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetPathItemsByGoal;

/// <summary>Một item trong lộ trình (chỉ cấu trúc, không có trạng thái user). Dùng để xem trước lộ trình.</summary>
public class PathItemPreviewDto
{
    public Guid ItemId { get; set; }
    public string ItemType { get; set; } = string.Empty; // Concept | Map
    public int SortOrder { get; set; }
    public Guid? ConceptId { get; set; }
    public string? ConceptName { get; set; }
    public string? ConceptDescription { get; set; }
    public string? ConceptContentKey { get; set; }
    public Guid? MapId { get; set; }
    public string? MapTitle { get; set; }
    public string? MapDescription { get; set; }
    public int? MapDifficulty { get; set; }
    public string? MapAvatarUrl { get; set; }
}

/// <summary>Lấy danh sách item trong lộ trình của một goal (cấu trúc only, không cần auth). Để FE xem trước "Lộ trình này gồm những gì".</summary>
public record GetPathItemsByGoalQuery(Guid LearningGoalId) : IRequest<Result<List<PathItemPreviewDto>>>;

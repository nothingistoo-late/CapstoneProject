using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetMyLearningPath;

public class LearningPathItemDto
{
    public Guid ItemId { get; set; }
    /// <summary>Concept hoặc Map.</summary>
    public string ItemType { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public Guid? ConceptId { get; set; }
    public string? ConceptName { get; set; }
    public string? ConceptDescription { get; set; }
    /// <summary>Key để FE load nội dung từ file tĩnh/bundle (vd. "variables" → content/variables.md).</summary>
    public string? ConceptContentKey { get; set; }
    public Guid? MapId { get; set; }
    public string? MapTitle { get; set; }
    public string? MapDescription { get; set; }
    public int? MapDifficulty { get; set; }
    public string? MapAvatarUrl { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsUnlocked { get; set; }
    public int? BestStars { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class MyLearningPathDto
{
    public Guid? LearningGoalId { get; set; }
    public string? LearningGoalName { get; set; }
    public string? LearningGoalDescription { get; set; }
    public List<LearningPathItemDto> Items { get; set; } = new();
}

/// <summary>Lấy lộ trình học của user hiện tại: mục tiêu đã chọn + danh sách concept/map theo thứ tự, trạng thái hoàn thành và mở khóa.</summary>
public record GetMyLearningPathQuery : IRequest<Result<MyLearningPathDto>>;

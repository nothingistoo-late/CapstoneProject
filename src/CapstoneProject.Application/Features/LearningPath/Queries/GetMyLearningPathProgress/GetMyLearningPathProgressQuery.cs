using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetMyLearningPathProgress;

public class LearningPathProgressDto
{
    public Guid? LearningGoalId { get; set; }
    public string? LearningGoalName { get; set; }
    public int TotalItems { get; set; }
    public int CompletedCount { get; set; }
    public int PercentComplete { get; set; }
    /// <summary>Game IDs gợi ý ôn tập (điểm thấp hoặc chưa đạt sao).</summary>
    public List<Guid> SuggestedReviewGameIds { get; set; } = new();
}

/// <summary>Lấy tiến độ lộ trình: tổng số item, đã hoàn thành bao nhiêu, % hoàn thành, gợi ý ôn tập.</summary>
public record GetMyLearningPathProgressQuery : IRequest<Result<LearningPathProgressDto>>;

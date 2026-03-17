using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetLearningGoals;

public record LearningGoalDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public string? IconUrl { get; set; }
}

/// <summary>Lấy danh sách mục tiêu học tập để user chọn (Logic cơ bản, Điều kiện, Vòng lặp, Giải quyết vấn đề...).</summary>
public record GetLearningGoalsQuery : IRequest<Result<List<LearningGoalDto>>>;

using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetSelectedGoal;

/// <summary>Mục tiêu học tập user đang chọn. Null nếu chưa chọn.</summary>
public class SelectedGoalDto
{
    public Guid LearningGoalId { get; set; }
    public string LearningGoalName { get; set; } = string.Empty;
    public string? LearningGoalDescription { get; set; }
}

/// <summary>Lấy mục tiêu học tập mà user hiện tại đang chọn. FE dùng cho header / breadcrumb mà không cần gọi full my-path.</summary>
public record GetSelectedGoalQuery : IRequest<Result<SelectedGoalDto?>>;

using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.LearningPath.Commands.SelectLearningGoal;

/// <summary>User chọn mục tiêu học tập (khi đăng nhập / vào dashboard). Tạo hoặc cập nhật UserLearningGoal.</summary>
public record SelectLearningGoalCommand(Guid LearningGoalId) : IRequest<Result>;

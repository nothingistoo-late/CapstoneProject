using CapstoneProject.Application.Common.Models;
using MediatR;
using CapstoneProject.Application.Features.LearningPath.Queries.GetLearningGoals;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetLearningGoalById;

/// <summary>Lấy chi tiết một mục tiêu học tập theo Id.</summary>
public record GetLearningGoalByIdQuery(Guid GoalId) : IRequest<Result<LearningGoalDto>>;

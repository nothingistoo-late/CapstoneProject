using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.LearningPath.Commands.CompleteConcept;

/// <summary>Đánh dấu user đã hoàn thành một khái niệm (đọc xong / làm xong bài tập nhỏ). Mở khóa item tiếp theo trong lộ trình.</summary>
public record CompleteConceptCommand(Guid ConceptId) : IRequest<Result>;

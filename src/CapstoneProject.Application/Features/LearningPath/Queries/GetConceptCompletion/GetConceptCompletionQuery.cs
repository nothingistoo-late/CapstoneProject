using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetConceptCompletion;

/// <summary>Trạng thái hoàn thành concept của user hiện tại. FE dùng để hiển thị "Đã hoàn thành" trên trang chi tiết concept.</summary>
public class ConceptCompletionDto
{
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public record GetConceptCompletionQuery(Guid ConceptId) : IRequest<Result<ConceptCompletionDto>>;

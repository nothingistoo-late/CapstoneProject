using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetConcepts;

public class GetConceptsQueryHandler : IRequestHandler<GetConceptsQuery, Result<List<ConceptDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetConceptsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<ConceptDto>>> Handle(GetConceptsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Concept>().GetQueryable()
            .Where(c => !c.IsDeleted && c.Status == EntityStatusEnum.Active);

        if (request.LearningGoalId.HasValue)
            query = query.Where(c => c.LearningGoalId == request.LearningGoalId.Value);

        var list = await query
            .OrderBy(c => c.SortOrder)
            .Select(c => new ConceptDto
            {
                Id = c.Id,
                LearningGoalId = c.LearningGoalId,
                LearningGoalName = c.LearningGoal != null ? c.LearningGoal.Name : null,
                Name = c.Name,
                Description = c.Description,
                ContentKey = c.ContentKey,
                SortOrder = c.SortOrder
            })
            .ToListAsync(cancellationToken);

        return Result<List<ConceptDto>>.Success(list, "Đã lấy danh sách khái niệm.");
    }
}


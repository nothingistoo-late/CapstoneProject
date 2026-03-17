using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetConceptById;

public class GetConceptByIdQueryHandler : IRequestHandler<GetConceptByIdQuery, Result<ConceptDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetConceptByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<ConceptDetailDto>> Handle(GetConceptByIdQuery request, CancellationToken cancellationToken)
    {
        var concept = await _unitOfWork.Repository<Concept>().GetQueryable()
            .Where(c => c.Id == request.ConceptId && !c.IsDeleted && c.Status == EntityStatusEnum.Active)
            .Select(c => new ConceptDetailDto
            {
                Id = c.Id,
                LearningGoalId = c.LearningGoalId,
                LearningGoalName = c.LearningGoal != null ? c.LearningGoal.Name : null,
                Name = c.Name,
                Description = c.Description,
                ContentKey = c.ContentKey,
                SortOrder = c.SortOrder
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (concept == null)
            return Result<ConceptDetailDto>.Failure("Concept not found.", ErrorCodeEnum.NotFound);

        return Result<ConceptDetailDto>.Success(concept);
    }
}

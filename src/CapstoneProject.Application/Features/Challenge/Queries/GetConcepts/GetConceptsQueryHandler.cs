using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Challenge.Queries.GetConcepts;

public class GetConceptsQueryHandler : IRequestHandler<GetConceptsQuery, Result<List<ConceptDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetConceptsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<ConceptDto>>> Handle(GetConceptsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Concept>().GetQueryable()
            .Where(c => !c.IsDeleted && c.Status == Domain.Enums.EntityStatusEnum.Active);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(c => (c.Name != null && c.Name.ToLower().Contains(term)) || (c.Description != null && c.Description.ToLower().Contains(term)));
        }
        var list = await query
            .OrderBy(c => c.Name)
            .Select(c => new ConceptDto { Id = c.Id, Name = c.Name, Description = c.Description })
            .ToListAsync(cancellationToken);
        return Result<List<ConceptDto>>.Success(list);
    }
}

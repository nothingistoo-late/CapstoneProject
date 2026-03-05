using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Maps.Queries.GetTags;

public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, Result<List<TagDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTagsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<TagDto>>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Tag>().GetQueryable()
            .Where(t => !t.IsDeleted && t.Status == Domain.Enums.EntityStatusEnum.Active);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(t => t.Name != null && t.Name.ToLower().Contains(term));
        }
        var list = await query
            .OrderBy(t => t.Name)
            .Select(t => new TagDto { Id = t.Id, Name = t.Name })
            .ToListAsync(cancellationToken);
        return Result<List<TagDto>>.Success(list);
    }
}

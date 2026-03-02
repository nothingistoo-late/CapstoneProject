using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Gameplay.Queries.GetHintsForMap;

public class GetHintsForMapQueryHandler : IRequestHandler<GetHintsForMapQuery, Result<List<HintLevelDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetHintsForMapQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<HintLevelDto>>> Handle(GetHintsForMapQuery request, CancellationToken cancellationToken)
    {
        var hints = await _unitOfWork.Repository<Hint>().GetQueryable()
            .Where(h => h.MapId == request.MapId && !h.IsDeleted)
            .OrderBy(h => h.OrderNo)
            .Select(h => new HintLevelDto { OrderNo = h.OrderNo, Content = h.Content })
            .ToListAsync(cancellationToken);
        return Result<List<HintLevelDto>>.Success(hints);
    }
}

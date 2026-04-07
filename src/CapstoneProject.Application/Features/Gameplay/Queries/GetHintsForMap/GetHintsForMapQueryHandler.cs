using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Gameplay.Queries.GetHintsForMap;

public class GetHintsForMapQueryHandler : IRequestHandler<GetHintsForMapQuery, Result<List<HintLevelDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetHintsForMapQueryHandler(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

    public async Task<Result<List<HintLevelDto>>> Handle(GetHintsForMapQuery request, CancellationToken cancellationToken)
    {
        var q = _unitOfWork.Repository<Hint>().GetQueryable()
            .AsNoTracking()
            .Where(h => !h.IsDeleted && h.MapDetail.MapId == request.MapId && !h.MapDetail.IsDeleted);

        if (request.MapDetailId.HasValue)
            q = q.Where(h => h.MapDetailId == request.MapDetailId.Value);

        var hints = await q
            .OrderBy(h => h.MapDetail.LevelOrder)
            .ThenBy(h => h.OrderNo)
            .Select(h => new HintLevelDto
            {
                LevelOrder = h.MapDetail.LevelOrder,
                MapDetailId = h.MapDetailId,
                OrderNo = h.OrderNo,
                Content = h.Content
            })
            .ToListAsync(cancellationToken);

        return Result<List<HintLevelDto>>.Success(hints, "Đã lấy gợi ý cho bản đồ.");
    }
}


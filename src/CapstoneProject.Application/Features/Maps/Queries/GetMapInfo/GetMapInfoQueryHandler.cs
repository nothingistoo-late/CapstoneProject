using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Queries.GetMapInfo;

public class GetMapInfoQueryHandler : IRequestHandler<GetMapInfoQuery, Result<MapInfoDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMapInfoQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MapInfoDto>> Handle(GetMapInfoQuery request, CancellationToken cancellationToken)
    {
        var map = await _unitOfWork.Repository<Map>().GetQueryable()
            .Where(m => m.Id == request.MapId && !m.IsDeleted)
            .Include(m => m.MapTags).ThenInclude(mt => mt.Tag)
            .Include(m => m.Creator)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
        if (map == null)
            return Result<MapInfoDto>.Failure($"Map not found with Id: {request.MapId}.", ErrorCodeEnum.NotFound);

        var learnedTagNameMap = await _unitOfWork.Repository<Tag>().GetQueryable()
            .Where(t => map.LearnedTags.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name, cancellationToken);

        var dto = new MapInfoDto
        {
            Id = map.Id,
            Title = map.Title,
            Description = map.Description,
            Difficulty = map.Difficulty,
            Type = map.Type.ToString(),
            TimeLimitMs = map.TimeLimitMs,
            IsPublished = map.IsPublished,
            MapStatus = map.MapStatus.ToString(),
            Price = map.Price,
            CreatedByUserId = map.CreatedBy ?? Guid.Empty,
            CreatedByUserName = map.Creator != null ? $"{map.Creator.FirstName} {map.Creator.LastName}".Trim() : null,
            CreatedAt = map.CreatedAt,
            TagNames = map.MapTags.Select(t => t.Tag.Name).ToList(),
            LearnedTags = map.LearnedTags
                .Select(id => learnedTagNameMap.TryGetValue(id, out var name) ? name : id.ToString())
                .ToList(),
            WinCondition = map.WinCondition,
            AvatarUrl = map.AvatarUrl
        };
        return Result<MapInfoDto>.Success(dto);
    }
}

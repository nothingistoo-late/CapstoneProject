using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Maps.Queries.GetMapById;

public class GetMapByIdQueryHandler : IRequestHandler<GetMapByIdQuery, Result<MapDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMapByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MapDetailDto>> Handle(GetMapByIdQuery request, CancellationToken cancellationToken)
    {
        var map = await _unitOfWork.Repository<Map>().GetQueryable()
            .Where(m => m.Id == request.MapId && !m.IsDeleted)
            .Include(m => m.MapDetail)
            .Include(m => m.Hints)
            .Include(m => m.MapTags).ThenInclude(mt => mt.Tag)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
        if (map == null)
            return Result<MapDetailDto>.Failure($"Map not found with Id: {request.MapId}.", ErrorCodeEnum.NotFound);

        bool showEditorial = false;
        if (request.IncludeEditorialForUser)
        {
            var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
            if (isValid && userIdNullable.HasValue)
            {
                var umr = await _unitOfWork.Repository<UserMapResult>().GetQueryable()
                    .FirstOrDefaultAsync(u => u.UserId == userIdNullable.Value && u.MapId == map.Id, cancellationToken);
                if (umr != null && umr.BestStars >= map.UnlockEditorialAfterStars) showEditorial = true;
            }
        }

        var dto = new MapDetailDto
        {
            Id = map.Id,
            Title = map.Title,
            Description = map.Description,
            Difficulty = map.Difficulty,
            Type = map.Type.ToString(),
            TimeLimitMs = map.TimeLimitMs,
            IsPublished = map.IsPublished,
            MapStatus = map.MapStatus,
            Price = map.Price,
            CreatedByUserId = map.CreatedBy ?? Guid.Empty,
            EditorialContent = showEditorial ? map.EditorialContent : null,
            UnlockEditorialAfterStars = map.UnlockEditorialAfterStars,
            CreatedAt = map.CreatedAt,
            MapDetailJson = ParseMapDetailJson(map.MapDetail?.JsonContent),
            Hints = map.Hints.OrderBy(h => h.OrderNo).Select(h => new HintItemDto { OrderNo = h.OrderNo, Content = h.Content }).ToList(),
            TagNames = map.MapTags.Select(t => t.Tag.Name).ToList(),
            WinCondition = map.WinCondition
        };
        return Result<MapDetailDto>.Success(dto);
    }

    private static JsonElement? ParseMapDetailJson(string? jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent)) return null;
        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            return doc.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Queries.GetMapInfo;

public class GetMapInfoQueryHandler : IRequestHandler<GetMapInfoQuery, Result<MapInfoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMapInfoQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MapInfoDto>> Handle(GetMapInfoQuery request, CancellationToken cancellationToken)
    {
        var map = await _unitOfWork.Repository<Map>().GetQueryable()
            .Where(m => m.Id == request.MapId && m.Status == EntityStatusEnum.Active)
            .Include(m => m.MapDetails)
            .Include(m => m.MapTags).ThenInclude(mt => mt.Tag)
            .Include(m => m.Creator)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
        if (map == null)
            return Result<MapInfoDto>.Failure($"Không tìm thấy bản đồ có Id: {request.MapId}.", ErrorCodeEnum.NotFound);

        if (map.IsDeleted)
        {
            var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
            if (!isValid || !userIdNullable.HasValue)
                return Result<MapInfoDto>.Failure($"Không tìm thấy bản đồ có Id: {request.MapId}.", ErrorCodeEnum.NotFound);

            var userId = userIdNullable.Value;
            var isAuthor = map.CreatedBy.HasValue && map.CreatedBy.Value == userId;
            var isOwned = isAuthor;
            if (!isOwned)
            {
                var purchased = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                    .AnyAsync(p => !p.IsDeleted && p.UserId == userId && p.MapId == map.Id && p.PaymentStatus == PaymentStatusEnum.Completed, cancellationToken);
                if (purchased)
                    isOwned = true;
                else
                    isOwned = await _unitOfWork.Repository<MyMap>().GetQueryable()
                        .AnyAsync(mm => !mm.IsDeleted && mm.UserId == userId && mm.MapId == map.Id, cancellationToken);
            }

            if (!isOwned)
                return Result<MapInfoDto>.Failure($"Không tìm thấy bản đồ có Id: {request.MapId}.", ErrorCodeEnum.NotFound);
        }

        var learnedTagNameMap = await _unitOfWork.Repository<Tag>().GetQueryable()
            .Where(t => map.LearnedTags.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name, cancellationToken);

        var (tLimit, win, mapType) = MapFirstLevelHelper.FirstLevelMetadata(map.MapDetails);
        var dto = new MapInfoDto
        {
            Id = map.Id,
            Title = map.Title,
            Description = map.Description,
            Difficulty = map.Difficulty,
            Type = mapType.ToString(),
            TimeLimitMs = tLimit,
            IsPublished = map.IsPublished,
            MapStatus = map.MapStatus.ToString(),
            Price = map.Price,
            CreatedByUserId = map.CreatedBy ?? Guid.Empty,
            CreatedByUserName = map.Creator != null ? $"{map.Creator.FirstName} {map.Creator.LastName}".Trim() : null,
            CreatedAt = map.CreatedAt,
            UpdatedAt = map.UpdatedAt,
            ContentVersion = map.ContentVersion,
            TagNames = map.MapTags.Select(t => t.Tag.Name).ToList(),
            LearnedTags = map.LearnedTags
                .Select(id => learnedTagNameMap.TryGetValue(id, out var name) ? name : id.ToString())
                .ToList(),
            WinCondition = win,
            AvatarUrl = map.AvatarUrl
        };
        return Result<MapInfoDto>.Success(dto, "Đã lấy thông tin bản đồ.");
    }
}

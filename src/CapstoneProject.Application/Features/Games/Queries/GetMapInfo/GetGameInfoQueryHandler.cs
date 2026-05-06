using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Games.Queries.GetMapInfo;

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
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        var userId = isValid && userIdNullable.HasValue ? userIdNullable.Value : (Guid?)null;

        var game = await _unitOfWork.Repository<Game>().GetQueryable()
            .Where(m => m.Id == request.GameId
                        && (m.Status == EntityStatusEnum.Active
                            || (userId.HasValue && m.CreatedBy == userId)))
            .Include(m => m.GameDetails)
            .Include(m => m.GameMedias)
            .Include(m => m.GameTags).ThenInclude(mt => mt.Tag)
            .Include(m => m.Creator)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
        if (game == null)
            return Result<MapInfoDto>.Failure($"Không tìm thấy trò chơi có Id: {request.GameId}.", ErrorCodeEnum.NotFound);

        if (game.IsDeleted)
        {
            if (!isValid || !userIdNullable.HasValue)
                return Result<MapInfoDto>.Failure($"Không tìm thấy trò chơi có Id: {request.GameId}.", ErrorCodeEnum.NotFound);

            var currentUserId = userIdNullable.Value;
            var isAuthor = game.CreatedBy.HasValue && game.CreatedBy.Value == currentUserId;
            var isOwned = isAuthor;
            if (!isOwned)
            {
                var purchased = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                    .AnyAsync(p => !p.IsDeleted && p.UserId == currentUserId && p.GameId == game.Id && p.PaymentStatus == PaymentStatusEnum.Completed, cancellationToken);
                if (purchased)
                    isOwned = true;
                else
                    isOwned = await _unitOfWork.Repository<MyGame>().GetQueryable()
                        .AnyAsync(mm => !mm.IsDeleted && mm.UserId == currentUserId && mm.GameId == game.Id, cancellationToken);
            }

            if (!isOwned)
                return Result<MapInfoDto>.Failure($"Không tìm thấy trò chơi có Id: {request.GameId}.", ErrorCodeEnum.NotFound);
        }

        var learnedTagNameMap = await _unitOfWork.Repository<Tag>().GetQueryable()
            .Where(t => game.LearnedTags.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name, cancellationToken);

        var (tLimit, win, mapType) = MapFirstLevelHelper.FirstLevelMetadata(game.GameDetails);
        var levelDtos = game.GameDetails
            .OrderBy(d => d.LevelOrder)
            .Select(d => new MapLevelItemDto
            {
                Id = d.Id,
                LevelOrder = d.LevelOrder,
                Title = d.Title,
                TimeLimitMs = d.TimeLimitMs,
                WinCondition = d.WinCondition,
                Type = d.Type.ToString(),
                DetailJson = null,
                Hints = new List<HintItemDto>()
            })
            .ToList();
        var dto = new MapInfoDto
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            Difficulty = game.Difficulty,
            Type = mapType.ToString(),
            TimeLimitMs = tLimit,
            IsPublished = game.IsPublished,
            GameStatus = game.GameStatus.ToString(),
            Price = game.Price,
            CreatedByUserId = game.CreatedBy ?? Guid.Empty,
            CreatedByUserName = game.Creator != null ? $"{game.Creator.FirstName} {game.Creator.LastName}".Trim() : null,
            CreatedAt = game.CreatedAt,
            UpdatedAt = game.UpdatedAt,
            ContentVersion = game.ContentVersion,
            TagNames = game.GameTags.Select(t => t.Tag.Name).ToList(),
            LearnedTags = game.LearnedTags
                .Select(id => learnedTagNameMap.TryGetValue(id, out var name) ? name : id.ToString())
                .ToList(),
            WinCondition = win,
            AvatarUrl = game.AvatarUrl,
            Gallery = game.GameMedias
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.SortOrder)
                .Select(x => new GameMediaItemDto
                {
                    Id = x.Id,
                    Url = x.Url,
                    Kind = x.Kind.ToString(),
                    SortOrder = x.SortOrder
                })
                .ToList(),
            Levels = levelDtos
        };
        return Result<MapInfoDto>.Success(dto, "Đã lấy thông tin trò chơi.");
    }
}

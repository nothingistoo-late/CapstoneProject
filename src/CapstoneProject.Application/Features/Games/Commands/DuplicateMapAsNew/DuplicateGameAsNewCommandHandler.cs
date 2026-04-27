using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Games.Commands.DuplicateMapAsNew;

public class DuplicateMapAsNewCommandHandler : IRequestHandler<DuplicateMapAsNewCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DuplicateMapAsNewCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(DuplicateMapAsNewCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<Guid>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var roles = await _currentUserService.GetCurrentRolesAsync();
        var isAdminOrMod = roles.Contains(RoleEnum.Admin) || roles.Contains(RoleEnum.Moderator);

        var mapRepo = _unitOfWork.Repository<Game>();
        var source = await mapRepo.GetQueryable()
            .AsNoTracking()
            .Include(m => m.GameDetails)
            .ThenInclude(d => d.Hints)
            .Include(m => m.GameTags)
            .Include(m => m.GameMedias)
            .FirstOrDefaultAsync(m => m.Id == command.SourceGameId && !m.IsDeleted, cancellationToken);

        if (source == null)
            return Result<Guid>.Failure($"Không tìm thấy trò chơi có Id: {command.SourceGameId}.", ErrorCodeEnum.NotFound);

        if (source.CreatedBy != userId && !isAdminOrMod)
            return Result<Guid>.Failure("Bạn không được phép sao chép trò chơi này.", ErrorCodeEnum.Forbidden);

        var details = source.GameDetails.Where(d => !d.IsDeleted).OrderBy(d => d.LevelOrder).ToList();
        if (details.Count == 0)
            return Result<Guid>.Failure("Trò chơi nguồn không có cấp độ để sao chép.", ErrorCodeEnum.ValidationFailed);

        var req = command.Request ?? new DuplicateMapAsNewRequest();
        var title = string.IsNullOrWhiteSpace(req.Title)
            ? $"{source.Title} (Copy)"
            : req.Title.Trim();
        if (title.Length > 200)
            return Result<Guid>.Failure("Tiêu đề không được vượt quá 200 ký tự.", ErrorCodeEnum.ValidationFailed);

        var autoPublish = req.AutoPublish;
        var newMap = new Game
        {
            Title = title,
            Description = req.Description ?? source.Description,
            Difficulty = req.Difficulty ?? source.Difficulty,
            Price = req.Price ?? source.Price,
            IsPublished = autoPublish,
            GameStatus = autoPublish ? GameStatusEnum.Published : GameStatusEnum.Draft,
            LearnedTags = req.LearnedTags != null
                ? new List<Guid>(req.LearnedTags)
                : new List<Guid>(source.LearnedTags),
            AvatarUrl = source.AvatarUrl,
            EditorialContent = req.EditorialContent ?? source.EditorialContent,
            UnlockEditorialAfterStars = req.UnlockEditorialAfterStars ?? source.UnlockEditorialAfterStars,
            ContentVersion = 1,
            IsActiveVersion = true
        };
        newMap.InitializeEntity(userId);
        newMap.RootGameId = newMap.Id;
        await mapRepo.AddAsync(newMap);

        if (req.TagIds != null)
        {
            foreach (var tagId in req.TagIds)
            {
                var mapTag = new GameTag { GameId = newMap.Id, TagId = tagId };
                mapTag.InitializeEntity(userId);
                await _unitOfWork.Repository<GameTag>().AddAsync(mapTag);
            }
        }
        else
        {
            foreach (var mt in source.GameTags.Where(t => !t.IsDeleted))
            {
                var mapTag = new GameTag { GameId = newMap.Id, TagId = mt.TagId };
                mapTag.InitializeEntity(userId);
                await _unitOfWork.Repository<GameTag>().AddAsync(mapTag);
            }
        }

        var detailRepo = _unitOfWork.Repository<GameDetail>();
        var hintRepo = _unitOfWork.Repository<Hint>();
        foreach (var d in details)
        {
            var newDetail = new GameDetail
            {
                GameId = newMap.Id,
                LevelOrder = d.LevelOrder,
                Title = d.Title,
                JsonContent = d.JsonContent,
                TimeLimitMs = d.TimeLimitMs,
                WinCondition = d.WinCondition,
                Type = d.Type
            };
            newDetail.InitializeEntity(userId);
            await detailRepo.AddAsync(newDetail);

            foreach (var h in d.Hints.Where(x => !x.IsDeleted).OrderBy(x => x.OrderNo))
            {
                var hint = new Hint { GameDetailId = newDetail.Id, OrderNo = h.OrderNo, Content = h.Content };
                hint.InitializeEntity(userId);
                await hintRepo.AddAsync(hint);
            }
        }

        foreach (var mm in source.GameMedias.Where(m => !m.IsDeleted).OrderBy(m => m.SortOrder))
        {
            var media = new GameMedia
            {
                GameId = newMap.Id,
                Url = mm.Url,
                Kind = mm.Kind,
                SortOrder = mm.SortOrder
            };
            media.InitializeEntity(userId);
            await _unitOfWork.Repository<GameMedia>().AddAsync(media);
        }

        var myMap = new MyGame { GameId = newMap.Id, UserId = userId, IsAuthor = true };
        myMap.InitializeEntity(userId);
        await _unitOfWork.Repository<MyGame>().AddAsync(myMap);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var msg = autoPublish
            ? "Game duplicated as a new listing and published. Source game was not modified."
            : "Game duplicated as a new draft. Source game was not modified.";
        return Result<Guid>.Success(newMap.Id, msg);
    }
}

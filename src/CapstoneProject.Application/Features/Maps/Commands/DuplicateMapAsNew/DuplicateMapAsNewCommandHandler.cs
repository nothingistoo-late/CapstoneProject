using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Maps.Commands.DuplicateMapAsNew;

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

        var mapRepo = _unitOfWork.Repository<Map>();
        var source = await mapRepo.GetQueryable()
            .AsNoTracking()
            .Include(m => m.MapDetails)
            .ThenInclude(d => d.Hints)
            .Include(m => m.MapTags)
            .Include(m => m.MapMedias)
            .FirstOrDefaultAsync(m => m.Id == command.SourceMapId && !m.IsDeleted, cancellationToken);

        if (source == null)
            return Result<Guid>.Failure($"Không tìm thấy bản đồ có Id: {command.SourceMapId}.", ErrorCodeEnum.NotFound);

        if (source.CreatedBy != userId && !isAdminOrMod)
            return Result<Guid>.Failure("Bạn không được phép sao chép bản đồ này.", ErrorCodeEnum.Forbidden);

        var details = source.MapDetails.Where(d => !d.IsDeleted).OrderBy(d => d.LevelOrder).ToList();
        if (details.Count == 0)
            return Result<Guid>.Failure("Bản đồ nguồn không có cấp độ để sao chép.", ErrorCodeEnum.ValidationFailed);

        var req = command.Request ?? new DuplicateMapAsNewRequest();
        var title = string.IsNullOrWhiteSpace(req.Title)
            ? $"{source.Title} (Copy)"
            : req.Title.Trim();
        if (title.Length > 200)
            return Result<Guid>.Failure("Tiêu đề không được vượt quá 200 ký tự.", ErrorCodeEnum.ValidationFailed);

        var autoPublish = req.AutoPublish;
        var newMap = new Map
        {
            Title = title,
            Description = req.Description ?? source.Description,
            Difficulty = req.Difficulty ?? source.Difficulty,
            Price = req.Price ?? source.Price,
            IsPublished = autoPublish,
            MapStatus = autoPublish ? MapStatusEnum.Published : MapStatusEnum.Draft,
            LearnedTags = req.LearnedTags != null
                ? new List<Guid>(req.LearnedTags)
                : new List<Guid>(source.LearnedTags),
            AvatarUrl = source.AvatarUrl,
            EditorialContent = req.EditorialContent ?? source.EditorialContent,
            UnlockEditorialAfterStars = req.UnlockEditorialAfterStars ?? source.UnlockEditorialAfterStars,
            ContentVersion = 1
        };
        newMap.InitializeEntity(userId);
        await mapRepo.AddAsync(newMap);

        if (req.TagIds != null)
        {
            foreach (var tagId in req.TagIds)
            {
                var mapTag = new MapTag { MapId = newMap.Id, TagId = tagId };
                mapTag.InitializeEntity(userId);
                await _unitOfWork.Repository<MapTag>().AddAsync(mapTag);
            }
        }
        else
        {
            foreach (var mt in source.MapTags.Where(t => !t.IsDeleted))
            {
                var mapTag = new MapTag { MapId = newMap.Id, TagId = mt.TagId };
                mapTag.InitializeEntity(userId);
                await _unitOfWork.Repository<MapTag>().AddAsync(mapTag);
            }
        }

        var detailRepo = _unitOfWork.Repository<MapDetail>();
        var hintRepo = _unitOfWork.Repository<Hint>();
        foreach (var d in details)
        {
            var newDetail = new MapDetail
            {
                MapId = newMap.Id,
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
                var hint = new Hint { MapDetailId = newDetail.Id, OrderNo = h.OrderNo, Content = h.Content };
                hint.InitializeEntity(userId);
                await hintRepo.AddAsync(hint);
            }
        }

        foreach (var mm in source.MapMedias.Where(m => !m.IsDeleted).OrderBy(m => m.SortOrder))
        {
            var media = new MapMedia
            {
                MapId = newMap.Id,
                Url = mm.Url,
                Kind = mm.Kind,
                SortOrder = mm.SortOrder
            };
            media.InitializeEntity(userId);
            await _unitOfWork.Repository<MapMedia>().AddAsync(media);
        }

        var myMap = new MyMap { MapId = newMap.Id, UserId = userId, IsAuthor = true };
        myMap.InitializeEntity(userId);
        await _unitOfWork.Repository<MyMap>().AddAsync(myMap);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var msg = autoPublish
            ? "Map duplicated as a new listing and published. Source map was not modified."
            : "Map duplicated as a new draft. Source map was not modified.";
        return Result<Guid>.Success(newMap.Id, msg);
    }
}

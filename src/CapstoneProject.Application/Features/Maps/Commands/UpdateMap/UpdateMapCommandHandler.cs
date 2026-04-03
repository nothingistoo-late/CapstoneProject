using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Commands.UpdateMap;

public class UpdateMapCommandHandler : IRequestHandler<UpdateMapCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMapCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Authentication required. Please log in to update a map.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var mapRepo = _unitOfWork.Repository<Map>();
        var map = await mapRepo.GetQueryable()
            .Include(m => m.MapDetails).ThenInclude(d => d.Hints)
            .Include(m => m.MapTags)
            .FirstOrDefaultAsync(m => m.Id == command.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result.Failure($"Map not found with Id: {command.MapId}.", ErrorCodeEnum.NotFound);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        bool isAdminOrMod = roles.Contains(RoleEnum.Admin) || roles.Contains(RoleEnum.Moderator);
        if (map.CreatedBy != userId && !isAdminOrMod)
            return Result.Failure("You do not have permission to update this map.", ErrorCodeEnum.Forbidden);

        var req = command.Request;
        map.Title = req.Title;
        map.Description = req.Description;
        map.Difficulty = req.Difficulty;
        map.Price = req.Price;
        map.EditorialContent = req.EditorialContent;
        if (req.UnlockEditorialAfterStars.HasValue)
            map.UnlockEditorialAfterStars = req.UnlockEditorialAfterStars.Value;
        if (req.LearnedTags != null)
            map.LearnedTags = req.LearnedTags;
        if (req.AvatarUrl != null)
            map.AvatarUrl = req.AvatarUrl;

        map.MapStatus = MapStatusEnum.Draft;
        map.IsPublished = false;
        map.UpdateEntity(userId);

        if (req.TagIds != null)
        {
            foreach (var mt in map.MapTags.ToList()) _unitOfWork.Repository<MapTag>().Delete(mt);
            foreach (var tagId in req.TagIds)
            {
                var mapTag = new MapTag { MapId = map.Id, TagId = tagId };
                mapTag.InitializeEntity(userId);
                await _unitOfWork.Repository<MapTag>().AddAsync(mapTag);
            }
        }

        if (req.Levels is { Count: > 0 })
        {
            foreach (var d in map.MapDetails.ToList())
                _unitOfWork.Repository<MapDetail>().Delete(d);
            foreach (var lv in req.Levels.OrderBy(x => x.LevelOrder))
            {
                MapHintsExtractor.MergeHintsFromJson(lv);
                MapLevelMetadataExtractor.MergeFromJson(lv);
                if (lv.TimeLimitMs <= 0 || lv.WinCondition <= 0)
                    return Result.Failure(
                        "Each level requires TimeLimitMs and WinCondition > 0 (set in Levels[] or in each level JSON).",
                        ErrorCodeEnum.ValidationFailed);
                if (lv.Type == null)
                    return Result.Failure(
                        "Each level must declare map type: type or mapType (0|1 or Topdown|Platform) in level JSON root, on the wrapper next to jsonContent, or as type on each item in Levels[].",
                        ErrorCodeEnum.ValidationFailed);
                var detail = new MapDetail
                {
                    MapId = map.Id,
                    LevelOrder = lv.LevelOrder,
                    Title = lv.Title,
                    JsonContent = lv.JsonContent.GetRawText(),
                    TimeLimitMs = lv.TimeLimitMs,
                    WinCondition = lv.WinCondition,
                    Type = lv.Type.Value
                };
                detail.InitializeEntity(userId);
                await _unitOfWork.Repository<MapDetail>().AddAsync(detail);
                foreach (var h in lv.Hints.OrderBy(x => x.OrderNo))
                {
                    var hint = new Hint { MapDetailId = detail.Id, OrderNo = h.OrderNo, Content = h.Content };
                    hint.InitializeEntity(userId);
                    await _unitOfWork.Repository<Hint>().AddAsync(hint);
                }
            }
        }
        else if (req.MapDetailJson.HasValue)
        {
            var json = req.MapDetailJson.Value.GetRawText();
            var first = map.MapDetails.OrderBy(x => x.LevelOrder).FirstOrDefault();
            if (first == null)
            {
                var tmp = new MapLevelInputDto
                {
                    LevelOrder = 0,
                    JsonContent = req.MapDetailJson.Value,
                    Hints = req.Hints?.ToList() ?? new List<HintItemDto>()
                };
                MapHintsExtractor.MergeHintsFromJson(tmp);
                MapLevelMetadataExtractor.MergeFromJson(tmp);
                if (tmp.TimeLimitMs <= 0 || tmp.WinCondition <= 0)
                    return Result.Failure(
                        "Level requires TimeLimitMs and WinCondition > 0 (set in JSON or Levels when using multi-level API).",
                        ErrorCodeEnum.ValidationFailed);
                if (tmp.Type == null)
                    return Result.Failure(
                        "The level must declare map type: type or mapType (0|1 or Topdown|Platform) in JSON root or as type in Levels[].",
                        ErrorCodeEnum.ValidationFailed);
                var mapMap = new MapDetail
                {
                    MapId = map.Id,
                    LevelOrder = 0,
                    JsonContent = json,
                    TimeLimitMs = tmp.TimeLimitMs,
                    WinCondition = tmp.WinCondition,
                    Type = tmp.Type.Value
                };
                mapMap.InitializeEntity(userId);
                await _unitOfWork.Repository<MapDetail>().AddAsync(mapMap);
                foreach (var h in tmp.Hints.OrderBy(x => x.OrderNo))
                {
                    var hint = new Hint { MapDetailId = mapMap.Id, OrderNo = h.OrderNo, Content = h.Content };
                    hint.InitializeEntity(userId);
                    await _unitOfWork.Repository<Hint>().AddAsync(hint);
                }
            }
            else
            {
                first.JsonContent = json;
                first.UpdateEntity(userId);
                _unitOfWork.Repository<MapDetail>().Update(first);

                var tmp = new MapLevelInputDto
                {
                    LevelOrder = first.LevelOrder,
                    JsonContent = req.MapDetailJson.Value,
                    Hints = req.Hints?.ToList() ?? new List<HintItemDto>()
                };
                MapHintsExtractor.MergeHintsFromJson(tmp);
                MapLevelMetadataExtractor.MergeFromJson(tmp);
                if (tmp.TimeLimitMs <= 0 || tmp.WinCondition <= 0)
                    return Result.Failure(
                        "Level requires TimeLimitMs and WinCondition > 0 (set in JSON or Levels when using multi-level API).",
                        ErrorCodeEnum.ValidationFailed);
                if (tmp.Type == null)
                    return Result.Failure(
                        "The level must declare map type: type or mapType (0|1 or Topdown|Platform) in JSON root or as type in Levels[].",
                        ErrorCodeEnum.ValidationFailed);
                first.TimeLimitMs = tmp.TimeLimitMs;
                first.WinCondition = tmp.WinCondition;
                first.Type = tmp.Type.Value;
                foreach (var h in first.Hints.ToList()) _unitOfWork.Repository<Hint>().Delete(h);
                foreach (var h in tmp.Hints.OrderBy(x => x.OrderNo))
                {
                    var hint = new Hint { MapDetailId = first.Id, OrderNo = h.OrderNo, Content = h.Content };
                    hint.InitializeEntity(userId);
                    await _unitOfWork.Repository<Hint>().AddAsync(hint);
                }
            }
        }

        if (map.ContentVersion < 1)
            map.ContentVersion = 1;
        map.ContentVersion++;
        mapRepo.Update(map);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Map updated and moved back to Draft status.");
    }
}

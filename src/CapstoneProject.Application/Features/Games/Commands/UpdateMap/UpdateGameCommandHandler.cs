using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Games.Commands.UpdateMap;

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
            return Result.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để cập nhật bản đồ.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var mapRepo = _unitOfWork.Repository<Game>();
        var game = await mapRepo.GetQueryable()
            .Include(m => m.GameDetails).ThenInclude(d => d.Hints)
            .Include(m => m.GameTags)
            .FirstOrDefaultAsync(m => m.Id == command.GameId && !m.IsDeleted, cancellationToken);
        if (game == null)
            return Result.Failure($"Không tìm thấy bản đồ có Id: {command.GameId}.", ErrorCodeEnum.NotFound);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        bool isAdminOrMod = roles.Contains(RoleEnum.Admin) || roles.Contains(RoleEnum.Moderator);
        if (game.CreatedBy != userId && !isAdminOrMod)
            return Result.Failure("Bạn không có quyền cập nhật bản đồ này.", ErrorCodeEnum.Forbidden);

        if (game.GameStatus == GameStatusEnum.Approved || game.GameStatus == GameStatusEnum.Published)
            return Result.Failure(
                "Game đã được duyệt/xuất bản không thể sửa trực tiếp. Vui lòng tạo version mới từ bản hiện tại rồi gửi duyệt lại.",
                ErrorCodeEnum.InvalidOperation);

        var req = command.Request;
        game.Title = req.Title;
        game.Description = req.Description;
        game.Difficulty = req.Difficulty;
        game.Price = req.Price;
        if (req.FreeTrialAttemptLimit.HasValue)
            game.FreeTrialAttemptLimit = req.FreeTrialAttemptLimit.Value;
        game.EditorialContent = req.EditorialContent;
        if (req.UnlockEditorialAfterStars.HasValue)
            game.UnlockEditorialAfterStars = req.UnlockEditorialAfterStars.Value;
        if (req.LearnedTags != null)
            game.LearnedTags = req.LearnedTags;
        if (req.AvatarUrl != null)
            game.AvatarUrl = req.AvatarUrl;

        GameTypeEnum? requestedType = null;
        if (MapLevelMetadataExtractor.TryParseMapType(req.Type, out var parsedType))
            requestedType = parsedType;

        game.GameStatus = GameStatusEnum.Draft;
        game.IsPublished = false;
        game.UpdateEntity(userId);

        if (req.TagIds != null)
        {
            foreach (var mt in game.GameTags.ToList()) _unitOfWork.Repository<GameTag>().Delete(mt);
            foreach (var tagId in req.TagIds)
            {
                var mapTag = new GameTag { GameId = game.Id, TagId = tagId };
                mapTag.InitializeEntity(userId);
                await _unitOfWork.Repository<GameTag>().AddAsync(mapTag);
            }
        }

        if (req.Levels is { Count: > 0 })
        {
            if (requestedType.HasValue)
            {
                foreach (var level in req.Levels)
                {
                    if (level.Type == null)
                        level.Type = requestedType.Value;
                }
            }

            var incomingLevels = req.Levels.OrderBy(x => x.LevelOrder).ToList();
            var existingDetailsByLevelOrder = game.GameDetails
                .ToDictionary(x => x.LevelOrder, x => x);

            var incomingLevelOrders = incomingLevels
                .Select(x => x.LevelOrder)
                .ToHashSet();

            var removedDetails = game.GameDetails
                .Where(x => !incomingLevelOrders.Contains(x.LevelOrder))
                .ToList();

            if (removedDetails.Count > 0)
            {
                var removedDetailIds = removedDetails.Select(x => x.Id).ToList();
                var hasSubmissionOnRemovedLevels = await _unitOfWork.Repository<Submission>()
                    .GetQueryable()
                    .AnyAsync(x => x.GameDetailId.HasValue && removedDetailIds.Contains(x.GameDetailId.Value), cancellationToken);
                if (hasSubmissionOnRemovedLevels)
                    return Result.Failure(
                        "Không thể xoá level đã có bài nộp. Vui lòng giữ lại level đó hoặc tạo version mới.",
                        ErrorCodeEnum.InvalidOperation);

                foreach (var detailToDelete in removedDetails)
                    _unitOfWork.Repository<GameDetail>().Delete(detailToDelete);
            }

            foreach (var lv in incomingLevels)
            {
                MapHintsExtractor.MergeHintsFromJson(lv);
                MapLevelMetadataExtractor.MergeFromJson(lv);
                if (lv.TimeLimitMs <= 0 || lv.WinCondition <= 0)
                    return Result.Failure(
                        "Mỗi cấp độ yêu cầu TimeLimitMs và WinCondition > 0 (được đặt trong Levels[] hoặc trong JSON của mỗi cấp độ).",
                        ErrorCodeEnum.ValidationFailed);
                if (lv.Type == null)
                    return Result.Failure(
                        MapLevelMetadataExtractor.InvalidMapTypeMessage,
                        ErrorCodeEnum.ValidationFailed);

                if (existingDetailsByLevelOrder.TryGetValue(lv.LevelOrder, out var existingDetail))
                {
                    existingDetail.Title = lv.Title;
                    existingDetail.JsonContent = lv.JsonContent.GetRawText();
                    existingDetail.TimeLimitMs = lv.TimeLimitMs;
                    existingDetail.WinCondition = lv.WinCondition;
                    existingDetail.Type = lv.Type.Value;
                    existingDetail.UpdateEntity(userId);
                    _unitOfWork.Repository<GameDetail>().Update(existingDetail);

                    foreach (var existingHint in existingDetail.Hints.ToList())
                        _unitOfWork.Repository<Hint>().Delete(existingHint);
                    foreach (var h in lv.Hints.OrderBy(x => x.OrderNo))
                    {
                        var hint = new Hint { GameDetailId = existingDetail.Id, OrderNo = h.OrderNo, Content = h.Content };
                        hint.InitializeEntity(userId);
                        await _unitOfWork.Repository<Hint>().AddAsync(hint);
                    }

                    continue;
                }

                var detail = new GameDetail
                {
                    GameId = game.Id,
                    LevelOrder = lv.LevelOrder,
                    Title = lv.Title,
                    JsonContent = lv.JsonContent.GetRawText(),
                    TimeLimitMs = lv.TimeLimitMs,
                    WinCondition = lv.WinCondition,
                    Type = lv.Type.Value
                };
                detail.InitializeEntity(userId);
                await _unitOfWork.Repository<GameDetail>().AddAsync(detail);
                foreach (var h in lv.Hints.OrderBy(x => x.OrderNo))
                {
                    var hint = new Hint { GameDetailId = detail.Id, OrderNo = h.OrderNo, Content = h.Content };
                    hint.InitializeEntity(userId);
                    await _unitOfWork.Repository<Hint>().AddAsync(hint);
                }
            }
        }
        else if (req.GameDetailJson.HasValue)
        {
            var json = req.GameDetailJson.Value.GetRawText();
            var first = game.GameDetails.OrderBy(x => x.LevelOrder).FirstOrDefault();
            if (first == null)
            {
                var tmp = new MapLevelInputDto
                {
                    LevelOrder = 0,
                    JsonContent = req.GameDetailJson.Value,
                    Hints = req.Hints?.ToList() ?? new List<HintItemDto>()
                };
                if (requestedType.HasValue)
                    tmp.Type = requestedType.Value;
                MapHintsExtractor.MergeHintsFromJson(tmp);
                MapLevelMetadataExtractor.MergeFromJson(tmp);
                if (tmp.TimeLimitMs <= 0 || tmp.WinCondition <= 0)
                    return Result.Failure(
                        "Cấp độ yêu cầu TimeLimitMs và WinCondition > 0 (được đặt trong JSON hoặc Levels khi sử dụng API nhiều cấp).",
                        ErrorCodeEnum.ValidationFailed);
                if (tmp.Type == null)
                    return Result.Failure(
                        MapLevelMetadataExtractor.InvalidMapTypeMessage,
                        ErrorCodeEnum.ValidationFailed);
                var mapMap = new GameDetail
                {
                    GameId = game.Id,
                    LevelOrder = 0,
                    JsonContent = json,
                    TimeLimitMs = tmp.TimeLimitMs,
                    WinCondition = tmp.WinCondition,
                    Type = tmp.Type.Value
                };
                mapMap.InitializeEntity(userId);
                await _unitOfWork.Repository<GameDetail>().AddAsync(mapMap);
                foreach (var h in tmp.Hints.OrderBy(x => x.OrderNo))
                {
                    var hint = new Hint { GameDetailId = mapMap.Id, OrderNo = h.OrderNo, Content = h.Content };
                    hint.InitializeEntity(userId);
                    await _unitOfWork.Repository<Hint>().AddAsync(hint);
                }
            }
            else
            {
                first.JsonContent = json;
                first.UpdateEntity(userId);
                _unitOfWork.Repository<GameDetail>().Update(first);

                var tmp = new MapLevelInputDto
                {
                    LevelOrder = first.LevelOrder,
                    JsonContent = req.GameDetailJson.Value,
                    Hints = req.Hints?.ToList() ?? new List<HintItemDto>()
                };
                if (requestedType.HasValue)
                    tmp.Type = requestedType.Value;
                MapHintsExtractor.MergeHintsFromJson(tmp);
                MapLevelMetadataExtractor.MergeFromJson(tmp);
                if (tmp.TimeLimitMs <= 0 || tmp.WinCondition <= 0)
                    return Result.Failure(
                        "Cấp độ yêu cầu TimeLimitMs và WinCondition > 0 (được đặt trong JSON hoặc Levels khi sử dụng API nhiều cấp).",
                        ErrorCodeEnum.ValidationFailed);
                if (tmp.Type == null)
                    return Result.Failure(
                        MapLevelMetadataExtractor.InvalidMapTypeMessage,
                        ErrorCodeEnum.ValidationFailed);
                first.TimeLimitMs = tmp.TimeLimitMs;
                first.WinCondition = tmp.WinCondition;
                first.Type = tmp.Type.Value;
                foreach (var h in first.Hints.ToList()) _unitOfWork.Repository<Hint>().Delete(h);
                foreach (var h in tmp.Hints.OrderBy(x => x.OrderNo))
                {
                    var hint = new Hint { GameDetailId = first.Id, OrderNo = h.OrderNo, Content = h.Content };
                    hint.InitializeEntity(userId);
                    await _unitOfWork.Repository<Hint>().AddAsync(hint);
                }
            }
        }

        if (game.ContentVersion < 1)
            game.ContentVersion = 1;
        game.ContentVersion++;
        mapRepo.Update(game);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Bản đồ đã được cập nhật và chuyển về trạng thái Bản nháp.");
    }
}

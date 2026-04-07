using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateMap;

public class CreateMapCommandHandler : IRequestHandler<CreateMapCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICloudinaryService _cloudinaryService;

    public CreateMapCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<Guid>> Handle(CreateMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<Guid>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để tạo bản đồ.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var req = command.Request;
        var map = new Map
        {
            Title = req.Title,
            Description = req.Description,
            Difficulty = req.Difficulty,
            Price = req.Price,
            FreeTrialAttemptLimit = req.FreeTrialAttemptLimit,
            IsPublished = command.AutoPublish,
            MapStatus = command.AutoPublish ? MapStatusEnum.Published : MapStatusEnum.Draft,
            LearnedTags = req.LearnedTags,
            AvatarUrl = req.AvatarUrl,
            ContentVersion = 1
        };
        map.InitializeEntity(userId);

        var mapRepo = _unitOfWork.Repository<Map>();
        await mapRepo.AddAsync(map);

        var mapTagRepo = _unitOfWork.Repository<MapTag>();
        foreach (var tagId in req.TagIds)
        {
            var mapTag = new MapTag { MapId = map.Id, TagId = tagId };
            mapTag.InitializeEntity(userId);
            await mapTagRepo.AddAsync(mapTag);
        }

        var levelInputs = ResolveLevelInputs(req);
        if (MapLevelMetadataExtractor.TryParseMapType(req.Type, out var defaultType))
        {
            foreach (var lv in levelInputs)
            {
                if (lv.Type == null)
                    lv.Type = defaultType;
            }
        }
        foreach (var lv in levelInputs)
            MapHintsExtractor.MergeHintsFromJson(lv);
        MapLevelMetadataExtractor.MergeFromJson(levelInputs);
        foreach (var lv in levelInputs)
        {
            if (lv.TimeLimitMs <= 0 || lv.WinCondition <= 0)
                return Result<Guid>.Failure(
                    "Mỗi cấp độ yêu cầu TimeLimitMs và WinCondition > 0 (được đặt trong Levels[] hoặc timeLimitMs / winCondition trong JSON của mỗi cấp độ).",
                    ErrorCodeEnum.ValidationFailed);
            if (lv.Type == null)
                return Result<Guid>.Failure(
                    MapLevelMetadataExtractor.InvalidMapTypeMessage,
                    ErrorCodeEnum.ValidationFailed);
        }

        var hintRepo = _unitOfWork.Repository<Hint>();
        foreach (var lv in levelInputs)
        {
            var detail = new MapDetail
            {
                MapId = map.Id,
                LevelOrder = lv.LevelOrder,
                Title = lv.Title,
                JsonContent = lv.JsonContent.GetRawText(),
                TimeLimitMs = lv.TimeLimitMs,
                WinCondition = lv.WinCondition,
                Type = lv.Type!.Value
            };
            detail.InitializeEntity(userId);
            await _unitOfWork.Repository<MapDetail>().AddAsync(detail);

            foreach (var h in lv.Hints.OrderBy(x => x.OrderNo))
            {
                var hint = new Hint { MapDetailId = detail.Id, OrderNo = h.OrderNo, Content = h.Content };
                hint.InitializeEntity(userId);
                await hintRepo.AddAsync(hint);
            }
        }

        var galleryResult = await MapGalleryMediaHelper.StageGalleryMediaAsync(
            map.Id,
            userId,
            command.GalleryFiles,
            _unitOfWork,
            _cloudinaryService,
            requireAtLeastOneFile: false,
            cancellationToken);
        if (!galleryResult.IsSuccess)
        {
            var code = galleryResult.ErrorCode != null && Enum.TryParse<ErrorCodeEnum>(galleryResult.ErrorCode, out var ec)
                ? ec
                : ErrorCodeEnum.ValidationFailed;
            return Result<Guid>.Failure(galleryResult.Message ?? "Tải lên thư viện không thành công.", code);
        }

        var myMap = new MyMap { MapId = map.Id, UserId = userId, IsAuthor = true };
        myMap.InitializeEntity(userId);
        await _unitOfWork.Repository<MyMap>().AddAsync(myMap);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var message = command.AutoPublish
            ? "Map created and published successfully."
            : "Map created successfully.";
        return Result<Guid>.Success(map.Id, message);
    }

    static List<MapLevelInputDto> ResolveLevelInputs(CreateMapRequest req)
    {
        if (req.Levels is { Count: > 0 })
            return req.Levels.OrderBy(x => x.LevelOrder).ToList();
        if (req.MapDetailJson.HasValue)
            return new List<MapLevelInputDto>
            {
                new()
                {
                    LevelOrder = 0,
                    Title = null,
                    JsonContent = req.MapDetailJson.Value,
                    Hints = req.Hints.ToList()
                }
            };
        return new List<MapLevelInputDto>();
    }
}

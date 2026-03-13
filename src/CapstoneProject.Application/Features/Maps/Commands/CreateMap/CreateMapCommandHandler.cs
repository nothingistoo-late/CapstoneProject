using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using CapstoneProject.Domain.Common;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateMap;

public class CreateMapCommandHandler : IRequestHandler<CreateMapCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateMapCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<Guid>.Failure("Authentication required. Please log in to create a map.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var req = command.Request;
        var map = new Map
        {
            Title = req.Title,
            Description = req.Description,
            Difficulty = req.Difficulty,
            TimeLimitMs = req.TimeLimitMs,
            WinCondition = req.WinCondition,
            Type = req.Type ?? MapTypeEnum.Topdown,
            Price = req.Price,
            IsPublished = command.AutoPublish,
            MapStatus = command.AutoPublish ? MapStatusEnum.Published : MapStatusEnum.Draft,
            AvatarUrl = req.AvatarUrl
        };
        map.InitializeEntity(userId);

        var mapRepo = _unitOfWork.Repository<Map>();
        await mapRepo.AddAsync(map);
        var hintRepo = _unitOfWork.Repository<Hint>();
        foreach (var h in req.Hints.OrderBy(x => x.OrderNo))
        {
            var hint = new Hint { MapId = map.Id, OrderNo = h.OrderNo, Content = h.Content };
            hint.InitializeEntity(userId);
            await hintRepo.AddAsync(hint);
        }

        var mapTagRepo = _unitOfWork.Repository<MapTag>();
        foreach (var tagId in req.TagIds)
        {
            var mapTag = new MapTag { MapId = map.Id, TagId = tagId };
            mapTag.InitializeEntity(userId);
            await mapTagRepo.AddAsync(mapTag);
        }

        var mapMap = new MapDetail
        {
            MapId = map.Id,
            JsonContent = req.MapDetailJson.GetRawText()
        };
        mapMap.InitializeEntity(userId);
        await _unitOfWork.Repository<MapDetail>().AddAsync(mapMap);

        var myMap = new MyMap { MapId = map.Id, UserId = userId, IsAuthor = true };
        myMap.InitializeEntity(userId);
        await _unitOfWork.Repository<MyMap>().AddAsync(myMap);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var message = command.AutoPublish
            ? "Map created and published successfully."
            : "Map created successfully.";
        return Result<Guid>.Success(map.Id, message);
    }
}

using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Challenge;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using CapstoneProject.Domain.Common;

namespace CapstoneProject.Application.Features.Challenge.Commands.CreateMap;

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
            CreatedByUserId = userId,
            Title = req.Title,
            Description = req.Description,
            Difficulty = req.Difficulty,
            TimeLimitMs = req.TimeLimitMs,
            Price = req.Price,
            IsPublished = false,
            MapStatus = MapStatusEnum.Draft
        };
        map.InitializeEntity(userId);

        var mapRepo = _unitOfWork.Repository<Map>();
        await mapRepo.AddAsync(map);

        var spec = new MapSpec
        {
            MapId = map.Id,
            GridSpec = req.GridSpec,
            InitialStateSpec = req.InitialStateSpec,
            WinConditionSpec = req.WinConditionSpec,
            FailConditionSpec = req.FailConditionSpec,
            Version = 1
        };
        spec.InitializeEntity(userId);
        await _unitOfWork.Repository<MapSpec>().AddAsync(spec);

        var hintRepo = _unitOfWork.Repository<Hint>();
        foreach (var h in req.Hints.OrderBy(x => x.OrderNo))
        {
            var hint = new Hint { MapId = map.Id, OrderNo = h.OrderNo, Content = h.Content };
            hint.InitializeEntity(userId);
            await hintRepo.AddAsync(hint);
        }

        var constraintRepo = _unitOfWork.Repository<MapConstraint>();
        foreach (var c in req.Constraints)
        {
            var constraint = new MapConstraint { MapId = map.Id, Type = c.Type, Payload = c.Payload };
            constraint.InitializeEntity(userId);
            await constraintRepo.AddAsync(constraint);
        }

        var mapTagRepo = _unitOfWork.Repository<MapTag>();
        foreach (var tagId in req.TagIds)
        {
            var mapTag = new MapTag { MapId = map.Id, TagId = tagId };
            mapTag.InitializeEntity(userId);
            await mapTagRepo.AddAsync(mapTag);
        }

        var mapConceptRepo = _unitOfWork.Repository<MapConcept>();
        foreach (var conceptId in req.ConceptIds)
        {
            var mapConcept = new MapConcept { MapId = map.Id, ConceptId = conceptId };
            mapConcept.InitializeEntity(userId);
            await mapConceptRepo.AddAsync(mapConcept);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(map.Id, "Map created successfully.");
    }
}

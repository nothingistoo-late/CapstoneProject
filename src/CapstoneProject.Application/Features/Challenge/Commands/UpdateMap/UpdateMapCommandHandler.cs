using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Challenge;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Challenge.Commands.UpdateMap;

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
            .Include(m => m.Hints).Include(m => m.Constraints).Include(m => m.MapTags).Include(m => m.MapConcepts).Include(m => m.MapSpecs)
            .FirstOrDefaultAsync(m => m.Id == command.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result.Failure($"Map not found with Id: {command.MapId}. The map may have been deleted or does not exist.", ErrorCodeEnum.NotFound);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        bool isAdminOrMod = roles.Contains(RoleEnum.Admin) || roles.Contains(RoleEnum.Moderator);
        if (map.CreatedByUserId != userId && !isAdminOrMod)
            return Result.Failure("You do not have permission to update this map. Only the map author or Admin/Moderator can edit it.", ErrorCodeEnum.Forbidden);

        if (map.MapStatus != MapStatusEnum.Draft && !isAdminOrMod)
            return Result.Failure($"Map cannot be edited. Only draft maps can be edited by the author. Current status: {map.MapStatus}. Admin or Moderator can edit maps in any status.", ErrorCodeEnum.InvalidOperation);

        var req = command.Request;
        map.Title = req.Title;
        map.Description = req.Description;
        map.Difficulty = req.Difficulty;
        map.TimeLimitMs = req.TimeLimitMs;
        map.Price = req.Price;
        map.EditorialContent = req.EditorialContent;
        if (req.UnlockEditorialAfterStars.HasValue)
            map.UnlockEditorialAfterStars = req.UnlockEditorialAfterStars.Value;
        map.UpdateEntity(userId);

        if (req.GridSpec != null || req.InitialStateSpec != null || req.WinConditionSpec != null || req.FailConditionSpec != null)
        {
            var latestSpec = map.MapSpecs.OrderByDescending(s => s.Version).FirstOrDefault();
            if (latestSpec != null)
            {
                if (req.GridSpec != null) latestSpec.GridSpec = req.GridSpec;
                if (req.InitialStateSpec != null) latestSpec.InitialStateSpec = req.InitialStateSpec;
                if (req.WinConditionSpec != null) latestSpec.WinConditionSpec = req.WinConditionSpec;
                if (req.FailConditionSpec != null) latestSpec.FailConditionSpec = req.FailConditionSpec;
                latestSpec.UpdateEntity(userId);
                _unitOfWork.Repository<MapSpec>().Update(latestSpec);
            }
        }

        if (req.Hints != null)
        {
            foreach (var h in map.Hints.ToList())
                _unitOfWork.Repository<Hint>().Delete(h);
            foreach (var h in req.Hints.OrderBy(x => x.OrderNo))
            {
                var hint = new Hint { MapId = map.Id, OrderNo = h.OrderNo, Content = h.Content };
                hint.InitializeEntity(userId);
                await _unitOfWork.Repository<Hint>().AddAsync(hint);
            }
        }
        if (req.Constraints != null)
        {
            foreach (var c in map.Constraints.ToList())
                _unitOfWork.Repository<MapConstraint>().Delete(c);
            foreach (var c in req.Constraints)
            {
                var constraint = new MapConstraint { MapId = map.Id, Type = c.Type, Payload = c.Payload };
                constraint.InitializeEntity(userId);
                await _unitOfWork.Repository<MapConstraint>().AddAsync(constraint);
            }
        }
        if (req.TagIds != null)
        {
            foreach (var mt in map.MapTags.ToList())
                _unitOfWork.Repository<MapTag>().Delete(mt);
            foreach (var tagId in req.TagIds)
            {
                var mapTag = new MapTag { MapId = map.Id, TagId = tagId };
                mapTag.InitializeEntity(userId);
                await _unitOfWork.Repository<MapTag>().AddAsync(mapTag);
            }
        }
        if (req.ConceptIds != null)
        {
            foreach (var mc in map.MapConcepts.ToList())
                _unitOfWork.Repository<MapConcept>().Delete(mc);
            foreach (var conceptId in req.ConceptIds)
            {
                var mapConcept = new MapConcept { MapId = map.Id, ConceptId = conceptId };
                mapConcept.InitializeEntity(userId);
                await _unitOfWork.Repository<MapConcept>().AddAsync(mapConcept);
            }
        }

        mapRepo.Update(map);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Map updated.");
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
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
            .Include(m => m.MapDetail)
            .Include(m => m.Hints)
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
        map.TimeLimitMs = req.TimeLimitMs;
        map.WinCondition = req.WinCondition;
        if (req.Type.HasValue)
            map.Type = req.Type.Value;
        map.Price = req.Price;
        map.EditorialContent = req.EditorialContent;
        if (req.UnlockEditorialAfterStars.HasValue)
            map.UnlockEditorialAfterStars = req.UnlockEditorialAfterStars.Value;
        map.UpdateEntity(userId);

        if (req.Hints != null)
        {
            foreach (var h in map.Hints.ToList()) _unitOfWork.Repository<Hint>().Delete(h);
            foreach (var h in req.Hints.OrderBy(x => x.OrderNo))
            {
                var hint = new Hint { MapId = map.Id, OrderNo = h.OrderNo, Content = h.Content };
                hint.InitializeEntity(userId);
                await _unitOfWork.Repository<Hint>().AddAsync(hint);
            }
        }

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

        if (req.MapDetailJson.HasValue)
        {
            var json = req.MapDetailJson.Value.GetRawText();

            if (map.MapDetail == null)
            {
                var mapMap = new MapDetail { MapId = map.Id, JsonContent = json };
                mapMap.InitializeEntity(userId);
                await _unitOfWork.Repository<MapDetail>().AddAsync(mapMap);
            }
            else
            {
                map.MapDetail.JsonContent = json;
                map.MapDetail.UpdateEntity(userId);
                _unitOfWork.Repository<MapDetail>().Update(map.MapDetail);
            }
        }

        mapRepo.Update(map);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Map updated.");
    }
}

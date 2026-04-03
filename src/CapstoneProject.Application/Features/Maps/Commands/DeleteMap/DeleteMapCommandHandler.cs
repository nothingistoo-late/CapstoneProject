using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Commands.DeleteMap;

public class DeleteMapCommandHandler : IRequestHandler<DeleteMapCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteMapCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Authentication required. Please log in to delete a map.", ErrorCodeEnum.Unauthorized);

        var map = await _unitOfWork.Repository<Map>().GetQueryable()
            .FirstOrDefaultAsync(m => m.Id == command.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result.Failure($"Map not found with Id: {command.MapId}. The map may have been deleted or does not exist.", ErrorCodeEnum.NotFound);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        var isAdminOrMod = roles.Contains(RoleEnum.Admin) || roles.Contains(RoleEnum.Moderator);
        if (map.CreatedBy != userIdNullable.Value && !isAdminOrMod)
            return Result.Failure("You do not have permission to delete this map. Only the map author or Admin/Moderator can delete it.", ErrorCodeEnum.Forbidden);

        map.IsPublished = false;
        if (map.MapStatus == MapStatusEnum.Published)
            map.MapStatus = MapStatusEnum.Approved;

        map.SoftDeleteEntity(userIdNullable.Value);
        _unitOfWork.Repository<Map>().Update(map);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Map deleted successfully.");
    }
}

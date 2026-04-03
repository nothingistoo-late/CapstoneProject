using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Commands.DeleteMapGalleryMedia;

public class DeleteMapGalleryMediaCommandHandler : IRequestHandler<DeleteMapGalleryMediaCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICloudinaryService _cloudinaryService;

    public DeleteMapGalleryMediaCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result> Handle(DeleteMapGalleryMediaCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var map = await _unitOfWork.Repository<Map>().GetQueryable()
            .FirstOrDefaultAsync(m => m.Id == request.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result.Failure("Map not found.", ErrorCodeEnum.NotFound);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        var isAdminOrMod = roles.Contains(RoleEnum.Admin) || roles.Contains(RoleEnum.Moderator);
        if (map.CreatedBy != userId && !isAdminOrMod)
            return Result.Failure("You do not have permission to update this map.", ErrorCodeEnum.Forbidden);

        var media = await _unitOfWork.Repository<MapMedia>().GetQueryable()
            .FirstOrDefaultAsync(m => m.Id == request.MediaId && m.MapId == request.MapId, cancellationToken);
        if (media == null)
            return Result.Failure("Gallery item not found.", ErrorCodeEnum.NotFound);

        var url = media.Url;
        var kind = media.Kind;
        _unitOfWork.Repository<MapMedia>().Delete(media);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _ = Task.Run(async () =>
        {
            try
            {
                var publicId = _cloudinaryService.GetPublicIdFromUrl(url);
                if (publicId != null)
                    await _cloudinaryService.DeleteAsync(publicId, kind, CancellationToken.None);
            }
            catch
            {
                /* ignore cleanup failure */
            }
        });

        return Result.Success("Gallery item removed.");
    }
}

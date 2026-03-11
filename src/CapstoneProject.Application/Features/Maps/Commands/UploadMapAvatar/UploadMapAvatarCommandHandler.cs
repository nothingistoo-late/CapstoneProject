using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Commands.UploadMapAvatar;

public class UploadMapAvatarCommandHandler : IRequestHandler<UploadMapAvatarCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICloudinaryService _cloudinaryService;

    public UploadMapAvatarCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<string>> Handle(UploadMapAvatarCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<string>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var map = await _unitOfWork.Repository<Map>().GetQueryable()
            .FirstOrDefaultAsync(m => m.Id == request.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result<string>.Failure("Map not found.", ErrorCodeEnum.NotFound);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        var isAdminOrMod = roles.Contains(RoleEnum.Admin) || roles.Contains(RoleEnum.Moderator);
        if (map.CreatedBy != userId && !isAdminOrMod)
            return Result<string>.Failure("You do not have permission to update this map's avatar.", ErrorCodeEnum.Forbidden);

        var avatarUrl = await _cloudinaryService.UploadImageAsync(
            request.AvatarFile,
            "maps",
            $"map_{request.MapId:N}",
            cancellationToken);
        if (string.IsNullOrEmpty(avatarUrl))
            return Result<string>.Failure("Upload avatar failed.", ErrorCodeEnum.FileUploadFailed);

        var oldUrl = map.AvatarUrl;
        map.AvatarUrl = avatarUrl;
        map.UpdateEntity(userId.Value);
        _unitOfWork.Repository<Map>().Update(map);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _ = Task.Run(async () =>
        {
            if (!string.IsNullOrEmpty(oldUrl))
            {
                try
                {
                    var publicId = _cloudinaryService.GetPublicIdFromUrl(oldUrl);
                    if (publicId != null)
                        await _cloudinaryService.DeleteAsync(publicId, cancellationToken);
                }
                catch { /* ignore */ }
            }
        });

        return Result<string>.Success(avatarUrl, "Map avatar updated.");
    }
}

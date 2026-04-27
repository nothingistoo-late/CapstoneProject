using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Games.Commands.DeleteMapGalleryMedia;

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
            return Result.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var game = await _unitOfWork.Repository<Game>().GetQueryable()
            .FirstOrDefaultAsync(m => m.Id == request.GameId && !m.IsDeleted, cancellationToken);
        if (game == null)
            return Result.Failure("Trò chơi không được tìm thấy.", ErrorCodeEnum.NotFound);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        var isAdminOrMod = roles.Contains(RoleEnum.Admin) || roles.Contains(RoleEnum.Moderator);
        if (game.CreatedBy != userId && !isAdminOrMod)
            return Result.Failure("Bạn không có quyền cập nhật trò chơi này.", ErrorCodeEnum.Forbidden);

        var media = await _unitOfWork.Repository<GameMedia>().GetQueryable()
            .FirstOrDefaultAsync(m => m.Id == request.MediaId && m.GameId == request.GameId, cancellationToken);
        if (media == null)
            return Result.Failure("Không tìm thấy mục thư viện.", ErrorCodeEnum.NotFound);

        var url = media.Url;
        var kind = media.Kind;
        _unitOfWork.Repository<GameMedia>().Delete(media);
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

        return Result.Success("Mục thư viện đã bị xóa.");
    }
}

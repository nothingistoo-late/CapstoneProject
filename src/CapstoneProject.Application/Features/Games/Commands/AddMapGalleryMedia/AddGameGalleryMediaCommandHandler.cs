using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Games.Commands.AddMapGalleryMedia;

public class AddMapGalleryMediaCommandHandler : IRequestHandler<AddMapGalleryMediaCommand, Result<List<GameMediaItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICloudinaryService _cloudinaryService;

    public AddMapGalleryMediaCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<List<GameMediaItemDto>>> Handle(AddMapGalleryMediaCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<List<GameMediaItemDto>>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var game = await _unitOfWork.Repository<Game>().GetQueryable()
            .FirstOrDefaultAsync(m => m.Id == request.GameId && !m.IsDeleted, cancellationToken);
        if (game == null)
            return Result<List<GameMediaItemDto>>.Failure("Trò chơi không được tìm thấy.", ErrorCodeEnum.NotFound);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        var isAdminOrMod = roles.Contains(RoleEnum.Admin) || roles.Contains(RoleEnum.Moderator);
        if (game.CreatedBy != userId && !isAdminOrMod)
            return Result<List<GameMediaItemDto>>.Failure("Bạn không có quyền cập nhật trò chơi này.", ErrorCodeEnum.Forbidden);

        var staged = await MapGalleryMediaHelper.StageGalleryMediaAsync(
            request.GameId,
            userId.Value,
            request.Files,
            _unitOfWork,
            _cloudinaryService,
            requireAtLeastOneFile: true,
            cancellationToken);
        if (!staged.IsSuccess)
            return staged;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<List<GameMediaItemDto>>.Success(staged.Data ?? new List<GameMediaItemDto>(), "Gallery media added.");
    }
}
